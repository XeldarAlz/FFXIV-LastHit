using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class EmptyCard
{
    public static void Draw(string id, string message, FontAwesomeIcon icon)
    {
        var iconText = icon.ToIconString();
        float iconWidth;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconWidth = ImGui.CalcTextSize(iconText).X;

        var style = ImGui.GetStyle();
        var cardPadX = 10f * ImGuiHelpers.GlobalScale;
        var cardPadY = 8f * ImGuiHelpers.GlobalScale;
        var avail = ImGui.GetContentRegionAvail().X;
        var textWrapWidth = MathF.Max(40f, avail - cardPadX * 2 - iconWidth - style.ItemSpacing.X);
        var textSize = ImGui.CalcTextSize(message, false, textWrapWidth);

        var height = MathF.Max(
            textSize.Y + cardPadY * 2 + 4f * ImGuiHelpers.GlobalScale,
            44f * ImGuiHelpers.GlobalScale);

        using (Card.Begin($"##empty_{id}", height, Styling.CardBgSoft, Styling.CardBorderDim))
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextMuted))
                ImGui.TextUnformatted(iconText);
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            {
                ImGui.PushTextWrapPos(0f);
                ImGui.TextUnformatted(message);
                ImGui.PopTextWrapPos();
            }
        }
    }
}
