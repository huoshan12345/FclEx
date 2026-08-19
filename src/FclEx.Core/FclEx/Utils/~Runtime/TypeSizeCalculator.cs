namespace FclEx.Utils;

/// <summary>
/// Calculates the storage occupied by the declared instance fields of a type.
/// </summary>
public static class TypeSizeCalculator
{
    private static readonly ConditionalWeakTable<Type, ValueBox<int>> _instanceFieldStorageSizes = new();

    /// <summary>
    /// Gets the number of bytes required to store the instance fields of <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The closed, concrete, non-array type to inspect.</param>
    /// <returns>The combined storage size, in bytes, of all instance fields.</returns>
    /// <remarks>
    /// This is a shallow, type-level calculation. A value-type field contributes its inline managed size and a
    /// reference-type field contributes one pointer. Referenced objects, object and array headers, runtime-added
    /// variable-length data, padding, and object alignment are not included. Inherited instance fields are included.
    /// Consequently, this value is not the total allocation size reported by the CLR or a memory profiler.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="type"/> is an array, interface, abstract type, open generic type, pointer, by-reference type,
    /// or <see langword="void"/>.
    /// </exception>
    public static int GetInstanceFieldStorageSize(Type type)
    {
        Check.NotNull(type);
        ValidateType(type);
        return _instanceFieldStorageSizes.GetValue(type, m => CalculateInstanceFieldStorageSize(m));
    }

    /// <summary>
    /// Gets the number of bytes required to store the instance fields of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The closed, concrete, non-array type to inspect.</typeparam>
    /// <returns>The combined storage size, in bytes, of all instance fields.</returns>
    /// <remarks>
    /// This is a shallow, type-level calculation. A value-type field contributes its inline managed size and a
    /// reference-type field contributes one pointer. Referenced objects, object and array headers, runtime-added
    /// variable-length data, padding, and object alignment are not included. Inherited instance fields are included.
    /// Consequently, this value is not the total allocation size reported by the CLR or a memory profiler.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="T"/> is an array, interface, abstract type, open generic type, pointer, by-reference type,
    /// or <see langword="void"/>.
    /// </exception>
    public static int GetInstanceFieldStorageSize<T>()
    {
        return GetInstanceFieldStorageSize(typeof(T));
    }

    private static void ValidateType(Type type)
    {
        if (type.IsArray)
            throw new ArgumentException("Array storage depends on the array length and cannot be calculated from its type alone.", nameof(type));
        if (type.IsInterface)
            throw new ArgumentException("An interface does not define an instantiable data layout.", nameof(type));
        if (type.IsAbstract)
            throw new ArgumentException("An abstract type does not have a concrete instantiable data layout.", nameof(type));
        if (type.ContainsGenericParameters)
            throw new ArgumentException("An open generic type does not have a concrete data layout.", nameof(type));
        if (type.IsPointer || type.IsByRef || type == typeof(void))
            throw new ArgumentException("Pointer, by-reference, and void types do not have instance fields to measure.", nameof(type));
    }

    private static int CalculateInstanceFieldStorageSize(Type type)
    {
        if (type.IsValueType)
            return Unsafe.SizeOf(type);

        var size = 0;
        foreach (var field in type.GetAllInstanceFields())
        {
            var fieldSize = field.FieldType.IsValueType
                ? Unsafe.SizeOf(field.FieldType)
                : IntPtr.Size;
            size = checked(size + fieldSize);
        }
        return size;
    }
}
