using System;
using System.Reflection;

namespace FclEx.Helpers;

public static partial class MethodHelper
{
    public static MethodInfo GetMethod(Action action) => action.Method;
    public static MethodInfo GetMethod<TResult>(Func<TResult> func) => func.Method;
}