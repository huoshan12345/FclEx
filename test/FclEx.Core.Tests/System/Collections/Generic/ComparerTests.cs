namespace System.Collections.Generic;

public class ComparerTests
{
    public class TestModel
    {
        private static int _id;
        public int Id { get; }

        public TestModel()
        {
            Id = Interlocked.Increment(ref _id);
        }
    }

    private static TestModel[] Generate()
    {
        var random = new Random(0);
        var testers = Enumerable.Repeat(() => new TestModel(), 100)
            .Select(m => m()).OrderBy(m => random.Next()).ToArray();
        return testers;
    }

    [Fact]
    public void KeyComparer_Test()
    {
        var testers = Generate();
        var testersOrdered = testers.OrderBy(m => m.Id).ToArray();
        {
            var comparer = KeyComparer<TestModel>.Create(m => m.Id);
            var sortList = new SortedSet<TestModel>(testers, comparer);
            Assert.True(testersOrdered.SequenceEqual(sortList));
        }
        {
            var comparer = KeyComparer<TestModel>.Create(m => m.Id);
            var sortList = new SortedSet<TestModel>(testers, comparer);
            Assert.True(testersOrdered.SequenceEqual(sortList));
        }
    }

    [Fact]
    public void CommonComparer_Test()
    {
        var testers = Generate();
        var testersOrdered = testers.OrderBy(m => m.Id).ToArray();
        var comparer = CommonComparer.Create<TestModel>((x, y) => Comparer<int>.Default.Compare(x.Id, y.Id));
        var sortList = new SortedSet<TestModel>(testers, comparer);
        Assert.True(testersOrdered.SequenceEqual(sortList));
    }
}