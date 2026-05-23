namespace System.Collections.Generic;

public interface IComparerBuilder<in T>
{
    IComparer<T> Build();
}