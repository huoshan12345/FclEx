namespace FclEx.Extensions;

public static class LinkedListExtensions
{
    extension<T>(LinkedList<T> list)
    {
        public void operator +=(T item)
        {
            list.AddLast(item);
        }

        public void operator +=(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.AddLast(item);
            }
        }
    }

    extension<T>(LinkedList<T>)
    {
        public static LinkedList<T> operator +(LinkedList<T> list, T item)
        {
            return new(list.Append(item));
        }

        public static LinkedList<T> operator +(T item, LinkedList<T> list)
        {
            return new(list.Prepend(item));
        }

        public static LinkedList<T> operator +(LinkedList<T> list, IEnumerable<T> items)
        {
            return new(list.Concat(items));
        }

        public static LinkedList<T> operator +(IEnumerable<T> items, LinkedList<T> list)
        {
            return new(items.Concat(list));
        }
    }
}
