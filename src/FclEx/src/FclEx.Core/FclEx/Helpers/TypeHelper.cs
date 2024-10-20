namespace FclEx.Helpers;

public static class TypeHelper
{
    public static readonly IReadOnlyCollection<Type> ActionTypes = AssemblyHelper.AssemblyOfAction
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Action))
        .ToHashSet();

    public static readonly IReadOnlyDictionary<int, Type> ActionTypeDic = ActionTypes.ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);

    public static readonly IReadOnlyCollection<Type> FuncTypes = AssemblyHelper.AssemblyOfFunc
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Func<int>))
        .ToHashSet();

    public static readonly IReadOnlyDictionary<int, Type> FuncTypeDic = FuncTypes.ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);

    public static Type? GetType(string name) => Type.GetType(name);

    public static Type GetRequiredType(string name) => Type.GetType(name) ?? throw new InvalidOperationException($"Cannot find type '{name}'");

    public static Type? GetType(string name, string assemblyName) => Type.GetType($"{name}, {assemblyName}");

    public static Type GetRequiredType(string name, string assemblyName) => Type.GetType($"{name}, {assemblyName}") ?? throw new InvalidOperationException($"Cannot find type '{name}'");
}