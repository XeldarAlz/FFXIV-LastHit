using Dalamud.Configuration;
using ECommons.Throttlers;
using PvpAutoLb.Core;
using System;
using System.Collections.Generic;

namespace PvpAutoLb;

public enum ThresholdMode
{
    Percent,
    Absolute,
}

public class JobThreshold
{
    public ThresholdMode Mode { get; set; } = ThresholdMode.Percent;
    public float Percent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint Absolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    private const string SaveThrottleKey = "PvpAutoLb.ConfigSave";

    public int Version { get; set; } = 2;

    public bool Enabled { get; set; } = true;

    public ThresholdMode ThresholdMode { get; set; } = ThresholdMode.Percent;
    public float HpThresholdPercent { get; set; } = PvpAutoLbConstants.DefaultThresholdPercent;
    public uint HpThresholdAbsolute { get; set; } = PvpAutoLbConstants.DefaultThresholdAbsolute;

    public bool AutoSelectLowestHp { get; set; } = true;
    public float AutoSelectRangeYalms { get; set; } = PvpAutoLbConstants.DefaultAutoSelectRangeYalms;

    public Dictionary<uint, JobThreshold> PerJobThresholds { get; set; } = new();

    public bool SkipDoomedTargets { get; set; } = true;

    public bool PlaySoundOnFire { get; set; } = false;
    public int FireSoundId { get; set; } = 7;
    public bool LogFireToChat { get; set; } = false;

    public List<string> NameBlocklist { get; set; } = new();
    public DutyMask EnabledDuties { get; set; } = DutyMask.All;

    public uint LifetimeFires { get; set; }
    public uint LifetimeKills { get; set; }
    public uint LifetimeEnemiesAffected { get; set; }

    public ThresholdMode EffectiveMode(uint jobId)
        => PerJobThresholds.TryGetValue(jobId, out var j) ? j.Mode : ThresholdMode;

    public float EffectivePercent(uint jobId)
        => PerJobThresholds.TryGetValue(jobId, out var j) ? j.Percent : HpThresholdPercent;

    public uint EffectiveAbsolute(uint jobId)
        => PerJobThresholds.TryGetValue(jobId, out var j) ? j.Absolute : HpThresholdAbsolute;

    public bool HasJobOverride(uint jobId) => jobId != 0 && PerJobThresholds.ContainsKey(jobId);

    public JobThreshold EnsureJobOverride(uint jobId)
    {
        if (!PerJobThresholds.TryGetValue(jobId, out var j))
        {
            j = new JobThreshold
            {
                Mode = ThresholdMode,
                Percent = HpThresholdPercent,
                Absolute = HpThresholdAbsolute,
            };
            PerJobThresholds[jobId] = j;
        }
        return j;
    }

    public void ClearJobOverride(uint jobId) => PerJobThresholds.Remove(jobId);

    public string FormatEffective(uint jobId, string prefix = "Fires below ")
    {
        var label = EffectiveMode(jobId) == ThresholdMode.Percent
            ? $"{prefix}{EffectivePercent(jobId):F0}% HP"
            : $"{prefix}{EffectiveAbsolute(jobId):N0} HP";
        return HasJobOverride(jobId) ? label + " (per-job)" : label;
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);

    // Slider/drag callbacks fire every frame; debounce so we don't hammer disk.
    public void SaveDebounced()
    {
        if (EzThrottler.Throttle(SaveThrottleKey, PvpAutoLbConstants.SaveThrottleMs))
            Save();
    }
}
