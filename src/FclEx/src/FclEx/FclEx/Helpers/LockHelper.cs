using System;

namespace FclEx.Helpers;

public static class LockHelper
{
    public static void DoubleCheckAndDo(Func<bool> condition, object lockObj, Action action)
    {
        if (!condition())
            return;

        lock (lockObj)
        {
            if (!condition())
                return;

            action();
        }
    }
}