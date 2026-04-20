using Dalamud.Configuration;
using System;

namespace LastHitPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public bool Enabled { get; set; } = true;
    public float HpThresholdPercent { get; set; } = 30f;
    public bool AutoSelectLowestHp { get; set; } = true;
    public float AutoSelectRangeYalms { get; set; } = 30f;

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
