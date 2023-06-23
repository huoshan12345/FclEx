namespace FclEx.Extensions;

public enum BuildType
{
    Debug,
    Release,
}

public static class AssemblyExtensions
{
    private static (BuildType BuildType, bool IsJitOptimized) GetInfo(this Assembly assembly)
    {
        BuildType buildType;
        bool isJitOptimized;
        var attr = assembly.GetCustomAttribute<DebuggableAttribute>();
        // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
        if (attr != null)
        {
            // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
            // it's a DEBUG build; we have to check the JIT Optimization flag
            // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
            isJitOptimized = !attr.IsJITOptimizerDisabled;
            buildType = attr.IsJITOptimizerDisabled ? BuildType.Debug : BuildType.Release;

            // check for Debug Output "full" or "pdb-only"
            //DebugOutput = (debuggableAttribute.DebuggingFlags &
            //               DebuggableAttribute.DebuggingModes.Default) !=
            //              DebuggableAttribute.DebuggingModes.None
            //    ? "Full" : "pdb-only";
        }
        else
        {
            isJitOptimized = true;
            buildType = BuildType.Release;
        }

        return (buildType, isJitOptimized);
    }

    public static bool IsDebug(this Assembly assembly)
    {
        return assembly.GetInfo().BuildType == BuildType.Debug;
    }

    public static bool IsRelease(this Assembly assembly)
    {
        return assembly.GetInfo().BuildType == BuildType.Release;
    }

    public static Type GetRequiredType(this Assembly assembly, string name, bool ignoreCase = false)
    {
        return assembly.GetType(name, true, ignoreCase) ?? throw new InvalidOperationException($"Cannot find type '{name}' in assembly '{assembly.FullName}'");
    }
}