using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace LastHitPlugin.Core;

internal static class TargetSelector
{
    public static IBattleChara? PickLowestHp(float rangeYalms)
    {
        if (!Player.Available) return null;
        var me = Player.Object!;
        var meId = me.GameObjectId;
        var mePos = me.Position;
        var rangeSq = rangeYalms * rangeYalms;

        return Svc.Objects
            .OfType<IBattleChara>()
            .Where(o => o.GameObjectId != meId)
            .Where(o => !o.IsDead && o.IsTargetable)
            .Where(o => o.StatusFlags.HasFlag(StatusFlags.Hostile))
            .Where(o =>
            {
                var dx = o.Position.X - mePos.X;
                var dz = o.Position.Z - mePos.Z;
                return dx * dx + dz * dz <= rangeSq;
            })
            .OrderBy(o => o.CurrentHp)
            .FirstOrDefault();
    }
}
