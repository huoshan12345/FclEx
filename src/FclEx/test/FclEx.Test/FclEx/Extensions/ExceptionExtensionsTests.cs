namespace FclEx.Extensions;

public class ExceptionExtensionsTests
{
    internal class InnermostException : Exception
    {
        private static int _id;
        public int Id { get; }

        public InnermostException()
        {
            Id = Interlocked.Increment(ref _id);
        }
    }

    public static IEnumerable<object[]> Exceptions { get; } = new[]
    {
        new InnermostException(),
        new Exception("", new Exception("", new InnermostException())),
        new AggregateException("", new InnermostException(), new AggregateException(new InnermostException())),
    }.Select(m => new object[] { m }).ToArray();

    [Theory]
    [MemberData(nameof(Exceptions))]
    public void ForEach_Test(Exception ex)
    {
        var list = new List<InnermostException>();
        ex.ForEach(m =>
        {
            Assert.Equal(typeof(InnermostException), m.GetType());
            list.Add(m.CastTo<InnermostException>());
        });
        Assert.NotEmpty(list);
        var ids = list.Select(m => m.Id).Distinct().ToArray();
        Assert.Equal(ids.Length, list.Count);
    }

    [Theory]
    [MemberData(nameof(Exceptions))]
    public void GetInnermost_Test(Exception ex)
    {
        var inner = ex.GetInnermost();
        Assert.NotNull(inner);
        Assert.Null(inner.InnerException);
        Assert.Equal(typeof(InnermostException), inner.GetType());
    }

    [Fact]
    public void SetMessage_Test()
    {
        const string message = "xxxxxxxxxx";
        var ex = new SimpleException("x");
        ex.SetMessage(message);
        Assert.Equal(message, ex.Message);
    }
}