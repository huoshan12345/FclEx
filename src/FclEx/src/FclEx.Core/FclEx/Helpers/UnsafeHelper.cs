namespace FclEx.Helpers;

public static unsafe class UnsafeHelper
{
    public static int SizeOf<T>()
    {
        return Unsafe.SizeOf<T>();
    }

    private static readonly MethodInfo _sizeof = typeof(Unsafe).GetRequiredMethod(nameof(Unsafe.SizeOf), 1);
    private static readonly ConcurrentDictionary<Type, int> _cache = new();

    public static int SizeOf(Type type)
    {
        return _cache.GetOrAdd(type, m =>
        {
            var method = _sizeof.MakeGenericMethod(m);
            return method.Invoke<int>(null, null);
        });
    }

    public static int SizeOf2<T>()
    {
        fixed (T* ptr = new T[2])
        {
            var ptrToT0 = (byte*)(&ptr[0]);
            var ptrToT1 = (byte*)(&ptr[1]);
            return (int)(((byte*)ptrToT1) - ((byte*)ptrToT0));
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

    public static IntPtr GetActualAddress<T>(ref T obj)
    {
        var pointer = Unsafe.AsPointer<T>(ref obj);
        return typeof(T).IsValueType
            ? new IntPtr(pointer)
            : *(IntPtr*)pointer; // the address of method table.
    }
}