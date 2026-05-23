namespace FclEx.Extensions;

public enum BuildType
{
    Debug,
    Release,
}

public static class AssemblyExtensions
{
    public static Stream OpenResource(this Assembly assembly, string name)
    {
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(name));
        if (resourceName == null)
            throw new ArgumentException($"Cannot find manifest resource name in assembly {assembly.GetName().Name} by name: " + name);

        return assembly.GetManifestResourceStream(resourceName)
               ?? throw new InvalidOperationException($"Cannot find manifest resource stream in assembly {assembly.GetName().Name} by name: " + resourceName);
    }

    public static T ReadResource<T>(this Assembly assembly, string name, Func<Stream, T> func)
    {
        using var resource = OpenResource(assembly, name);
        return func(resource);
    }

    public static string ReadResource(this Assembly assembly, string name, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return ReadResource(assembly, name, s =>
        {
            using var sr = new StreamReader(s, encoding);
            return sr.ReadToEnd();
        });
    }

    public static (BuildType BuildType, bool IsJitOptimized) GetBuildInfo(this Assembly assembly)
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
        return assembly.GetBuildInfo().BuildType == BuildType.Debug;
    }

    public static bool IsRelease(this Assembly assembly)
    {
        return assembly.GetBuildInfo().BuildType == BuildType.Release;
    }

    public static Type GetRequiredType(this Assembly assembly, string name, bool ignoreCase = false)
    {
        return assembly.GetType(name, true, ignoreCase) ?? throw new InvalidOperationException($"Cannot find type '{name}' in assembly '{assembly.FullName}'");
    }
}