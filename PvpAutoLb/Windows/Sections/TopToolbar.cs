using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Sections;

internal static class TopToolbar
{
    public static void Draw(Plugin plugin)
    {
        ImGui.AlignTextToFramePadding();
        var jobName = JobLookup.Abbreviation(JobLookup.CurrentJobId);
        var hasJob = !string.IsNullOrEmpty(jobName);
        using (ImRaii.PushColor(ImGuiCol.Text, hasJob ? Styling.TextDim : Styling.TextMuted))
            ImGui.TextUnformatted(hasJob ? jobName : "offline");

        var infoLabel = FontAwesomeIcon.InfoCircle.ToIconString();
        var gearLabel = FontAwesomeIcon.Cog.ToIconString();
        bool infoClicked, gearClicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var framePadX = ImGui.GetStyle().FramePadding.X;
            var spacingX = ImGui.GetStyle().ItemSpacing.X;
            var gearW = ImGui.CalcTextSize(gearLabel).X + framePadX * 2;
            var infoW = ImGui.CalcTextSize(infoLabel).X + framePadX * 2;
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - gearW - infoW - spacingX);
            infoClicked = ImGui.Button(infoLabel + "##about");
            ImGui.SameLine();
            gearClicked = ImGui.Button(gearLabel + "##gear");
        }
        if (infoClicked) plugin.ToggleAboutUi();
        if (gearClicked) plugin.ToggleConfigUi();

        ImGui.Separator();
    }
}
