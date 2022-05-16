using System;
using System.Reflection;

namespace FclEx.Helpers
{
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class MethodHelper
    {
        public static MethodInfo GetMethodInfo(Action action) => action.Method;
        public static MethodInfo GetMethodInfo<TResult>(Func<TResult> func) => func.Method;
    }
}
