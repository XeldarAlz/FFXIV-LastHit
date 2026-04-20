using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace LastHitPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration config;

    public ConfigWindow(Plugin plugin) : base("LastHit — Configuration###LastHitConfig")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(380, 320);
        SizeCondition = ImGuiCond.FirstUseEver;
        config = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (config.IsConfigWindowMovable) Flags &= ~ImGuiWindowFlags.NoMove;
        else Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var enabled = config.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            config.Enabled = enabled;
            config.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Fire Limit Break when target HP drops below:");

        var mode = (int)config.ThresholdMode;
        if (ImGui.RadioButton("Percent of max HP", ref mode, (int)ThresholdMode.Percent))
        {
            config.ThresholdMode = ThresholdMode.Percent;
            config.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Absolute HP", ref mode, (int)ThresholdMode.Absolute))
        {
            config.ThresholdMode = ThresholdMode.Absolute;
            config.Save();
        }

        if (config.ThresholdMode == ThresholdMode.Percent)
        {
            var pct = config.HpThresholdPercent;
            if (ImGui.SliderFloat("##pct", ref pct, 1f, 99f, "%.0f%%"))
            {
                config.HpThresholdPercent = pct;
                config.Save();
            }
        }
        else
        {
            var abs = (int)config.HpThresholdAbsolute;
            if (ImGui.DragInt("##abs", ref abs, 100f, 1, 500000, "%d HP"))
            {
                config.HpThresholdAbsolute = (uint)Math.Max(1, abs);
                config.Save();
            }
        }

        ImGui.Separator();

        var auto = config.AutoSelectLowestHp;
        if (ImGui.Checkbox("Auto-select lowest-HP enemy when no manual target", ref auto))
        {
            config.AutoSelectLowestHp = auto;
            config.Save();
        }

        var range = config.AutoSelectRangeYalms;
        if (ImGui.SliderFloat("Auto-select range", ref range, 5f, 50f, "%.0f y"))
        {
            config.AutoSelectRangeYalms = range;
            config.Save();
        }

        ImGui.Separator();

        var movable = config.IsConfigWindowMovable;
        if (ImGui.Checkbox("Config window movable", ref movable))
        {
            config.IsConfigWindowMovable = movable;
            config.Save();
        }
    }
}
