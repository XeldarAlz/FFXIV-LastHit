using System;
using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaActionCategory = Lumina.Excel.Sheets.ActionCategory;
using LuminaClassJob = Lumina.Excel.Sheets.ClassJob;

namespace PvpAutoLb.Core;

internal static class LbCatalog
{
    private static readonly uint[] EmptyIds = Array.Empty<uint>();

    // Job ids whose PvP LB is defensive or support-focused. Tagged here rather
    // than detected, because Lumina exposes "self-target" but not "intent",
    // and several offensive LBs also self-target during the cast.
    private static readonly HashSet<uint> SupportJobs = new()
    {
        19, // PLD — Phalanx
        21, // WAR — Primal Scream
        23, // BRD — Final Fantasia
        25, // BLM — Soul Resonance
        33, // AST — Celestial River
        38, // DNC — Contradance
        39, // RPR — Tenebrae Lemurum
        40, // SGE — Mesotes
        42, // PCT — Advent of Chocobastion
    };

    private static Dictionary<uint, uint[]>? actionsByJob;
    private static readonly Dictionary<uint, LbTargetingProfile> ProfileCache = new();

    public static IReadOnlyList<uint> ResolveActionIds(uint classJobId)
    {
        EnsureLoaded();
        return actionsByJob!.TryGetValue(classJobId, out var ids) ? ids : EmptyIds;
    }

    public static uint ResolveActionId(uint classJobId)
    {
        var list = ResolveActionIds(classJobId);
        return list.Count > 0 ? list[0] : 0u;
    }

    public static LbTargetingProfile ResolveProfile(uint classJobId)
    {
        var actionId = ResolveActionId(classJobId);
        if (actionId == 0) return LbTargetingProfile.None;
        if (!ProfileCache.TryGetValue(actionId, out var p))
        {
            p = LbTargetingProfile.FromAction(actionId);
            ProfileCache[actionId] = p;
        }
        return p;
    }

    public static LbKind Classify(uint classJobId)
        => SupportJobs.Contains(classJobId) ? LbKind.Support : LbKind.Offensive;

    private static void EnsureLoaded()
    {
        if (actionsByJob != null) return;
        actionsByJob = BuildIndex();
        LogResolvedMap();
        if (actionsByJob.Count == 0) DumpDiagnostics();
    }

    private static Dictionary<uint, uint[]> BuildIndex()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) return new Dictionary<uint, uint[]>();

        var primaries = sheet
            .Where(a => a.IsPvP)
            .Where(a => a.ActionCategory.RowId == PvpAutoLbConstants.LimitBreakCategoryId)
            .Where(a => a.ClassJob.RowId != 0 && a.ClassJob.RowId != uint.MaxValue)
            .GroupBy(a => a.ClassJob.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToList());

        var followUpsByParent = sheet
            .Where(a => a.IsPvP && a.ActionCombo.RowId != 0)
            .GroupBy(a => a.ActionCombo.RowId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.RowId).ToArray());

        var result = new Dictionary<uint, uint[]>(primaries.Count);
        foreach (var (jobId, primaryIds) in primaries)
        {
            var ordered = new List<uint>(primaryIds);
            var seen = new HashSet<uint>(primaryIds);
            var queue = new Queue<uint>(primaryIds);
            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                if (!followUpsByParent.TryGetValue(parent, out var followUps)) continue;
                foreach (var id in followUps)
                {
                    if (seen.Add(id))
                    {
                        ordered.Add(id);
                        queue.Enqueue(id);
                    }
                }
            }
            result[jobId] = ordered.ToArray();
        }
        return result;
    }

    private static void LogResolvedMap()
    {
        Svc.Log.Info($"[PvpAutoLb] resolved PvP LBs for {actionsByJob!.Count} jobs");
        var jobs = Svc.Data.GetExcelSheet<LuminaClassJob>();
        var actions = Svc.Data.GetExcelSheet<LuminaAction>();
        foreach (var (jobId, ids) in actionsByJob.OrderBy(kv => kv.Key))
        {
            var jobName = jobs?.GetRowOrDefault(jobId)?.Abbreviation.ToString() ?? $"Job{jobId}";
            var parts = string.Join(", ", ids.Select(id =>
            {
                var name = actions?.GetRowOrDefault(id)?.Name.ToString() ?? $"Action{id}";
                return $"{id} ({name})";
            }));
            Svc.Log.Debug($"[PvpAutoLb]   {jobId} {jobName} -> [{parts}]");
        }
    }

    private static void DumpDiagnostics()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) { Svc.Log.Error("[PvpAutoLb] diag: action sheet is null"); return; }

        var categories = Svc.Data.GetExcelSheet<LuminaActionCategory>();
        Svc.Log.Info("[PvpAutoLb] diag: IsPvP actions by ActionCategory:");
        foreach (var g in sheet.Where(a => a.IsPvP).GroupBy(a => a.ActionCategory.RowId).OrderByDescending(g => g.Count()))
        {
            var name = categories?.GetRowOrDefault(g.Key)?.Name.ToString() ?? "?";
            Svc.Log.Info($"[PvpAutoLb]   cat={g.Key} ({name}) count={g.Count()}");
        }
    }
}
