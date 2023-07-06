namespace FclEx.Comparers;

public interface IComparerBuilder<in T>
{
    IComparer<T> Build();
}