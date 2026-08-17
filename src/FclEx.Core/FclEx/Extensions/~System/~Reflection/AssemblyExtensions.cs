namespace FclEx.Extensions;

public static class AssemblyExtensions
{
    /// <summary>
    /// Resolves a manifest resource name by its complete name, or by a unique ordinal suffix.
    /// </summary>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <param name="name">The complete resource name or a suffix that uniquely identifies it.</param>
    /// <returns>The complete manifest resource name, or <see langword="null"/> when no resource matches.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or matches multiple resources by suffix.</exception>
    internal static string? ResolveManifestResourceName(this Assembly assembly, string name)
    {
        Check.NotEmpty(name);

        var resourceNames = assembly.GetManifestResourceNames();
        if (resourceNames.FirstOrDefault(p => string.Equals(p, name, StringComparison.Ordinal)) is { } exactMatch)
            return exactMatch;

        var suffixMatches = resourceNames
            .Where(p => p.EndsWith(name, StringComparison.Ordinal))
            .ToArray();

        return suffixMatches.Length switch
        {
            0 => null,
            1 => suffixMatches[0],
            _ => throw new ArgumentException(
                $"The resource name '{name}' is ambiguous. Matching resources: {string.Join(", ", suffixMatches)}.",
                nameof(name)),
        };
    }

    public static Stream OpenResource(this Assembly assembly, string name)
    {
        var resourceName = assembly.ResolveManifestResourceName(name);
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

    /// <summary>Returns whether the assembly's <see cref="DebuggableAttribute"/> enables JIT optimization.</summary>
    /// <remarks>
    /// This reads assembly metadata, not the actual runtime JIT state, and it cannot determine whether the assembly was
    /// built with the Debug or Release MSBuild configuration. Assemblies without <see cref="DebuggableAttribute"/> are
    /// treated as enabling optimization.
    /// </remarks>
    /// <param name="assembly">The assembly whose metadata to inspect.</param>
    public static bool IsJitOptimized(this Assembly assembly)
    {
        bool isJitOptimized;
        var attr = assembly.GetCustomAttribute<DebuggableAttribute>();
        // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
        if (attr != null)
        {
            // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
            // it's a DEBUG build; we have to check the JIT Optimization flag
            // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
            isJitOptimized = !attr.IsJITOptimizerDisabled;

            // check for Debug Output "full" or "pdb-only"
            //DebugOutput = (debuggableAttribute.DebuggingFlags &
            //               DebuggableAttribute.DebuggingModes.Default) !=
            //              DebuggableAttribute.DebuggingModes.None
            //    ? "Full" : "pdb-only";
        }
        else
        {
            isJitOptimized = true;
        }

        return isJitOptimized;
    }

    public static Type GetRequiredType(this Assembly assembly, string name, bool ignoreCase = false)
    {
        return assembly.GetType(name, true, ignoreCase) ?? throw new InvalidOperationException($"Cannot find type '{name}' in assembly '{assembly.FullName}'");
    }
}
