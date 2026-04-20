using System.Collections.Generic;

namespace LastHitPlugin.Core;

internal static class JobModuleRegistry
{
    private static readonly List<IJobLimitBreakModule> Modules = new();

    public static void Register(IJobLimitBreakModule module) => Modules.Add(module);

    public static void Clear() => Modules.Clear();

    public static uint ResolveActionId(uint classJobId)
    {
        foreach (var m in Modules)
            if (m.TryResolve(classJobId, out var id) && id != 0)
                return id;
        return 0;
    }
}
