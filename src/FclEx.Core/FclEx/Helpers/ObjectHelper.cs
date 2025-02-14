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

    private static long _nextId;
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
        return GCHandle.Alloc(obj, type).ToDisposable(m => m.Free());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CloneByJson<T>(T? obj, JsonSerializerOptions? options = null)
    {
        return obj is null ? obj : obj.ToJson(options).FromJson<T>(options);
    }
}