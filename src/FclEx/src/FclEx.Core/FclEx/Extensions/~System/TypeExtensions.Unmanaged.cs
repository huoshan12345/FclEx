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

    private static (bool, Exception?) Check(this Type type, string name, Action<Type> action)
    {
        return _flagCache.GetOrAdd((type, name), m =>
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
        (var flag, ex) = type.Check(nameof(IsMarshalable), m => CheckMarshalable(m, null, null));
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
            Throw(type, null, path);

        // Exclude all generic types as well as nullable types. 
        if (type.IsGenericType)
            Throw(type, "generic", path);

        if (type.IsAbstract)
            Throw(type, "abstract", path);

        visited ??= [];

        if (visited.Add(type) == false)
            Throw(type, "circular referenced", path);

        foreach (var m in type.GetAllInstanceFields())
        {
            var name = m.TryGetCorrespondingProperty(out var property)
                ? property.Name
                : m.Name;
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
                    Throw(type, "multi-dimensional array", path);

                if (elementType.IsArray)
                    Throw(type, "nested array", path);

                CheckBlittable(elementType, visited, path); // check if element type is pinnable as well.

                var array = Array.CreateInstance(elementType, 1);
                var entry = ObjectHelper.GetUninitializedObject(elementType);
                array.SetValue(entry, 0);
                instance = array;
            }
            else if (type.IsAutoLayout) // don't do this check for array type.
            {
                Throw(type, "laid out automatically", path);
            }
            else
            {
                instance = ObjectHelper.GetUninitializedObject(type);
            }

            GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        }

        [DoesNotReturn]
        static void Throw(Type type, string? reason, string? path)
        {
            var error = StringBuilderHelper.Build(m =>
            {
                m.Append($"The type '{type.LongName()}'");
                if (path.IsNotEmpty())
                {
                    m.Append(" at ");
                    m.Append(path);
                }
                m.Append(" is not blittable");
                if (reason.IsNotEmpty())
                {
                    m.Append($" because it is {reason}");
                }
                m.Append('.');
            });
            throw new ArgumentException(error, nameof(type));
        }
    }

    private static void CheckMarshalable(Type type, FieldInfo? field, HashSet<Type>? visited)
    {
        type = type.UnwrapNullable();

        if (type.IsGenericType)
            Throw("generic");

        if (type.IsAbstract)
            Throw("abstract");

        if (type.IsEnum || Types.PrimitiveTypes.Contains(type))
            return;

        if (field is not null && field.IsDefined(typeof(MarshalAsAttribute), false))
            return;

        if (type.IsAutoLayout)
            Throw("auto layout");

        if (type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            Throw(null);

        _ = Marshal.SizeOf(type);

        visited ??= [];

        if (visited.Add(type) == false)
            Throw("circular referenced");

        foreach (var m in type.GetAllInstanceFields())
        {
            CheckMarshalable(m.FieldType, m, visited);
        }

        return;

        [DoesNotReturn]
        void Throw(string? reason)
        {
            var reasonSuffix = reason is null
                ? string.Empty
                : $" because it is {reason}";
            var error = $"The type '{type.LongName()}' is not marshalable{reasonSuffix}.";
            throw new ArgumentException(error, nameof(type));
        }
    }
}