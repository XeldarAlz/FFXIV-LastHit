using ECommons.GameHelpers;

namespace PvpAutoLb.Core;

internal readonly record struct LbDrawState(
    uint JobId,
    uint ActionId,
    bool IsSupport,
    LbReadyReason Readiness,
    LbTargetingProfile Profile)
{
    public bool ActionReady => Readiness == LbReadyReason.Ready;
    public bool CanFire => ActionId != 0 && !IsSupport && ActionReady;

    public static LbDrawState Resolve(AutoLbController ctrl)
    {
        var jobId = Player.Available ? Player.Object!.ClassJob.RowId : 0u;
        var actionId = LbCatalog.ResolveActionId(jobId);
        if (actionId == 0)
            return new LbDrawState(jobId, 0, false, LbReadyReason.GaugeLow, LbTargetingProfile.None);

        var isSupport = LbCatalog.Classify(jobId) == LbKind.Support;
        var profile = ctrl.LastProfile.ActionId == actionId
            ? ctrl.LastProfile
            : LbTargetingProfile.FromAction(actionId);

        var targetEntity = ctrl.LastResolvedTarget?.EntityId ?? PvpAutoLbConstants.NoTargetEntityId;
        var ready = ActionExec.IsReady(actionId, targetEntity);
        var readiness = ready ? LbReadyReason.Ready : InferReason(ctrl.LastResolvedTarget, profile);

        return new LbDrawState(jobId, actionId, isSupport, readiness, profile);
    }

    private static LbReadyReason InferReason(Dalamud.Game.ClientState.Objects.Types.IBattleChara? target, LbTargetingProfile profile)
    {
        if (target != null && profile.Range > 0 && Geo.DistanceToPlayer(target) > profile.Range)
            return LbReadyReason.OutOfRange;
        return LbReadyReason.GaugeLow;
    }
}
