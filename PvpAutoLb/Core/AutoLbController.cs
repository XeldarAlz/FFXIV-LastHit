using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace PvpAutoLb.Core;

internal sealed class AutoLbController : IDisposable
{
    private const string TickThrottleKey = "PvpAutoLb.Tick";
    private const string FireThrottleKeyPrefix = "PvpAutoLb.Fire.";

    private readonly Configuration config;
    private readonly Dictionary<uint, string> fireKeyCache = new();

    private ulong? userOriginalTargetId;
    private ulong? lastOurSwapTargetId;
    private DateTime lastSwapAtUtc;

    public HpTracker HpTracker { get; } = new();
    public SessionStats Stats { get; }

    public DateTime? LastFiredUtc { get; private set; }
    public IBattleChara? LastResolvedTarget { get; private set; }
    public LbTargetingProfile LastProfile { get; private set; } = LbTargetingProfile.None;
    public int LastEnemiesAffected { get; private set; }

    public AutoLbController(Configuration config)
    {
        this.config = config;
        Stats = new SessionStats(config);
        Svc.Framework.Update += OnTick;
        Svc.Log.Info("[PvpAutoLb] controller online — build " + typeof(AutoLbController).Assembly.GetName().Version);
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnTick;
    }

    private void OnTick(IFramework _)
    {
        // The framework dispatcher swallows handler exceptions in some Dalamud
        // builds, so we wrap and log to keep state observable from /xllog.
        try
        {
            Tick();
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "[PvpAutoLb] tick failed");
        }
    }

    private void Tick()
    {
        Stats.Tick();
        TryRestoreUserTarget();

        if (!config.Enabled) { ClearState(); return; }
        if (!Player.Available) { ClearState(); return; }
        if (!IsDutyAllowed()) { ClearState(); return; }
        if (!EzThrottler.Throttle(TickThrottleKey, PvpAutoLbConstants.TickThrottleMs)) return;

        var jobId = Player.Object!.ClassJob.RowId;
        var profile = LbCatalog.ResolveProfile(jobId);
        LastProfile = profile;
        if (profile.ActionId == 0) { ClearState(); return; }

        var hostiles = TargetSelector.ScanHostiles(config.AutoSelectRangeYalms);
        for (var i = 0; i < hostiles.Count; i++) HpTracker.Sample(hostiles[i]);

        if (config.AutoSelectLowestHp)
        {
            var decision = FireDecisionMaker.Decide(profile, config, hostiles, HpTracker);
            LastResolvedTarget = decision?.HardTarget;
            LastEnemiesAffected = decision?.EnemiesAffected ?? 0;
            if (decision == null) return;
            if (IsBlocklisted(decision.HardTarget)) return;
            TryFire(jobId, decision.HardTarget);
        }
        else
        {
            if (Svc.Targets.Target is not IBattleChara manual || manual.IsDead)
            {
                LastResolvedTarget = null;
                LastEnemiesAffected = 0;
                return;
            }
            LastResolvedTarget = manual;
            if (!HpMath.IsBelowThreshold(manual, config, jobId))
            {
                LastEnemiesAffected = 0;
                return;
            }
            if (IsBlocklisted(manual)) { LastEnemiesAffected = 0; return; }
            LastEnemiesAffected = 1;
            TryFire(jobId, manual);
        }
    }

    private void TryFire(uint jobId, IBattleChara target)
    {
        var actionIds = LbCatalog.ResolveActionIds(jobId);
        if (actionIds.Count == 0) return;

        var targetId = (ulong)target.EntityId;
        foreach (var actionId in actionIds)
        {
            if (!ActionExec.IsReady(actionId, targetId)) continue;
            if (!EzThrottler.Throttle(GetFireThrottleKey(actionId), PvpAutoLbConstants.FireThrottleMs)) continue;

            SwapHardTarget(target);

            if (ActionExec.TryUse(actionId, targetId))
            {
                LastFiredUtc = DateTime.UtcNow;
                Stats.RecordFire(target, LastEnemiesAffected);
                Feedback.OnFire(config, target, ActionName(actionId));
                Svc.Log.Info($"[PvpAutoLb] fired {actionId} on 0x{target.EntityId:X} (caught {LastEnemiesAffected})");
                return;
            }
        }
    }

    // Saves the user's pre-swap target the first time we yank it during a fire
    // window, so TryRestoreUserTarget can put them back after the LB lands.
    private void SwapHardTarget(IBattleChara target)
    {
        var current = Svc.Targets.Target;
        if (current?.EntityId == target.EntityId) return;

        if (userOriginalTargetId == null && current is IBattleChara prev)
            userOriginalTargetId = prev.EntityId;

        Svc.Targets.Target = target;
        lastOurSwapTargetId = target.EntityId;
        lastSwapAtUtc = DateTime.UtcNow;
    }

    private void TryRestoreUserTarget()
    {
        if (userOriginalTargetId == null) return;
        if ((DateTime.UtcNow - lastSwapAtUtc).TotalMilliseconds < PvpAutoLbConstants.TargetRestoreDelayMs) return;

        // If the user has manually moved off our pick, respect their choice
        // and stop tracking — don't snap them back.
        if (Svc.Targets.Target?.EntityId != lastOurSwapTargetId)
        {
            ClearRestoreState();
            return;
        }

        var original = Svc.Objects.SearchById(userOriginalTargetId.Value);
        if (original is IBattleChara b && !b.IsDead)
            Svc.Targets.Target = b;
        else
            Svc.Targets.Target = null;

        ClearRestoreState();
    }

    private void ClearRestoreState()
    {
        userOriginalTargetId = null;
        lastOurSwapTargetId = null;
    }

    private static string ActionName(uint actionId)
    {
        var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
        return sheet?.GetRowOrDefault(actionId)?.Name.ToString() ?? $"action {actionId}";
    }

    private bool IsDutyAllowed()
    {
        var current = DutyDetector.Current();
        if (current == DutyMask.None) return true;
        return (config.EnabledDuties & current) != 0;
    }

    private bool IsBlocklisted(IBattleChara target)
    {
        if (config.NameBlocklist.Count == 0) return false;
        var name = target.Name.TextValue;
        for (var i = 0; i < config.NameBlocklist.Count; i++)
        {
            if (string.Equals(config.NameBlocklist[i], name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private string GetFireThrottleKey(uint actionId)
    {
        if (!fireKeyCache.TryGetValue(actionId, out var key))
        {
            key = FireThrottleKeyPrefix + actionId;
            fireKeyCache[actionId] = key;
        }
        return key;
    }

    private void ClearState()
    {
        LastResolvedTarget = null;
        LastEnemiesAffected = 0;
    }
}
