using System;
using System.Linq;

namespace FclEx.Utils;

public class CompositeDisposableTests
{
    public class Tester : IDisposable
    {
        public int Count { get; set; }

        public void Dispose()
        {
            Count = -1;
        }
    }

    [Fact]
    public void Create_Test()
    {
        var test = new Tester();

        test.Yield().Select(m =>
        {
            m.Count = 1;
            return m;
        }).AsComposite();

        Assert.Equal(1, test.Count);
    }

    [Fact]
    public void Dispose_Test()
    {
        var test = new Tester();
        using (test.Yield().AsComposite()) { }
        Assert.Equal(-1, test.Count);
    }
}