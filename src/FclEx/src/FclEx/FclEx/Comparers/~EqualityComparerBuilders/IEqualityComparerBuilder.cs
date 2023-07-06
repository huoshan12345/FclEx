namespace FclEx.Comparers;

public interface IEqualityComparerBuilder<in T>
{
    IEqualityComparer<T> Build();
}