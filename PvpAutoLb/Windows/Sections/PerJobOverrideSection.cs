using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class PerJobOverrideSection
{
    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Per-job override");

        var jobId = JobLookup.CurrentJobId;
        if (jobId == 0)
        {
            DrawOfflineCard();
            return;
        }

        var jobName = JobLookup.Name(jobId);
        var hasOverride = cfg.HasJobOverride(jobId);
        var height = hasOverride ? 168f : 60f;

        using (Card.Begin("##joboverride", height * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            if (ImGui.Checkbox($"Override for {jobName}", ref hasOverride))
            {
                if (hasOverride) cfg.EnsureJobOverride(jobId);
                else cfg.ClearJobOverride(jobId);
                cfg.Save();
            }
            Tooltip.OnHover("When on, this job uses its own threshold instead of the global one.");

            if (!cfg.HasJobOverride(jobId)) return;

            ImGui.Spacing();
            DrawControls(cfg, jobId);
        }
    }

    private static void DrawOfflineCard()
    {
        using (Card.Begin("##joboverride_off", 56f * ImGuiHelpers.GlobalScale, Styling.CardBgSoft, Styling.CardBorderDim))
        {
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted("Log into a job to set a per-job override.");
        }
    }

    private static void DrawControls(Configuration cfg, uint jobId)
    {
        var j = cfg.EnsureJobOverride(jobId);
        var avail = ImGui.GetContentRegionAvail().X;
        var half = (avail - ImGui.GetStyle().ItemSpacing.X) / 2f;
        var size = new Vector2(half, 26f * ImGuiHelpers.GlobalScale);

        if (SegmentedControl.DrawSegment("Percent of max", "job_pct", j.Mode == ThresholdMode.Percent, size))
        {
            j.Mode = ThresholdMode.Percent;
            cfg.Save();
        }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Absolute HP", "job_abs", j.Mode == ThresholdMode.Absolute, size))
        {
            j.Mode = ThresholdMode.Absolute;
            cfg.Save();
        }

        ImGui.Spacing();
        ImGui.SetNextItemWidth(-1);

        if (j.Mode == ThresholdMode.Percent)
        {
            var pct = j.Percent;
            if (ImGui.SliderFloat("##jobpct", ref pct, 1f, 99f, "%.0f%% of max HP"))
            {
                j.Percent = pct;
                cfg.SaveDebounced();
            }
        }
        else
        {
            var abs = (int)j.Absolute;
            if (ImGui.DragInt("##jobabs", ref abs, 100f, 1, 500_000, "%d HP"))
            {
                j.Absolute = (uint)Math.Max(1, abs);
                cfg.SaveDebounced();
            }
        }
    }
}
