namespace FclEx.Helpers;

public static class ObjectHelper
{
    public static object GetUninitializedObject(Type type)
    {
#if NETSTANDARD2_0
        return FormatterServices.GetUninitializedObject(type);
#else
        return RuntimeHelpers.GetUninitializedObject(type);
#endif
    }

    public static T GetUninitializedObject<T>()
    {
        return (T)GetUninitializedObject(typeof(T));
    }

    private static long _nextId;
    // ReSharper disable once UseCollectionExpression
    private static readonly ConditionalWeakTable<object, object> _objectIds = new();
    public static long GetObjectId<T>(T? obj) where T : class
    {
        return obj is null
            ? 0
            : (long)_objectIds.GetValue(obj, _ => Interlocked.Increment(ref _nextId));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DisposableValue<GCHandle> ToGCHandle(object? obj, GCHandleType type)
    {
        return Disposable.FromValue(GCHandle.Alloc(obj, type), m => m.Free());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CloneByJson<T>(T? obj, JsonSerializerOptions? options = null)
    {
        return obj is null ? obj : obj.ToJson(options).FromJson<T>(options);
    }

    public static byte[] MarshalToBytes<T>(T obj)
    {
        Check.NotNull(obj);

        var length = Marshal.SizeOf<T>();
        var bufByte = new byte[length];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        Marshal.StructureToPtr(obj, ptr, false);
        Marshal.Copy(ptr, bufByte, 0, length);
        return bufByte;
    }

    public static T? GetFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetRequiredField(fieldName, true);
        return field.GetValue<T>(obj);
    }

    public static T GetRequiredFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetRequiredField(fieldName, true);
        return field.GetRequiredValue<T>(obj);
    }
}