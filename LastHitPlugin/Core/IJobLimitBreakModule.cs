namespace LastHitPlugin.Core;

internal interface IJobLimitBreakModule
{
    bool Handles(uint classJobId);
    uint GetLimitBreakActionId();
}
