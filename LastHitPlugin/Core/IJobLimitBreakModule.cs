namespace LastHitPlugin.Core;

internal interface IJobLimitBreakModule
{
    bool TryResolve(uint classJobId, out uint actionId);
}
