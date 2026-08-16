namespace FclEx.Extensions;

public partial class ExceptionExtensionsTests
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

    [Fact]
    public void Enumerate_ShouldReturnEveryExceptionOnce()
    {
        var sharedLeaf = new InnermostException();
        var sharedBranch = new Exception("shared", sharedLeaf);
        var root = new AggregateException(sharedBranch, sharedBranch);

        var exceptions = root.Enumerate().ToArray();

        Assert.Equal([root, sharedBranch, sharedLeaf], exceptions);
    }

    [Fact]
    public void EnumerateLeaves_ShouldReturnOnlyLeavesOnce()
    {
        var firstLeaf = new InnermostException();
        var sharedLeaf = new InnermostException();
        var sharedBranch = new Exception("shared", sharedLeaf);
        var root = new AggregateException(firstLeaf, sharedBranch, sharedBranch);

        var leaves = root.EnumerateLeaves().ToArray();

        Assert.Equal([firstLeaf, sharedLeaf], leaves);
    }

    [Fact]
    public void EnumerateLeaves_LeafRoot_ShouldReturnRoot()
    {
        var root = new InnermostException();

        Assert.Equal([root], root.EnumerateLeaves());
    }

    [Fact]
    public void GetInnermost_ShouldReturnLastExceptionInInnerExceptionChain()
    {
        var innermost = new InnermostException();
        var root = new Exception("root", new Exception("middle", innermost));

        Assert.Same(innermost, root.GetInnermost());
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
