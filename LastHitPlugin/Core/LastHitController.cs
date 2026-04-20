using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace LastHitPlugin.Core;

internal sealed class LastHitController : IDisposable
{
    private readonly Configuration config;

    public LastHitController(Configuration config)
    {
        this.config = config;
        Svc.Framework.Update += OnTick;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnTick;
    }

    private void OnTick(IFramework _)
    {
        if (!config.Enabled) return;
        if (!Player.Available) return;
        if (!EzThrottler.Throttle("LastHit.Tick", 100)) return;

        var target = ResolveTarget();
        if (target == null) return;

        var hpPct = target.MaxHp == 0 ? 0f : 100f * target.CurrentHp / target.MaxHp;
        if (hpPct >= config.HpThresholdPercent) return;

        var module = JobModuleRegistry.For(Player.Object!.ClassJob.RowId);
        if (module == null) return;

        if (!EzThrottler.Throttle("LastHit.Fire", 1500)) return;
        ActionExec.TryUse(module.GetLimitBreakActionId());
    }

    private IBattleChara? ResolveTarget()
    {
        if (Svc.Targets.Target is IBattleChara manual && !manual.IsDead)
            return manual;
        if (!config.AutoSelectLowestHp) return null;
        return TargetSelector.PickLowestHp(config.AutoSelectRangeYalms);
    }
}
