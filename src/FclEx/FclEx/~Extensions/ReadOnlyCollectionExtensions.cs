using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FclEx
{
    public static class ReadOnlyCollectionExtensions
    {
        ///// <summary>
        ///// Splits a collection of objects into an unknown number of pages with n items per page 
        ///// <para>(for example, if I have a list of 45 shoes and say 'shoes.Partition(10)' I will now have 4 pages of 10 shoes and 1 page of 5 shoes.</para>>
        ///// </summary>
        ///// <typeparam name="T">The type of object the collection should contain.</typeparam>
        ///// <param name="superset">The collection of objects to be divided into subsets.</param>
        ///// <param name="pageSize">The maximum number of items each page may contain.</param>
        ///// <returns>A subset of this collection of objects, split into pages of maximum size n.</returns>
        //public static IEnumerable<IEnumerable<T>> Partition<T>(this IReadOnlyCollection<T> superset, int pageSize)
        //{
        //    if (superset.Count < pageSize) yield return superset;
        //    else
        //    {
        //        var numberOfPages = Math.Ceiling(superset.Count / (double)pageSize);
        //        for (var i = 0; i < numberOfPages; i++)
        //            yield return superset.Skip(pageSize * i).Take(pageSize);
        //    }
        //}
    }
}
