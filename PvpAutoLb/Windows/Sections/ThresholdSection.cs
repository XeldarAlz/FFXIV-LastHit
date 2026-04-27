using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class ThresholdSection
{
    private const uint SamplePreviewMaxHp = 75_000u;

    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Threshold");

        using (Card.Begin("##threshold", 148f * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            DrawModeToggle(cfg);
            ImGui.Spacing();
            DrawValueControl(cfg);
            ImGui.Spacing();
            DrawPreview(cfg);
        }
    }

    private static void DrawModeToggle(Configuration cfg)
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var half = (avail - ImGui.GetStyle().ItemSpacing.X) / 2f;
        var size = new Vector2(half, 28f * ImGuiHelpers.GlobalScale);

        if (SegmentedControl.DrawSegment("Percent of max", "thresh_pct", cfg.ThresholdMode == ThresholdMode.Percent, size))
        {
            cfg.ThresholdMode = ThresholdMode.Percent;
            cfg.Save();
        }
        ImGui.SameLine();
        if (SegmentedControl.DrawSegment("Absolute HP", "thresh_abs", cfg.ThresholdMode == ThresholdMode.Absolute, size))
        {
            cfg.ThresholdMode = ThresholdMode.Absolute;
            cfg.Save();
        }
    }

    private static void DrawValueControl(Configuration cfg)
    {
        ImGui.SetNextItemWidth(-1);
        if (cfg.ThresholdMode == ThresholdMode.Percent)
        {
            var pct = cfg.HpThresholdPercent;
            if (ImGui.SliderFloat("##pct", ref pct, 1f, 99f, "%.0f%% of max HP"))
            {
                cfg.HpThresholdPercent = pct;
                cfg.SaveDebounced();
            }
        }
        else
        {
            var abs = (int)cfg.HpThresholdAbsolute;
            if (ImGui.DragInt("##abs", ref abs, 100f, 1, 500_000, "%d HP"))
            {
                cfg.HpThresholdAbsolute = (uint)Math.Max(1, abs);
                cfg.SaveDebounced();
            }
        }
    }

    private static void DrawPreview(Configuration cfg)
    {
        var thresholdFrac = cfg.ThresholdMode == ThresholdMode.Percent
            ? Math.Clamp(cfg.HpThresholdPercent / 100f, 0.01f, 0.99f)
            : Math.Clamp((float)cfg.HpThresholdAbsolute / SamplePreviewMaxHp, 0.01f, 0.99f);

        var barHeight = 18f * ImGuiHelpers.GlobalScale;
        using (ImRaii.PushColor(ImGuiCol.PlotHistogram, Styling.AccentGreen))
        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.06f, 0.07f, 0.08f, 0.90f)))
            ImGui.ProgressBar(1.0f, new Vector2(-1, barHeight), "preview");

        var rectMin = ImGui.GetItemRectMin();
        var rectMax = ImGui.GetItemRectMax();
        var x = rectMin.X + (rectMax.X - rectMin.X) * thresholdFrac;
        var draw = ImGui.GetWindowDrawList();
        var overlay = ImGui.GetColorU32(new Vector4(Styling.AccentRed.X, Styling.AccentRed.Y, Styling.AccentRed.Z, 0.32f));
        draw.AddRectFilled(rectMin, new Vector2(x, rectMax.Y), overlay, 3f);
        draw.AddLine(new Vector2(x, rectMin.Y - 2), new Vector2(x, rectMax.Y + 2),
            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.90f)), 1.8f);

        ImGui.Spacing();
        var label = cfg.ThresholdMode == ThresholdMode.Percent
            ? $"Fires when target drops below {cfg.HpThresholdPercent:F0}% of its max HP."
            : $"Fires when target drops below {cfg.HpThresholdAbsolute:N0} HP.";
        using (ImRaii.PushFont(UiBuilder.IconFont))
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentViolet))
            ImGui.TextUnformatted(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
            ImGui.TextUnformatted(label);
    }
}
