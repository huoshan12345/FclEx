using System;
using System.Collections.Generic;
using System.Linq;

namespace FclEx.Extensions.EnumerableExtensions;

public class ToOrderedDicTests
{
    private static void CheckIsOrdered<TKey, TValue>(ICollection<KeyValuePair<TKey, TValue>> col)
    {
        if (col.IsEmpty()) return;

        var cur = col.First().Key;
        var cmp = Comparer<TKey>.Default;
        foreach (var (key, _) in col.Skip(1))
        {
            Assert.True(cmp.Compare(cur, key) <= 0);
            cur = key;
        }
    }

    [Fact]
    public void ToOrderedDic_Test()
    {
        var dic = Enumerable.Range(1, 10).Select(m => KvPair.Create(m, m));
        var ordered = dic.ToOrderedDic();
        CheckIsOrdered(ordered);

        var random = new Random(12345);
        while (ordered.Any())
        {
            ordered.RemoveAt(random.Next(0, ordered.Count - 1));
            CheckIsOrdered(ordered);
        }
    }

    [Fact]
    public void ToOrderedDic_Selector_Test()
    {
        var dic = Enumerable.Range(1, 10);
        var ordered = dic.ToOrderedDic(x => x, x => x);
        CheckIsOrdered(ordered);

        var random = new Random(12345);
        while (ordered.Any())
        {
            ordered.RemoveAt(random.Next(0, ordered.Count - 1));
            CheckIsOrdered(ordered);
        }
    }

    [Fact]
    public void ToOrderedDic_Selector_Throw_Test()
    {
        var dic = Enumerable.Range(1, 10).Select(m => KvPair.Create(m % 5, m));
        Assert.Throws<ArgumentException>(() => dic.ToOrderedDic());
    }
}