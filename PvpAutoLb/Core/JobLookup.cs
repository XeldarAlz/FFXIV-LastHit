using ECommons.DalamudServices;
using ECommons.GameHelpers;
using LuminaClassJob = Lumina.Excel.Sheets.ClassJob;

namespace PvpAutoLb.Core;

internal static class JobLookup
{
    public static uint CurrentJobId => Player.Available ? Player.Object!.ClassJob.RowId : 0u;

    public static string Abbreviation(uint jobId)
    {
        if (jobId == 0) return string.Empty;
        var sheet = Svc.Data.GetExcelSheet<LuminaClassJob>();
        return sheet?.GetRowOrDefault(jobId)?.Abbreviation.ToString() ?? string.Empty;
    }

    public static string Name(uint jobId)
    {
        if (jobId == 0) return "?";
        var sheet = Svc.Data.GetExcelSheet<LuminaClassJob>();
        return sheet?.GetRowOrDefault(jobId)?.Name.ToString() ?? $"Job {jobId}";
    }
}
