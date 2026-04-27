using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class SegmentedControl
{
    private static readonly Vector4 ActiveBg = new(0.25f, 0.40f, 0.65f, 0.90f);
    private static readonly Vector4 IdleBg = new(0.20f, 0.21f, 0.24f, 0.60f);

    public static bool DrawSegment(string label, string id, bool active, Vector2 size)
    {
        var bg = active ? ActiveBg : IdleBg;
        var bgHover = bg + new Vector4(0.08f, 0.08f, 0.08f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.Text, active ? Styling.TextStrong : Styling.TextSecondary))
        {
            return ImGui.Button(label + "##" + id, size) && !active;
        }
    }
}
