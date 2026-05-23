namespace FclEx.Utils;

public class CompositeDisposableTests
{
    public class Tester : IDisposable
    {
        public int Count { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Count = -1;
        }
    }

    [Fact]
    public void Create_Test()
    {
        var test = new Tester();

        new[] { test }.Select(m =>
        {
            m.Count = 1;
            return m;
        }).Merge();

        Assert.Equal(1, test.Count);
    }

    [Fact]
    public void Dispose_Test()
    {
        var test = new Tester();
        using (new[] { test }.Merge()) { }
        Assert.Equal(-1, test.Count);
    }
}