namespace FclEx.Extensions;

public static class LinkedListExtensions
{
    extension<T>(LinkedList<T> list)
    {
        public void operator +=(LinkedList<T> other)
        {
            foreach (var item in other)
            {
                list.AddLast(item);
            }
        }
    }

    extension<T>(LinkedList<T>)
    {
        public static LinkedList<T> operator +(LinkedList<T> list, LinkedList<T> other)
        {
            return new(list.Concat(other));
        }

        public static LinkedList<T> operator +(LinkedList<T> list, T item)
        {
            list.AddLast(item);
            return list;
        }

        public static LinkedList<T> operator +(LinkedList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.AddLast(item);
            }
            return list;
        }

        public static LinkedList<T> operator +(T item, LinkedList<T> list)
        {
            list.AddFirst(item);
            return list;
        }

        public static LinkedList<T> operator +(IEnumerable<T> items, LinkedList<T> list)
        {
            var p = list.First;
            if (p is null)
            {
                foreach (var item in items)
                {
                    list.AddLast(item);
                }
            }
            else
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var item in items)
                {
                    p = list.AddAfter(p, item);
                }
            }

            return list;
        }
    }
}
