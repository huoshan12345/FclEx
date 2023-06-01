using System.Collections.Generic;

namespace FclEx.Comparers;

internal class CommonComparer<T> : IComparer<T>
{
    private readonly Comparison<T> _compareFunc;

    public CommonComparer(Comparison<T> compareFunc)
    {
        _compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
    }

    public int Compare(T? x, T? y)
    {
        return _compareFunc(x!, y!);
    }
}