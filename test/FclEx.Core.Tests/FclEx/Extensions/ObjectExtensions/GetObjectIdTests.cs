namespace FclEx.Extensions.ObjectExtensions;

public class GetObjectIdTests
{
    [Fact]
    public void SameInstance()
    {
        var obj = new object();
        Assert.Equal(obj.GetObjectId(), obj.GetObjectId());
    }

    [Fact]
    public void DifferentInstances()
    {
        var obj = new object();
        var obj2 = new object();
        Assert.NotEqual(obj.GetObjectId(), obj2.GetObjectId());
    }
}