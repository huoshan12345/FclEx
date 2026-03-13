namespace System.Collections.Generic.OrderedList;

public class SeqListMisc
{
    internal class Driver<T>
    {
        #region Contains

        public void BasicContains(T[] items)
        {
            var list = new OrderedList<T>(items);

            foreach (var m in items)
            {
                Assert.Contains(m, list);
            }
        }

        public void NonExistingValues(T[] itemsX, T[] itemsY)
        {
            var list = new OrderedList<T>(itemsX);

            foreach (var m in itemsY)
            {
                Assert.DoesNotContain(m, list); //"Should not contain item"
            }
        }

        public void RemovedValues(T[] items)
        {
            var list = new OrderedList<T>(items);
            foreach (var m in items)
            {
                list.Remove(m);
                Assert.DoesNotContain(m, list); //"Should not contain item"
            }
        }

        public void AddRemoveValues(T[] items)
        {
            var list = new OrderedList<T>(items);
            foreach (var m in items)
            {
                list.Add(m);
                list.Remove(m);
                list.Add(m);
                Assert.Contains(m, list); //"Should contain item."
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
                Assert.Contains(items[items.Length / 2], list); //"Should contain item."
                list.Remove(items[items.Length / 2]);
            }
            Assert.DoesNotContain(items[items.Length / 2], list); //"Should not contain item"
        }

        public void ContainsNullWhenReference(T[] items, T? value)
        {
            if (value != null)
            {
                throw new ArgumentException("invalid argument passed to testcase");
            }

            var list = new OrderedList<T>(items) { value! };
            Assert.Contains(value, list); //"Should contain item."
        }

        #endregion

        #region Clear

        public void ClearEmptyList()
        {
            var list = new OrderedList<T>();
            Assert.Empty(list); //"Should be equal to 0"
            list.Clear();
            Assert.Empty(list); //"Should be equal to 0."
        }

        public void ClearMultipleTimesEmptyList(int times)
        {
            var list = new OrderedList<T>();
            Assert.Empty(list); //"Should be equal to 0."
            for (var i = 0; i < times; i++)
            {
                list.Clear();
                Assert.Empty(list); //"Should be equal to 0."
            }
        }

        public void ClearNonEmptyList(T[] items)
        {
            var list = new OrderedList<T>(items);
            list.Clear();
            Assert.Empty(list); //"Should be equal to 0."
        }

        public void ClearMultipleTimesNonEmptyList(T[] items, int times)
        {
            var list = new OrderedList<T>(items);
            for (var i = 0; i < times; i++)
            {
                list.Clear();
                Assert.Empty(list); //"Should be equal to 0."
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
                Assert.Equal(arr[i], items[i]); //"Should be equal."
            }
        }

        public void EnsureNotUnderlyingToArray(T[] items, T item)
        {
            var list = new OrderedList<T>(items);
            var arr = list.ToArray();
            list[0] = item;
            if (arr[0] == null)
                Assert.NotNull(list[0]); //"Should NOT be null"
            else
                Assert.NotEqual(arr[0], list[0]); //"Should NOT be equal."
        }

        #endregion
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
            stringArr1[i] = "SomeTestString" + i.ToString();
        }
        var stringArr2 = new string[10];
        for (var i = 0; i < 10; i++)
        {
            stringArr2[i] = "SomeTestString" + (i + 10).ToString();
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
            stringArr[i] = "SomeTestString" + i.ToString();
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