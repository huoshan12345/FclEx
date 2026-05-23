namespace FclEx.Extensions;

partial class TypeExtensions
{
    // let Exception be an out parameter to provider the reason why the check is not passed.
    private delegate bool TypeCheck(Type type, [NotNullWhen(false)] out Exception? ex);

    private static readonly ConcurrentDictionary<(Type, string), (bool, Exception?)> _flagCache = new();

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
        return _flagCache.GetOrAdd((type, checkName), m =>
        {
            try
            {
                action(type);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        });
    }

    public static void EnsureBlittable(this Type type)
    {
        type.Ensure(IsBlittable, "blittable");
    }

    public static void EnsureMarshalable(this Type type)
    {
        type.Ensure(IsMarshalable, "marshalable");
    }

    /// <summary>
    /// Blittable types have an identical presentation in memory for both managed and unmanaged code.<br/>
    /// * There are 12 primitive types are blittable. The type of <see cref="bool"/> 和 <see cref="char"/> are NOT blittable.<br/>
    /// * One-dimensional arrays of blittable primitive types, such as an array of integers.
    /// However, a type that contains a variable array of blittable types is NOT itself blittable.<br/>
    /// * Formatted value types that contain only blittable types (and classes if they are marshalled as formatted types).
    /// </summary>
    public static bool IsBlittable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.Check(nameof(IsBlittable), m => CheckBlittable(m, null, null));
        return flag;
    }

    public static bool IsMarshalable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.Check(nameof(IsMarshalable), m => CheckMarshalable(m, null, null, null));
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

        visited ??= [];

        if (visited.Add(type) == false)
            ThrowBlittable(type, "circular referenced", path);

        foreach (var m in type.GetAllInstanceFields())
        {
            var name = m.GetAutoPropertyOrFieldName();
            var fieldPath = (path ?? "$") + "." + name;
            CheckBlittable(m.FieldType, visited, fieldPath);
        }

        CheckPinnable(type, visited, path);

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

                CheckBlittable(elementType, visited, path); // check if element type is pinnable as well.

                var array = Array.CreateInstance(elementType, 1);
                var entry = ObjectHelper.GetUninitializedObject(elementType);
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
                instance = ObjectHelper.GetUninitializedObject(type);
            }

            GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        }
    }

    private static void CheckMarshalable(Type type, FieldInfo? field, HashSet<Type>? visited, string? path)
    {
        type = type.UnwrapNullable();

        if (type.IsGenericType)
            ThrowMarshalable(type, "generic", path);

        if (type.IsAbstract)
            ThrowMarshalable(type, "abstract", path);

        if (type.IsEnum || Types.PrimitiveTypes.Contains(type))
            return;

        if (field is not null && field.IsDefined(typeof(MarshalAsAttribute), false))
            return;

        if (type.IsAutoLayout)
            ThrowMarshalable(type, "auto layout", path);

        if (type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            ThrowMarshalable(type, null, path);

        visited ??= [];

        if (visited.Add(type) == false)
            ThrowMarshalable(type, "circular referenced", path);

        foreach (var m in type.GetAllInstanceFields())
        {
            var name = m.GetAutoPropertyOrFieldName();
            var fieldPath = (path ?? "$") + "." + name;
            CheckMarshalable(m.FieldType, m, visited, fieldPath);
        }

        _ = Marshal.SizeOf(type);
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