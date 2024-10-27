namespace FclEx.Helpers;

// code from https://stackoverflow.com/a/53029501/4255140
public static class AddressHelper
{
    private static readonly object _lock = new();
    private static readonly ObjectReinterpreter _reinterpreter = new()
    {
        AsObject = new ObjectWrapper(),
    };

    public static IntPtr GetAddress(object obj)
    {
        lock (_lock)
        {
            _reinterpreter.AsObject.Object = obj;
            var address = _reinterpreter.AsIntPtr.Value;
            _reinterpreter.AsObject.Object = null;
            return address;
        }
    }

    public static T? GetInstance<T>(IntPtr address)
    {
        lock (_lock)
        {
            _reinterpreter.AsIntPtr.Value = address;
            var obj = (T?)_reinterpreter.AsObject.Object;
            _reinterpreter.AsObject.Object = null;
            return obj;
        }
    }

    // I bet you thought C# was type-safe.
    [StructLayout(LayoutKind.Explicit)]
    private struct ObjectReinterpreter
    {
        [FieldOffset(0)] public ObjectWrapper AsObject;
        [FieldOffset(0)] public IntPtrWrapper AsIntPtr;
    }

    private class ObjectWrapper
    {
        public object? Object;
    }

    private class IntPtrWrapper
    {
        public IntPtr Value;
    }
}