namespace FclEx.Utils;

public static class ListAccessor<T>
{
    private static readonly Type _owner = typeof(ListAccessor<T>); // put it first.

    public static readonly Func<List<T>, T[]> Items = BuildItemsAccessor();
    public static readonly RefGetter<List<T>, int> Size = BuildSizeAccessor();
    public static readonly RefGetter<List<T>, int> Version = BuildVersionAccessor();

    private static Func<List<T>, T[]> BuildItemsAccessor()
    {
        return Accessor<List<T>>.BuildGetter<T[]>(_owner, "_items");
    }

    private static RefGetter<List<T>, int> BuildSizeAccessor()
    {
        return Accessor<List<T>>.BuildRefGetter<int>(_owner, "_size");
    }

    private static RefGetter<List<T>, int> BuildVersionAccessor()
    {
        return Accessor<List<T>>.BuildRefGetter<int>(_owner, "_version");
    }
}