namespace System.Collections.Generic;

public interface IEqualityComparerBuilder<in T>
{
    IEqualityComparer<T> Build();
}