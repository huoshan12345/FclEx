using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx.Extensions
{
    public static class IntPtrExtensions
    {
        public static ValueDisposable<IntPtr> AsDisposable(this IntPtr ptr)
        {
            return ptr.AsDisposable(Marshal.FreeHGlobal);
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
                    return (T)address.ToDelegate(args[^1], args[..^1]);
                }

                throw new ArgumentException("The specified Type must not be a generic type.", nameof(T));
            }
            else
            {
                return (T)Marshal.GetDelegateForFunctionPointer(address, type);
            }
        }
    }
}
