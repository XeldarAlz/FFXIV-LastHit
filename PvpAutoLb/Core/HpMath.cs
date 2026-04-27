using Dalamud.Game.ClientState.Objects.Types;

namespace PvpAutoLb.Core;

internal static class HpMath
{
    // ShieldPercentage is in 0–100, expressed as a percentage of MaxHp.
    public static uint ShieldHp(IBattleChara t)
        => (uint)((ulong)t.MaxHp * t.ShieldPercentage / 100UL);

    public static uint EffectiveHp(IBattleChara t)
        => t.CurrentHp + ShieldHp(t);

    public static float EffectiveHpPercent(IBattleChara t)
        => t.MaxHp == 0 ? 0f : 100f * EffectiveHp(t) / t.MaxHp;

    public static bool IsBelowThreshold(IBattleChara t, Configuration cfg, uint jobId)
    {
        var eff = EffectiveHp(t);
        if (cfg.EffectiveMode(jobId) == ThresholdMode.Absolute)
            return eff < cfg.EffectiveAbsolute(jobId);
        if (t.MaxHp == 0) return false;
        return 100f * eff / t.MaxHp < cfg.EffectivePercent(jobId);
    }
}
