namespace FclEx.Extensions;

partial class TypeExtensions
{
    // Let Exception be an out parameter to provide the reason why the check did not pass.
    private delegate bool TypeCheck(Type type, [NotNullWhen(false)] out Exception? ex);

    private record TypeCheckResult(bool Passed, string? ErrorMessage, string? ParamName);

    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, TypeCheckResult>> _typeCheckCache = new();

    private static void Ensure(this Type type, TypeCheck check, string predicateName)
    {
        if (check(type, out var ex))
            return;

        if (ex is ArgumentException { ParamName: nameof(type) })
        {
            ex.ReThrow();
        }
        else
        {
            throw new ArgumentException($"The type {type.LongName()} is not {predicateName} due to: " + ex?.Message, nameof(type), ex);
        }
    }

    private static (bool, Exception?) Check(this Type type, string checkName, Action<Type> action)
    {
        var checks = _typeCheckCache.GetValue(type, _ => new());
        var result = checks.GetOrAdd(checkName, _ =>
        {
            try
            {
                action(type);
                return new TypeCheckResult(true, null, null);
            }
            catch (Exception ex)
            {
                return new TypeCheckResult(
                    false,
                    ex.Message,
                    ex is ArgumentException argumentException ? argumentException.ParamName : null);
            }
        });

        return result.Passed
            ? (true, null)
            : (false, new ArgumentException(result.ErrorMessage, result.ParamName).SetStackTrace());
    }

    /// <summary>
    /// Throws when the specified type is not blittable.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <exception cref="ArgumentException">The type is not blittable.</exception>
    public static void EnsureBlittable(this Type type)
    {
        type.Ensure(IsBlittable, "blittable");
    }

    /// <summary>
    /// Throws when the specified type is not marshalable.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <param name="allowPointerFields">
    /// <see langword="true"/> to allow fields whose <see cref="MarshalAsAttribute"/> representation contains a
    /// native pointer; otherwise, only inline marshal representations are accepted.
    /// </param>
    /// <exception cref="ArgumentException">The type is not marshalable under the requested policy.</exception>
    public static void EnsureMarshalable(this Type type, bool allowPointerFields = true)
    {
        if (type.IsMarshalable(out var exception, allowPointerFields))
            return;

        if (exception is ArgumentException { ParamName: nameof(type) })
            exception.ReThrow();

        throw new ArgumentException(
            $"The type {type.LongName()} is not marshalable due to: {exception?.Message}",
            nameof(type),
            exception);
    }

    /// <summary>
    /// Determines whether the type has the same binary representation in managed and unmanaged memory.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="ex">
    /// When the method returns <see langword="false"/>, contains the exception describing why the type is not blittable.
    /// </param>
    /// <returns><see langword="true"/> if the type is blittable; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Blittable types include the primitive numeric types, one-dimensional arrays of blittable primitive types,
    /// and sequential or explicit-layout types whose instance fields are also blittable. <see cref="bool"/>,
    /// <see cref="char"/>, strings, delegates, generic types, auto-layout types, nested arrays, and
    /// multi-dimensional arrays are rejected.
    /// </remarks>
    public static bool IsBlittable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.Check(nameof(IsBlittable), m => m.CheckBlittable(null, null));
        return flag;
    }

    /// <summary>
    /// Determines whether the type can be marshalled by the runtime interop marshaler under a pointer-field policy.
    /// </summary>
    /// <param name="type">The type to inspect. Nullable value types are checked as their underlying value type.</param>
    /// <param name="allowPointerFields">
    /// <see langword="true"/> to accept pointer-based <see cref="MarshalAsAttribute"/> forms such as
    /// <see cref="UnmanagedType.LPStr"/> and <see cref="UnmanagedType.LPArray"/>; otherwise, rejects them.
    /// </param>
    /// <param name="ex">When the method returns <see langword="false"/>, contains the reason the type was rejected.</param>
    /// <returns><see langword="true"/> when the type satisfies the requested marshalability policy; otherwise, <see langword="false"/>.</returns>
    public static bool IsMarshalable(this Type type, [NotNullWhen(false)] out Exception? ex, bool allowPointerFields = true)
    {
        var checkName = $"{nameof(IsMarshalable)}:{allowPointerFields}";
        (var flag, ex) = type.Check(checkName, m => CheckMarshalable(m, null, null, null, allowPointerFields));
        return flag;
    }

    private static void CheckBlittable(this Type type, HashSet<Type>? visited, string? path)
    {
        // https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types

        if (Types.BlittableTypes.Contains(type))
            return;

        if (type == typeof(Array)
            || type == typeof(bool)
            || type == typeof(char)
            || type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            ThrowBlittable(type, null, path);

        // Exclude all generic types as well as nullable types. 
        if (type.IsGenericType)
            ThrowBlittable(type, "generic", path);

        if (type.IsAbstract)
            ThrowBlittable(type, "abstract", path);

        var stack = visited ?? [];

        if (stack.Add(type) == false)
            ThrowBlittable(type, "circular referenced", path);

        try
        {
            foreach (var m in type.GetAllInstanceFields())
            {
                var name = m.GetAutoPropertyOrFieldName();
                var fieldPath = (path ?? "$") + "." + name;
                m.FieldType.CheckBlittable(stack, fieldPath);
            }

            CheckPinnable(type, stack, path);
        }
        finally
        {
            stack.Remove(type);
        }

        return;

        static void CheckPinnable(Type type, HashSet<Type>? visited, string? path)
        {
            object instance;
            if (type.GetElementType() is { } elementType)
            {
                if (type.GetArrayRank() > 1)
                    ThrowBlittable(type, "multi-dimensional array", path);

                if (elementType.IsArray)
                    ThrowBlittable(type, "nested array", path);

                elementType.CheckBlittable(visited, path); // check if element type is pinnable as well.

                var array = Array.CreateInstance(elementType, 1);
                var entry = RuntimeHelpers.GetUninitializedObject(elementType);
                array.SetValue(entry, 0);
                instance = array;
            }
            else if (type.IsAutoLayout) // don't do this check for array type.
            {
                ThrowBlittable(type, "laid out automatically", path);
                return;
            }
            else
            {
                instance = RuntimeHelpers.GetUninitializedObject(type);
            }

            GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        }
    }

    private static void CheckMarshalable(Type type, FieldInfo? field, HashSet<Type>? visited, string? path, bool allowPointerFields = true)
    {
        type = type.UnwrapNullable();

        if (type.IsGenericType)
            ThrowMarshalable(type, "generic", path);

        if (type.IsAbstract)
            ThrowMarshalable(type, "abstract", path);

        if (type.IsEnum || Types.PrimitiveTypes.Contains(type))
            return;

        if (field?.GetCustomAttribute<MarshalAsAttribute>(false) is { } marshalAs)
        {
            if (allowPointerFields || marshalAs.Value.IsInlineMarshalRepresentation(field.FieldType))
                return;

            ThrowMarshalable(type, $"marshalled as {marshalAs.Value}, which is not an inline representation", path);
        }

        if (type.IsAutoLayout)
            ThrowMarshalable(type, "auto layout", path);

        if (type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            ThrowMarshalable(type, null, path);

        var stack = visited ?? [];

        if (stack.Add(type) == false)
            ThrowMarshalable(type, "circular referenced", path);

        try
        {
            foreach (var m in type.GetAllInstanceFields())
            {
                var name = m.GetAutoPropertyOrFieldName();
                var fieldPath = (path ?? "$") + "." + name;
                CheckMarshalable(m.FieldType, m, stack, fieldPath, allowPointerFields);
            }

            _ = Marshal.SizeOf(type);
        }
        finally
        {
            stack.Remove(type);
        }
    }

    private static bool IsInlineMarshalRepresentation(this UnmanagedType value, Type fieldType)
    {
        return value switch
        {
            UnmanagedType.I1 or UnmanagedType.U1 or
            UnmanagedType.I2 or UnmanagedType.U2 or
            UnmanagedType.I4 or UnmanagedType.U4 or
            UnmanagedType.I8 or UnmanagedType.U8 or
            UnmanagedType.R4 or UnmanagedType.R8 or
            UnmanagedType.Bool or UnmanagedType.VariantBool or
            UnmanagedType.Error or
            UnmanagedType.SysInt or UnmanagedType.SysUInt or
            UnmanagedType.ByValTStr => true,
            UnmanagedType.ByValArray => fieldType.GetElementType() is { } elementType
                                        && (elementType.IsEnum || Types.PrimitiveTypes.Contains(elementType)),
            _ => false,
        };
    }

    [DoesNotReturn]
    private static void ThrowBlittable(Type type, string? reason, string? path)
        => Throw(type, "blittable", reason, path);

    [DoesNotReturn]
    private static void ThrowMarshalable(Type type, string? reason, string? path)
        => Throw(type, "marshalable", reason, path);

    [DoesNotReturn]
    private static void Throw(Type type, string checkName, string? reason, string? path)
    {
        var error = StringBuilderHelper.Build(m =>
        {
            m.Append($"The type '{type.LongName()}'");
            if (path.IsNotEmpty())
            {
                m.Append(" at ");
                m.Append(path);
            }
            m.Append(" is not ");
            m.Append(checkName);
            if (reason.IsNotEmpty())
            {
                m.Append($" because it is {reason}");
            }
            m.Append('.');
        });
        throw new ArgumentException(error, nameof(type));
    }

    /// <summary>
    /// Gets the name of the property if the specified field is an auto-implemented property backing field;
    /// otherwise, returns the field name.
    /// </summary>
    /// <param name="field">The <see cref="FieldInfo"/> instance representing the field.</param>
    /// <returns>
    /// The name of the associated property if <paramref name="field"/> is recognized as a backing field for
    /// an auto-implemented property; otherwise, the field's own name.
    /// </returns>
    public static string GetAutoPropertyOrFieldName(this FieldInfo field)
    {
        return field.TryGetAutoProperty(out var property)
            ? property.Name
            : field.Name;
    }
}
