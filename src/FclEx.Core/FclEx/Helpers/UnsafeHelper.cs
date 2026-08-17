namespace FclEx.Helpers;

public static unsafe class UnsafeHelper
{
    private static readonly MethodInfo _sizeof = typeof(UnsafeHelper).GetRequiredMethod(nameof(SizeOfImpl), 1);
    private static readonly ConditionalWeakTable<Type, ValueBox<int>> _cache = new();

    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, MethodInfo>> _methods = new();

    /// <summary>
    /// Calculates the size, in bytes, of a specified type.
    /// </summary>
    /// <returns>The size of type in bytes.</returns>
    /// <remarks>
    /// This method works by creating a fixed buffer with two instances of type. 
    /// It then computes the memory distance between the addresses of these two elements to determine 
    /// the size of a single instance of type. This approach is particularly useful 
    /// for unmanaged or blittable types where the size is not easily obtainable otherwise.
    /// </remarks>
    public static int SizeOf(Type type)
    {
        return _cache.GetValue(type, m =>
        {
            var method = _sizeof.MakeGenericMethod(m);
            return method.Invoke<int>(null, null);
        });
    }

    /// <summary>
    /// Calculates the size, in bytes, of a specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which to determine the size in bytes.</typeparam>
    /// <returns>The size of type <typeparamref name="T"/> in bytes.</returns>
    /// <remarks>
    /// This method works by creating a fixed buffer with two instances of type <typeparamref name="T"/>. 
    /// It then computes the memory distance between the addresses of these two elements to determine 
    /// the size of a single instance of <typeparamref name="T"/>. This approach is particularly useful 
    /// for unmanaged or blittable types where the size is not easily obtainable otherwise.
    /// </remarks>
    public static int SizeOf<T>()
    {
        return SizeOf(typeof(T));
    }

    private static int SizeOfImpl<T>()
    {
        fixed (T* ptr = new T[2])
        {
            var ptrToT0 = new IntPtr(&ptr[0]);
            var ptrToT1 = new IntPtr(&ptr[1]);
            return (int)(ptrToT1.AbsDiff(ptrToT0));
        }
    }

    /// <summary>
    /// Writes the unmanaged representation of <paramref name="value"/> to <paramref name="dest"/>.
    /// </summary>
    /// <typeparam name="T">The unmanaged value type to write.</typeparam>
    /// <param name="dest">The destination address.</param>
    /// <param name="value">The value to write.</param>
    /// <remarks>
    /// Exactly <see cref="Unsafe.SizeOf{T}"/> bytes are written. The caller must ensure that
    /// <paramref name="dest"/> is non-null, writable, suitably aligned, and points to at least that many bytes.
    /// </remarks>
    // code from https://benbowen.blog/post/fun_with_makeref/
    public static void WriteTo<T>(IntPtr dest, T value) where T : unmanaged
    {
        var bytePtr = (byte*)dest;

        // This line gets a reference to value.
        // This would be like doing T* valuePtr = &value; if such a thing were allowed in C#.
        var valueRef = __makeref(value);

        // First of all we're getting a pointer to valueRef (so that's a reference to our reference),
        // and treating it as a pointer to an IntPtr instead of a pointer to a TypedReference.
        // This works because the first 4/8 bytes in the TypedReference struct are an IntPtr specifically the pointer to value.
        // Then we dereference that IntPtr pointer to a regular old IntPtr,
        // and finally cast that IntPtr to a byte* so we can use it in the copy code below.
        var valuePtr = (byte*)*((IntPtr*)&valueRef);

        var size = Unsafe.SizeOf<T>();
        for (var i = 0; i < size; ++i)
        {
            bytePtr[i] = valuePtr[i];
        }
    }

    /// <summary>
    /// Reads an unmanaged value from <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T">The unmanaged value type to read.</typeparam>
    /// <param name="source">The source address.</param>
    /// <returns>The value represented by the bytes at <paramref name="source"/>.</returns>
    /// <remarks>
    /// Exactly <see cref="Unsafe.SizeOf{T}"/> bytes are read. The caller must ensure that
    /// <paramref name="source"/> is non-null, readable, suitably aligned, and points to at least that many bytes.
    /// </remarks>
    public static T ReadFrom<T>(IntPtr source) where T : unmanaged
    {
        var bytePtr = (byte*)source;

        var result = default(T);
        var resultRef = __makeref(result);
        var resultPtr = (byte*)*((IntPtr*)&resultRef);

        var size = Unsafe.SizeOf<T>();
        for (var i = 0; i < size; ++i)
        {
            resultPtr[i] = bytePtr[i];
        }

        return result;
    }

    /// <summary>
    /// Reinterprets the raw unmanaged bytes of <paramref name="curValue"/> as <typeparamref name="TOut"/>.
    /// </summary>
    /// <typeparam name="TIn">The unmanaged source type.</typeparam>
    /// <typeparam name="TOut">The unmanaged destination type.</typeparam>
    /// <param name="curValue">The source value.</param>
    /// <returns>A value of type <typeparamref name="TOut"/> with the same byte representation.</returns>
    /// <exception cref="InvalidOperationException">
    /// <typeparamref name="TIn"/> and <typeparamref name="TOut"/> have different sizes.
    /// </exception>
    /// <remarks>No conversion, validation, byte-order adjustment, or numeric interpretation is performed.</remarks>
    public static TOut Reinterpret<TIn, TOut>(TIn curValue)
        where TIn : unmanaged
        where TOut : unmanaged
    {
        var size = Unsafe.SizeOf<TIn>();
        if (size != Unsafe.SizeOf<TOut>())
            throw new InvalidOperationException($"Cannot reinterpret from {typeof(TIn)} to {typeof(TOut)} because their sizes differ.");

        var result = default(TOut);

        var resultRef = __makeref(result);
        var resultPtr = (byte*)*((IntPtr*)&resultRef);

        var curValueRef = __makeref(curValue);
        var curValuePtr = (byte*)*((IntPtr*)&curValueRef);

        for (var i = 0; i < size; ++i)
        {
            resultPtr[i] = curValuePtr[i];
        }

        return result;
    }

    /// <summary>
    /// Dereferences a pointer and returns the value at the specified memory address.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of the value being dereferenced.</typeparam>
    /// <param name="ptr">A pointer to the memory address containing the value.</param>
    /// <returns>
    /// The value located at the memory address pointed to by <paramref name="ptr"/>.
    /// </returns>
    /// <remarks>
    /// This function interprets the memory address as a pointer to a value of type <typeparamref name="T"/>.
    /// <typeparamref name="T"/> is restricted to unmanaged types so an arbitrary address cannot be interpreted as a
    /// managed object reference. The caller remains responsible for ensuring that <paramref name="ptr"/> is non-null,
    /// suitably aligned, readable for <typeparamref name="T"/>, and valid for the duration of this call.
    /// </remarks>
    public static T? GetValue<T>(IntPtr ptr) where T : unmanaged
    {
        var pointer = ptr.ToPointer();
        return *(T*)pointer;
    }

    /// <summary>
    /// Dereferences a pointer and returns its value as the specified unmanaged runtime type.
    /// </summary>
    /// <param name="ptr">A pointer to the memory address containing the value.</param>
    /// <param name="type">The unmanaged runtime type of the value.</param>
    /// <returns>The value located at <paramref name="ptr"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not an unmanaged type.</exception>
    /// <remarks>Has the same pointer validity, alignment, and readability requirements as <see cref="GetValue{T}(IntPtr)"/>.</remarks>
    public static object? GetValue(IntPtr ptr, Type type)
    {
        var methods = _methods.GetValue(type, _ => new());
        var method = methods.GetOrAdd(nameof(GetValue), name =>
        {
            var methodDef = typeof(UnsafeHelper).GetRequiredMethod(name, 1, typeof(IntPtr));
            return methodDef.MakeGenericMethod(type);
        });
        return method.Invoke(null, [ptr]);
    }
}
