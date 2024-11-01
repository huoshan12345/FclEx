using FclEx.Accessors;

namespace FclEx.Utils;

public static class SizeCalculator
{
    private static object GetUninitializedObject(Type type)
    {
        // for string type, GetUninitializedObject will throw an ArgumentException:
        // Uninitialized Strings cannot be created.
        // so we need to do special handling for it.
        return type == typeof(string)
            ? string.Empty
            : ObjectHelper.GetUninitializedObject(type);
    }

    private static int CalculateValueTypeInstance(Type type)
    {
        var fields = type.GetAllInstanceFields();
        if (fields.Length == 0)
            return 0;

        return UnsafeHelper.SizeOf(type);
    }

    private static int CalculateReferenceTypeInstance(Type type)
    {
        var fields = type.GetAllInstanceFields();

        // 如果指定的类型没有定义任何字段，CalculateReferenceTypeInstance 返回引用类型实例的最小字节数：3倍地址指针字节数。
        // 对于x86架构，一个应用类型对象至少占用12字节，包括 Object Header（4 bytes）、方法表指针（4 bytes）和最少4字节的字段内容（即使没有类型没有定义任何字段，这个4个字节也是必需的）。
        // 对于x64架构，这个最小字节数会变成24，因为方法表指针和最小字段内容变成了8个字节，虽然 Object Header 的有效内容只占用4个字节，但是前面会添加4个字节的Padding。
        if (fields.Length == 0)
            return 3 * IntPtr.Size;

        // TODO: GetUninitializedObject does work for abstract types and delegate types.
        var instance = GetUninitializedObject(type);
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref instance, type);
        Debug.Assert(addresses.Length == fields.Length);

        var ((firstAddress, _), (lastAddress, lastField)) = addresses.Zip(fields).MinMaxBy(m => m.First.ToInt64());
        var lastFieldOffset = (int)lastAddress.AbsDiff(firstAddress);
        var lastFieldSize = lastField.FieldType.IsValueType
            ? CalculateValueTypeInstance(lastField.FieldType)
            : IntPtr.Size;

        var size = lastFieldOffset + lastFieldSize + IntPtr.Size * 2; // plus sizes of two pointers for ObjectHeader and MethodTableAddress
        // Round up to IntPtr.Size
        var round = IntPtr.Size - 1;
        return ((size + round) & (~round));

        static IEnumerable<Type> GetBaseTypesAndThis(Type? type)
        {
            while (type is not null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }

    private static readonly ConcurrentDictionary<Type, int> _sizes = new();

    /// <summary>
    /// Retrieves the size of the specified type <paramref name="type"/>.
    /// </summary>
    /// <returns>
    /// The size in bytes of the specified type.<br/>
    /// * For value types, the size is the sum of the sizes of all its members.<br/>
    /// * For reference types, the size is the sum of the sizes of all its members plus the size of two pointers:<br/>
    /// one pointing to the object header and the other pointing to the method table.<br/>
    /// * The size of a member refers to the total size if the member is a value type,
    /// or the size of a pointer if the member is a reference type.<br/>
    /// * The size will be aligned to the size of a pointer, as the CLR performs memory alignment.
    /// </returns>
    public static int SizeOf(Type type)
    {
        return _sizes.GetOrAdd(type, SizeOfImpl);

        static int SizeOfImpl(Type type)
        {
            return type.IsValueType
                ? CalculateValueTypeInstance(type)
                : CalculateReferenceTypeInstance(type);
        }
    }

    /// <summary>
    /// Retrieves the size of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which to determine the size.</typeparam>
    /// <returns>
    /// The size in bytes of the specified type.<br/>
    /// * For value types, the size is the sum of the sizes of all its members.<br/>
    /// * For reference types, the size is the sum of the sizes of all its members plus the size of two pointers:<br/>
    /// one pointing to the object header and the other pointing to the method table.<br/>
    /// * The size of a member refers to the total size if the member is a value type,
    /// or the size of a pointer if the member is a reference type.<br/>
    /// * The size will be aligned to the size of a pointer, as the CLR performs memory alignment.
    /// </returns>
    public static int SizeOf<T>()
    {
        return SizeOf(typeof(T));
    }
}