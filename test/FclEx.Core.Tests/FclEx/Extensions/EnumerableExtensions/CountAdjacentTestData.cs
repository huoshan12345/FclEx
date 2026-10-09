namespace FclEx.Extensions.EnumerableExtensions;

internal static class CountAdjacentTestData
{
    public static TheoryData<int[], (int Item, int Count)[]> Sequences => new()
    {
        { [], [] },
        { [1], [(1, 1)] },
        { [1, 1, 1], [(1, 3)] },
        { [1, 2, 3], [(1, 1), (2, 1), (3, 1)] },
        { [1, 1, 2, 2, 2, 1], [(1, 2), (2, 3), (1, 1)] },
        { [0, 0, -1, -1, 0], [(0, 2), (-1, 2), (0, 1)] }
    };

    public static void AssertCountIncrementBoundaryAndDisposal(
        Func<IEnumerable<int>, IEqualityComparer<int>, IEnumerable<(int Item, int Count)>> operation,
        bool overflow)
    {
        var source = new TrackingSource(1, 1);
        var incrementMethod = typeof(Check).Assembly.GetType("FclEx.Extensions.EnumerableExtensions", throwOnError: true)!
            .GetMethod("IncrementAdjacentCount", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(incrementMethod);
        var increment = (Func<int, int>)Delegate.CreateDelegate(typeof(Func<int, int>), incrementMethod);
        var comparisons = 0;
        var comparer = new CallbackComparer(() =>
        {
            comparisons++;
            Assert.Equal(int.MaxValue, increment(overflow ? int.MaxValue : int.MaxValue - 1));
            return true;
        });

        using (var enumerator = operation(source.Enumerate(), comparer).GetEnumerator())
        {
            if (overflow)
            {
                Assert.Throws<OverflowException>(() => enumerator.MoveNext());
            }
            else
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal((1, 2), enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }

            Assert.Equal(1, comparisons);
            Assert.Equal(1, source.Disposals);
        }
    }

    internal sealed class CallbackComparer(Func<bool> equals) : IEqualityComparer<int>
    {
        public bool Equals(int first, int second) => equals();

        public int GetHashCode(int value) => throw new NotSupportedException();
    }

    internal sealed class TrackingSource(params int[] items)
    {
        public int Passes { get; private set; }
        public int Visits { get; private set; }
        public int Disposals { get; private set; }

        public IEnumerable<int> Enumerate()
        {
            Passes++;
            try
            {
                foreach (var item in items)
                {
                    Visits++;
                    yield return item;
                }
            }
            finally
            {
                Disposals++;
            }
        }
    }
}
