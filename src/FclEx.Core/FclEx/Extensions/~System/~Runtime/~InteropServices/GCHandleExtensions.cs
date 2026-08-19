namespace FclEx.Extensions;

public static class GCHandleExtensions
{
    extension(GCHandle)
    {
        [MethodImpl(AggressiveInlining)]
        public static DisposableValue<GCHandle> Create(object? obj, GCHandleType type)
        {
            return Disposable.FromValue(GCHandle.Alloc(obj, type), m => m.Free());
        }
    }
}
