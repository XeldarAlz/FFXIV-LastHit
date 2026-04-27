using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class TargetSection
{
    public static void Draw(Configuration cfg, AutoLbController ctrl, LbDrawState state)
    {
        Styling.SectionLabel("Target");

        var candidates = TargetSelector.ScanHostiles(cfg.AutoSelectRangeYalms);
        if (candidates.Count == 0)
        {
            EmptyCard.Draw("target",
                $"Scanning — no hostile targets within {cfg.AutoSelectRangeYalms:F0}y.",
                FontAwesomeIcon.Satellite);
            return;
        }

        var picked = ctrl.LastResolvedTarget ?? candidates[0];
        var pickedBelow = HpMath.IsBelowThreshold(picked, cfg, state.JobId);
        HeroCard.Draw(picked, pickedBelow, cfg, state);

        var others = 0;
        for (var i = 0; i < candidates.Count; i++)
            if (candidates[i].GameObjectId != picked.GameObjectId) others++;

        if (others == 0) return;

        ImGui.Spacing();
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted($"OTHER IN RANGE ({others})");

        for (var i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            if (c.GameObjectId == picked.GameObjectId) continue;
            CandidateRow.Draw(c, cfg, state.JobId);
        }
    }
}
