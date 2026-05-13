namespace FclEx;

public delegate Task AsyncEventHandler<in TSender>(TSender sender);

public delegate void RefAction<T, in TMember>(ref T obj, TMember value);

public delegate ref TMember RefGetter<in T, TMember>(T obj);

public unsafe delegate TMember* PtrGetter<in T, TMember>(T obj);

public delegate void ValueChangedHandler<in T>(T oldValue, T newValue);

public static class DelegateExtensions
{
    public static RefGetter<T, TMember> AsRef<T, TMember>(this PtrGetter<T, TMember> getter)
    {
        return obj =>
        {
            unsafe
            {
                var ptr = getter(obj);
                return ref Unsafe.AsRef<TMember>(ptr);
            }
        };
    }
}