using System.Collections.Generic;

namespace LastHitPlugin.Core;

internal enum LbKind
{
    Offensive,
    Support,
}

internal interface IJobLimitBreakModule
{
    IReadOnlyList<uint> Resolve(uint classJobId);
    LbKind Classify(uint classJobId);
}
