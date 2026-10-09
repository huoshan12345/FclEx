using System.Reflection;

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

    public static void AssertCountBoundaryWithInjectedCounter(
        Func<IEnumerable<int>, IEqualityComparer<int>, IEnumerable<(int Item, int Count)>> operation,
        bool overflow)
    {
        var source = new TrackingSource(1, 1);
        IEnumerator<(int Item, int Count)>? enumerator = null;
        var comparer = new CallbackComparer(() =>
        {
            var countField = Assert.Single(enumerator!.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
                field => field.Name.StartsWith("<count>", StringComparison.Ordinal));
            Assert.Equal(1, Assert.IsType<int>(countField.GetValue(enumerator)));
            countField.SetValue(enumerator, overflow ? int.MaxValue : int.MaxValue - 1);
            return true;
        });

        using (enumerator = operation(source.Enumerate(), comparer).GetEnumerator())
        {
            if (overflow)
            {
                Assert.Throws<OverflowException>(() => enumerator.MoveNext());
            }
            else
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal((1, int.MaxValue), enumerator.Current);
                Assert.False(enumerator.MoveNext());
            }

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
