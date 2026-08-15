namespace FclEx.Extensions;

public static class IntPtrExtensions
{
    public static DisposableValue<IntPtr> ToDisposable(this IntPtr ptr, Action<IntPtr> freeAction)
    {
        return Disposable.FromValue(ptr, freeAction);
    }

    public static string ToHexString(this IntPtr ptr)
    {
        var value = ptr.ToInt64();
        return "0x" + value.ToString("X" + IntPtr.Size * 2);
    }

    /// <summary>
    /// Calculates the absolute difference in bytes between two <see cref="IntPtr"/> values.
    /// </summary>
    /// <param name="ptr">The first pointer.</param>
    /// <param name="other">The second pointer to compare with.</param>
    /// <returns>The absolute difference in bytes between <paramref name="ptr"/> and <paramref name="other"/>.</returns>
    public static nuint AbsDiff(this IntPtr ptr, IntPtr other)
    {
        if (IntPtr.Size == sizeof(int))
        {
            var left = unchecked((uint)ptr.ToInt32());
            var right = unchecked((uint)other.ToInt32());
            return left >= right ? left - right : right - left;
        }

        var left64 = unchecked((ulong)ptr.ToInt64());
        var right64 = unchecked((ulong)other.ToInt64());
        return left64 >= right64
            ? (nuint)(left64 - right64)
            : (nuint)(right64 - left64);
    }

    /// <summary>
    /// Calculates the difference between two memory addresses represented by <see cref="IntPtr"/> values.
    /// </summary>
    /// <param name="ptr">The starting <see cref="IntPtr"/> address.</param>
    /// <param name="other">The <see cref="IntPtr"/> address to subtract from <paramref name="ptr"/>.</param>
    /// <returns>
    /// The difference, in bytes, between <paramref name="ptr"/> and <paramref name="other"/> as a <see cref="long"/>.
    /// </returns>
    public static long Subtract(this IntPtr ptr, IntPtr other)
    {
        return ptr.ToInt64() - other.ToInt64();
    }

    public static T? MarshalTo<T>(this IntPtr ptr)
    {
        return Marshal.PtrToStructure<T>(ptr);
    }

    public static Delegate ToDelegate(this IntPtr address, Type returnType, IEnumerable<Type> parameters)
    {
        return Marshal.GetDelegateForFunctionPointer(address, DelegateHelper.MakeNewCustomDelegate(returnType, parameters));
    }

    public static T ToDelegate<T>(this IntPtr address) where T : Delegate
    {
        var type = typeof(T);
        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            var def = type.GetGenericTypeDefinition();

            if (TypeHelper.ActionTypes.Contains(def) && def.Assembly == AssemblyHelper.AssemblyOfAction)
            {
                return (T)address.ToDelegate(typeof(void), args);
            }

            if (TypeHelper.FuncTypes.Contains(def) && def.Assembly == AssemblyHelper.AssemblyOfFunc)
            {
                return (T)address.ToDelegate(args.Last(), args.Take(args.Length - 1));
            }

            throw new ArgumentException("The specified Type must not be a generic type.", nameof(T));
        }
        else
        {
            return (T)Marshal.GetDelegateForFunctionPointer(address, type);
        }
    }
}
