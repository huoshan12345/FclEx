namespace System.Collections.Generic.OrderedList;

public class OrderedListTestsMisc
{
    internal class Driver<T>
    {
        #region GetRange

        public void BasicGetRange(T[] items, int index, int count)
        {
            var list = new OrderedList<T>(items);
            var range = list.AsSpan().Slice(index, count);

            //ensure range is good
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(range[i], items[i + index]);
            }

            //ensure no side effects
            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal(list[i], items[i]);
            }
        }

        public void BasicSliceSyntax(T[] items, int index, int count)
        {
            var list = new OrderedList<T>(items);
            var range = list.AsSpan()[index..(index + count)];

            //ensure range is good
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(range[i], items[i + index]);
            }

            //ensure no side effects
            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal(list[i], items[i]);
            }
        }

        public void EnsureRangeIsReference(T[] items, T item, int index, int count)
        {
            var list = new OrderedList<T>(items);
            var range = list.AsSpan()[index..(index + count)];
            var tempItem = list[index];
            range[0] = item;
            Assert.Equal(list[index], tempItem);
        }

        public void EnsureThrowsAfterModification(T[] items, T item, int index, int count)
        {
            var list = new OrderedList<T>(items);
            var range = list.AsSpan()[index..(index + count)];
            var tempItem = list[index];
            list[index] = item;

            Assert.Equal(range[0], tempItem);
        }

        public void GetRangeValidations(T[] items)
        {
            //
            //Always send items.Length is even
            //
            var list = new OrderedList<T>(items);
            var bad = new[]
            {
                (items.Length, 1),
                (items.Length + 1, 0),
                (items.Length + 1, 1),
                (items.Length, 2),
                (items.Length / 2, items.Length / 2 + 1),
                (items.Length - 1, 2),
                (items.Length - 2, 3),
                (1, items.Length),
                (0, items.Length + 1),
                (1, items.Length + 1),
                (2, items.Length),
                (items.Length / 2 + 1, items.Length / 2),
                (2, items.Length - 1),
                (3,items.Length - 2),
            };

            for (var i = 0; i < bad.Length; i++)
            {
                var i1 = i;
                var bad1 = bad;
                AssertExtensions.Throws<ArgumentException>(null, () =>
                {
                    var (index, count) = bad1[i1];
                    list.AsSpan().Slice(index, count);
                });
            }

            bad =
            [
                (-1, -1),
                (-1, 0),
                (-1, 1),
                (-1, 2),
                (0, -1),
                (1, -1),
                (2,-1),
            ];

            for (var i = 0; i < bad.Length; i++)
            {
                var i1 = i;
                var bad1 = bad;

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var (index, count) = bad1[i1];
                    list.AsSpan().Slice(index, count);
                });
            }
        }

        #endregion

        #region Contains

        public void BasicContains(T[] items)
        {
            var list = new OrderedList<T>(items);

            foreach (var item in items)
            {
                Assert.Contains(item, list);
            }
        }

        public void NonExistingValues(T[] itemsX, T[] itemsY)
        {
            var list = new OrderedList<T>(itemsX);

            foreach (var item in itemsY)
            {
                Assert.DoesNotContain(item, list);
            }
        }

        public void RemovedValues(T[] items)
        {
            var list = new OrderedList<T>(items);
            foreach (var item in items)
            {
                ((ICollection<T>)list).Remove(item);
                Assert.DoesNotContain(item, list);
            }
        }

        public void AddRemoveValues(T[] items)
        {
            var list = new OrderedList<T>(items);
            foreach (var item in items)
            {
                list.Add(item);
                ((ICollection<T>)list).Remove(item);
                list.Add(item);
                Assert.Contains(item, list);
            }
        }

        public void MultipleValues(T[] items, int times)
        {
            var list = new OrderedList<T>(items);

            for (var i = 0; i < times; i++)
            {
                list.Add(items[items.Length / 2]);
            }

            for (var i = 0; i < times + 1; i++)
            {
                Assert.Contains(items[items.Length / 2], list);
                ((ICollection<T>)list).Remove(items[items.Length / 2]);
            }
            Assert.DoesNotContain(items[items.Length / 2], list);
        }

        public void ContainsNullWhenReference(T[] items, T? value)
        {
            if (value != null)
            {
                throw new ArgumentException("invalid argument passed to testcase");
            }

            var list = new OrderedList<T>(items) { value! };
            Assert.Contains(value, list);
        }

        #endregion

        #region Clear

        public void ClearEmptyList()
        {
            OrderedList<T> list = [];
            Assert.Equal(0, list.Count);
            list.Clear();
            Assert.Equal(0, list.Count);
        }

        public void ClearMultipleTimesEmptyList(int times)
        {
            OrderedList<T> list = [];
            Assert.Equal(0, list.Count);
            for (var i = 0; i < times; i++)
            {
                list.Clear();
                Assert.Equal(0, list.Count);
            }
        }

        public void ClearNonEmptyList(T[] items)
        {
            var list = new OrderedList<T>(items);
            list.Clear();
            Assert.Equal(0, list.Count);
        }

        public void ClearMultipleTimesNonEmptyList(T[] items, int times)
        {
            var list = new OrderedList<T>(items);
            for (var i = 0; i < times; i++)
            {
                list.Clear();
                Assert.Equal(0, list.Count);
            }
        }

        #endregion

        #region ToArray

        public void BasicToArray(T[] items)
        {
            var list = new OrderedList<T>(items);
            var arr = list.ToArray();

            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal(arr[i], items[i]);
            }
        }

        public void EnsureNotUnderlyingToArray(T[] items, T item)
        {
            var list = new OrderedList<T>(items);
            var arr = list.ToArray();
            list[0] = item;

            if (arr[0] == null)
            {
                Assert.NotNull(list[0]);
            }
            else
            {
                Assert.NotEqual(arr[0], list[0]);
            }
        }

        #endregion
    }

    [Fact]
    public static void SlicingWorks()
    {
        var driver = new Driver<int>();
        var intArr1 = new int[100];
        for (var i = 0; i < 100; i++)
            intArr1[i] = i;

        driver.BasicSliceSyntax(intArr1, 50, 50);
        driver.BasicSliceSyntax(intArr1, 0, 50);
        driver.BasicSliceSyntax(intArr1, 50, 25);
        driver.BasicSliceSyntax(intArr1, 0, 25);
        driver.BasicSliceSyntax(intArr1, 75, 25);
        driver.BasicSliceSyntax(intArr1, 0, 100);
        driver.BasicSliceSyntax(intArr1, 0, 99);
        driver.BasicSliceSyntax(intArr1, 1, 1);
        driver.BasicSliceSyntax(intArr1, 99, 1);
    }

    [Fact]
    public static void ContainsTests()
    {
        var intDriver = new Driver<int>();
        var intArr1 = new int[10];
        for (var i = 0; i < 10; i++)
        {
            intArr1[i] = i;
        }

        var intArr2 = new int[10];
        for (var i = 0; i < 10; i++)
        {
            intArr2[i] = i + 10;
        }

        intDriver.BasicContains(intArr1);
        intDriver.NonExistingValues(intArr1, intArr2);
        intDriver.RemovedValues(intArr1);
        intDriver.AddRemoveValues(intArr1);
        intDriver.MultipleValues(intArr1, 3);
        intDriver.MultipleValues(intArr1, 5);
        intDriver.MultipleValues(intArr1, 17);


        var stringDriver = new Driver<string>();
        var stringArr1 = new string[10];
        for (var i = 0; i < 10; i++)
        {
            stringArr1[i] = "SomeTestString" + i;
        }
        var stringArr2 = new string[10];
        for (var i = 0; i < 10; i++)
        {
            stringArr2[i] = "SomeTestString" + (i + 10);
        }

        stringDriver.BasicContains(stringArr1);
        stringDriver.NonExistingValues(stringArr1, stringArr2);
        stringDriver.RemovedValues(stringArr1);
        stringDriver.AddRemoveValues(stringArr1);
        stringDriver.MultipleValues(stringArr1, 3);
        stringDriver.MultipleValues(stringArr1, 5);
        stringDriver.MultipleValues(stringArr1, 17);
        stringDriver.ContainsNullWhenReference(stringArr1, null);
    }

    [Fact]
    public static void ClearTests()
    {
        var intDriver = new Driver<int>();
        var intArr = new int[10];
        for (var i = 0; i < 10; i++)
        {
            intArr[i] = i;
        }

        intDriver.ClearEmptyList();
        intDriver.ClearMultipleTimesEmptyList(1);
        intDriver.ClearMultipleTimesEmptyList(10);
        intDriver.ClearMultipleTimesEmptyList(100);
        intDriver.ClearNonEmptyList(intArr);
        intDriver.ClearMultipleTimesNonEmptyList(intArr, 2);
        intDriver.ClearMultipleTimesNonEmptyList(intArr, 7);
        intDriver.ClearMultipleTimesNonEmptyList(intArr, 31);

        var stringDriver = new Driver<string>();
        var stringArr = new string[10];
        for (var i = 0; i < 10; i++)
        {
            stringArr[i] = "SomeTestString" + i;
        }

        stringDriver.ClearEmptyList();
        stringDriver.ClearMultipleTimesEmptyList(1);
        stringDriver.ClearMultipleTimesEmptyList(10);
        stringDriver.ClearMultipleTimesEmptyList(100);
        stringDriver.ClearNonEmptyList(stringArr);
        stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 2);
        stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 7);
        stringDriver.ClearMultipleTimesNonEmptyList(stringArr, 31);
    }
}