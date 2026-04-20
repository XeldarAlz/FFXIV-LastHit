using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LastHitPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("LastHit###LastHitMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var cfg = plugin.Configuration;
        var ctrl = plugin.Controller;

        ImGui.TextUnformatted($"Status: {(cfg.Enabled ? "ENABLED" : "disabled")}");
        if (ImGui.Button("Settings")) plugin.ToggleConfigUi();

        ImGui.Separator();

        var target = ctrl.LastResolvedTarget;
        if (target == null || target.IsDead)
        {
            ImGui.TextUnformatted("No valid target.");
        }
        else
        {
            var cur = target.CurrentHp;
            var max = target.MaxHp;
            var pct = max == 0 ? 0f : 100f * cur / max;

            ImGui.TextUnformatted($"Target: {target.Name.TextValue}");
            ImGui.TextUnformatted($"HP: {cur:N0} / {max:N0}  ({pct:F1}%%)");

            var threshLabel = cfg.ThresholdMode == ThresholdMode.Percent
                ? $"< {cfg.HpThresholdPercent:F0}%%"
                : $"< {cfg.HpThresholdAbsolute:N0} HP";
            ImGui.TextUnformatted($"Threshold: {threshLabel}");

            var willFire = cfg.ThresholdMode == ThresholdMode.Percent
                ? pct < cfg.HpThresholdPercent
                : cur < cfg.HpThresholdAbsolute;
            ImGui.TextUnformatted($"Will fire: {(willFire ? "YES" : "no")}");
        }

        ImGui.Separator();

        if (ctrl.LastFiredUtc is { } ts)
        {
            var ago = DateTime.UtcNow - ts;
            ImGui.TextUnformatted($"Last fired: {ago.TotalSeconds:F1}s ago");
        }
        else
        {
            ImGui.TextUnformatted("Last fired: never");
        }
    }
}
