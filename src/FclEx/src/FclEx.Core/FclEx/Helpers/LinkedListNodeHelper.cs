namespace FclEx.Helpers;

public static class LinkedListNodeHelper
{
    public static LinkedListNode<T> Create<T>(T item)
    {
        return new LinkedListNode<T>(item);
    }
}