namespace FclEx.Extensions;

public static partial class TypeExtensions
{
    /// <summary>
    /// Creates an instance of the specified type by invoking a public constructor whose leading parameters
    /// match the supplied arguments and whose remaining parameters are optional.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">
    /// The constructor arguments. A <see langword="null"/> argument matches any reference type parameter or
    /// <see cref="Nullable{T}"/> parameter. Omitted optional parameters are supplied from their metadata default values.
    /// </param>
    /// <returns>The created object.</returns>
    /// <exception cref="MissingMethodException">No public constructor matches the supplied arguments.</exception>
    /// <exception cref="AmbiguousMatchException">More than one public constructor matches the supplied arguments.</exception>
    /// <remarks>
    /// This method is intended to behave like a default-parameter-aware version of
    /// <c>Activator.CreateInstance(type, args)</c>. It does not reorder arguments.
    /// </remarks>
    public static object CreateObject(this Type type, params object?[] args)
    {
        FclEx.Check.NotNull(type);

        args ??= [null];

        if (args.Length == 0 && type.IsValueType)
            return Activator.CreateInstance(type)!;

        var matches = type.GetConstructors()
            .Where(m => ArgumentListMatches(m, args))
            .ToArray();

        if (matches.Length == 0)
            throw new MissingMethodException(type.FullName, ".ctor");

        if (matches.Length > 1)
            throw new AmbiguousMatchException($"More than one constructor of type '{type.FullName}' matches the supplied arguments.");

        var ctor = matches[0];
        var paras = ctor.GetParameters();
        if (paras.Length > args.Length)
        {
            args = args.Concat(paras.Skip(args.Length).Select(m => m.RawDefaultValue)).ToArray();
        }
        return ctor.Invoke(args);

        static bool ArgumentListMatches(MethodBase method, object?[] args)
        {
            var parameters = method.GetParameters();
            if (parameters.Length < args.Length)
                return false;

            for (var i = 0; i < args.Length; i++)
            {
                if (ParameterMatches(parameters[i].ParameterType, args[i]) == false)
                    return false;
            }

            return parameters.Skip(args.Length).All(p => p.IsOptional);
        }

        static bool ParameterMatches(Type parameterType, object? arg)
        {
            if (arg is null)
                return parameterType.IsValueType == false || Nullable.GetUnderlyingType(parameterType) != null;

            var argType = arg.GetType();
            if (parameterType.IsAssignableFrom(argType))
                return true;

            return Nullable.GetUnderlyingType(parameterType) is { } underlyingType
                   && underlyingType.IsAssignableFrom(argType);
        }
    }

    [MethodImpl(AggressiveInlining)]
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
    /// It does not treat <paramref name="type"/> itself as an implementation, even when
    /// <paramref name="type"/> and <paramref name="interfaceType"/> are the same interface type.
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
    /// It returns <see langword="false"/> when <paramref name="type"/> is the same interface as
    /// <paramref name="interfaceType"/>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static bool Implements(this Type type, Type interfaceType)
    {
        return type.GetImplementedInterface(interfaceType) != null;
    }

    /// <summary>
    /// Determines whether the specified type inherits from a given base type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to inspect.</param>
    /// <param name="baseType">The base type to check against.</param>
    /// <returns>
    /// <see langword="true"/> if the type inherits from the specified base type; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method supports both direct inheritance and generic type definitions.
    /// It starts from <see cref="Type.BaseType"/>, so it does not treat <paramref name="type"/>
    /// itself as inheriting from <paramref name="baseType"/>.
    /// </remarks>
    public static bool Inherits(this Type type, Type baseType)
    {
        var t = type.BaseType;
        while (t != null)
        {
            if (t == baseType)
                return true;

            if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == baseType)
                return true;

            t = t.BaseType;
        }
        return false;
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsDynamic(this Type type)
    {
        return type.IsDefined<DynamicAttribute>(true);
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsCompilerGenerated(this Type type)
    {
        return type.IsDefined<CompilerGeneratedAttribute>(false);
    }

#if !NET5_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
    {
        return type.GetConstructor(bindingAttr, null, types, null);
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsAssignableTo(this Type type, [NotNullWhen(true)] Type? targetType)
    {
        return targetType?.IsAssignableFrom(type) ?? false;
    }
#endif

    [MethodImpl(AggressiveInlining)]
    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }

    /// <summary>
    /// Determines whether an implicit conversion operator exists from the source type to the target type.
    /// </summary>
    /// <param name="sourceType">The conversion source type.</param>
    /// <param name="targetType">The conversion target type.</param>
    /// <param name="declaringType">
    /// The type on which to search for the operator. When omitted, both <paramref name="sourceType"/> and
    /// <paramref name="targetType"/> are searched.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a public static <c>op_Implicit</c> method with the requested source and
    /// target types is found; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasImplicitConversion(this Type sourceType, Type targetType, Type? declaringType = null)
    {
        var declaringTypes = declaringType is null
            ? [sourceType, targetType]
            : new[] { declaringType };

        return declaringTypes
            .Distinct()
            .SelectMany(m => m.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
            .Any(mi =>
            {
                var parameter = mi.GetParameters().FirstOrDefault();
                return parameter != null && parameter.ParameterType == sourceType;
            });

    }
}
