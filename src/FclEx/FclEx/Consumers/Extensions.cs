using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Consumers
{
    public static class Extensions
    {
        public static void AddRange<T>(this IConsumer<T> consumer, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                consumer.Add(item);
            }
        }
    }
}
