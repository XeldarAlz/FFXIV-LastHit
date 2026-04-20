using System.Collections.Generic;
using System.Linq;

namespace LastHitPlugin.Core;

internal static class JobModuleRegistry
{
    private static readonly List<IJobLimitBreakModule> Modules = new();

    public static void Register(IJobLimitBreakModule module) => Modules.Add(module);

    public static void Clear() => Modules.Clear();

    public static IJobLimitBreakModule? For(uint classJobId)
        => Modules.FirstOrDefault(m => m.Handles(classJobId));
}
