using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using LastHitPlugin.Core;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace LastHitPlugin.Jobs;

internal sealed class DefaultPvpLimitBreakModule : IJobLimitBreakModule
{
    private const uint LimitBreakCategoryId = 9;

    private Dictionary<uint, uint>? cache;

    public bool TryResolve(uint classJobId, out uint actionId)
    {
        cache ??= BuildCache();
        return cache.TryGetValue(classJobId, out actionId);
    }

    private static Dictionary<uint, uint> BuildCache()
    {
        var sheet = Svc.Data.GetExcelSheet<LuminaAction>();
        if (sheet == null) return new Dictionary<uint, uint>();

        return sheet
            .Where(a => a.IsPvP)
            .Where(a => a.ActionCategory.RowId == LimitBreakCategoryId)
            .Where(a => a.ClassJob.RowId != 0)
            .GroupBy(a => a.ClassJob.RowId)
            .ToDictionary(g => g.Key, g => g.First().RowId);
    }
}
