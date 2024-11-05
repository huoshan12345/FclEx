namespace FclEx.Helpers;

public static unsafe class UnsafeHelper
{
    private static readonly MethodInfo _sizeof = typeof(UnsafeHelper).GetRequiredMethod(nameof(SizeOfImpl), 1);
    private static readonly ConcurrentDictionary<Type, int> _cache = new();

    private static readonly ConcurrentDictionary<(Type, string), MethodInfo> _methods = new();

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
        return _cache.GetOrAdd(type, m =>
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

    // code from https://benbowen.blog/post/fun_with_makeref/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteTo<T>(IntPtr dest, T value, int sizeOfT) where T : struct
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

        for (var i = 0; i < sizeOfT; ++i)
        {
            bytePtr[i] = valuePtr[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadFrom<T>(IntPtr source, int sizeOfT) where T : struct
    {
        var bytePtr = (byte*)source;

        var result = default(T);
        var resultRef = __makeref(result);
        var resultPtr = (byte*)*((IntPtr*)&resultRef);

        for (var i = 0; i < sizeOfT; ++i)
        {
            resultPtr[i] = bytePtr[i];
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut Reinterpret<TIn, TOut>(TIn curValue, int sizeBytes)
        where TIn : struct
        where TOut : struct
    {
        var result = default(TOut);

        var resultRef = __makeref(result);
        var resultPtr = (byte*)*((IntPtr*)&resultRef);

        var curValueRef = __makeref(curValue);
        var curValuePtr = (byte*)*((IntPtr*)&curValueRef);

        for (var i = 0; i < sizeBytes; ++i)
        {
            resultPtr[i] = curValuePtr[i];
        }

        return result;
    }

    /// <summary>
    /// Dereferences a pointer and returns the value at the specified memory address.
    /// </summary>
    /// <typeparam name="T">The type of the value being dereferenced.</typeparam>
    /// <param name="ptr">A pointer to the memory address containing the value.</param>
    /// <returns>
    /// The value located at the memory address pointed to by <paramref name="ptr"/>.
    /// </returns>
    /// <remarks>
    /// This function interprets the memory address as a pointer to a value of type <typeparamref name="T"/>.
    /// Use with caution, as dereferencing an invalid or misaligned pointer can lead to runtime errors or undefined behavior.
    /// </remarks>
    public static T? GetValue<T>(IntPtr ptr)
    {
        var pointer = ptr.ToPointer();
        return *(T*)pointer;
    }
    
    public static object? GetValue(IntPtr ptr, Type type)
    {
        var method = _methods.GetOrAdd((type, nameof(GetValue)), m =>
        {
            var methodDef = typeof(UnsafeHelper).GetRequiredMethod(m.Item2, 1, typeof(IntPtr));
            return methodDef.MakeGenericMethod(type);
        });
        return method.Invoke(null, [ptr]);
    }
}