namespace FclEx.Extensions;

public static partial class TypeExtensions
{
    public static object CreateObject(this Type type, params object?[] args)
    {
        FclEx.Check.NotNull(type);

        if (args.IsNullOrEmpty())
            return Activator.CreateInstance(type)!;

        var argsType = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var ctor = type.GetConstructors().FirstOrDefault(m => m.ArgumentListMatches(argsType));
        if (ctor != null)
        {
            var paras = ctor.GetParameters();
            if (paras.Length > args.Length)
            {
                args = args.Concat(paras.Skip(args.Length).Select(m => m.RawDefaultValue)).ToArray();
            }
            return ctor.Invoke(args);
        }

        throw new MissingMethodException();
    }

    public static T CreateObject<T>(this Type type, params object?[] args)
    {
        return type.CreateObject(args).CastTo<T>();
    }

    /// <summary>
    /// Finds the specified interface implemented by the given type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect.</param>
    /// <param name="interfaceType">The interface type to find.</param>
    /// <returns>
    /// The implemented interface <see cref="Type"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This method checks both direct interface implementations and generic interface definitions.
    /// </remarks>
    public static Type? GetImplementedInterface(this Type type, Type interfaceType)
    {
        return type.GetInterfaces().FirstOrDefault(x =>
            x == interfaceType
            || x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);
    }

    /// <summary>
    /// Determines whether the specified type implements a given interface.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect.</param>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the type implements the specified interface; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method internally uses <see cref="GetImplementedInterface"/> to perform the check.
    /// </remarks>
    public static bool Implements(this Type type, Type interfaceType)
    {
        return type.GetImplementedInterface(interfaceType) != null;
    }

    /// <summary>
    /// Determines whether the specified type inherits from a given base type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect. Can be <see langword="null"/>.</param>
    /// <param name="baseType">The base type to check against.</param>
    /// <returns>
    /// <see langword="true"/> if the type inherits from the specified base type; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method supports both direct inheritance and generic type definitions.
    /// </remarks>
    public static bool Inherits(this Type? type, Type baseType)
    {
        var t = type;
        while (t != null)
        {
            if (t == baseType)
                return true;

            if (t.IsGenericType && t.GetGenericTypeDefinition() == baseType)
                return true;

            t = t.BaseType;
        }
        return false;
    }

    public static bool IsDynamic(this Type type)
    {
        return type.IsDefined<DynamicAttribute>(true);
    }

    public static bool IsCompilerGenerated(this Type type)
    {
        return type.IsDefined<CompilerGeneratedAttribute>(true);
    }

#if NETSTANDARD2_0
    public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
    {
        return type.GetConstructor(bindingAttr, null, types, null);
    }

    public static bool IsAssignableTo(this Type type, [NotNullWhen(true)] Type? targetType) => targetType?.IsAssignableFrom(type) ?? false;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }

    public static bool HasImplicitConversion(this Type baseType, Type targetType, Type? definedOn = null)
    {
        definedOn ??= baseType;
        return definedOn.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
            .Any(mi =>
            {
                var pi = mi.GetParameters().FirstOrDefault();
                return pi != null && pi.ParameterType == baseType;
            });

    }
}