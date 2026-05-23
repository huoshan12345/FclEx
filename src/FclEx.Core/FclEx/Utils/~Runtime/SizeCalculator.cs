namespace FclEx.Utils;

public static class SizeCalculator
{
    private static object GetUninitializedObject(Type type)
    {
        // for string type, GetUninitializedObject will throw an ArgumentException:
        // Uninitialized Strings cannot be created.
        // so we need to do special handling for it.
        if (type == typeof(string))
            return string.Empty;

        if (type.IsAssignableTo(typeof(Delegate)))
            return new Action(Console.WriteLine);

        return ObjectHelper.GetUninitializedObject(type);
    }

    private static int CalculateValueTypeInstance(Type type)
    {
        var fields = type.GetAllInstanceFields();
        return fields.Count == 0
            ? 0
            : UnsafeHelper.SizeOf(type);
    }

    private static int CalculateReferenceTypeInstance(Type type)
    {
        var fields = type.GetAllInstanceFields();

        // If the specified type does not define any fields, CalculateReferenceTypeInstance returns the minimum byte size for a reference type instance: 3 times the size of an address pointer.
        // On the x86 architecture, a reference type object occupies at least 12 bytes, including the Object Header (4 bytes), the method table pointer (4 bytes),
        //     and at least 4 bytes for field content (even if the type does not define any fields, these 4 bytes are still required).
        // On the x64 architecture, this minimum byte size increases to 24, as the method table pointer and minimum field content each take 8 bytes.
        //     Although the Object Header effectively only occupies 4 bytes, 4 bytes of padding are added before it.
        if (fields.Count == 0)
            return 3 * IntPtr.Size;

        var instance = GetUninitializedObject(type);
        var addresses = ObjectAccessor.GetAllFieldAddresses(ref instance, type);
        Debug.Assert(addresses.Length == fields.Count);

        // nint in netfx does not implement IComparable, so we need to use ToInt64 for comparison
        var ((firstAddress, _), (lastAddress, lastField)) = addresses.Zip(fields).MinMaxBy(m => m.First.ToInt64());
        var lastFieldOffset = (int)lastAddress.AbsDiff(firstAddress);
        var lastFieldSize = lastField.FieldType.IsValueType
            ? CalculateValueTypeInstance(lastField.FieldType)
            : IntPtr.Size;

        // plus sizes of two pointers for ObjectHeader and MethodTableAddress
        var size = lastFieldOffset + lastFieldSize + IntPtr.Size * 2;
        return size.RoundUpTo(IntPtr.Size);
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