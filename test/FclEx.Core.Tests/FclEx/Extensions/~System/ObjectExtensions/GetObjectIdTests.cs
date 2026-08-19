namespace FclEx.Extensions.ObjectExtensions;

public class GetObjectIdTests
{
    [Fact]
    public void GetObjectId_SameInstance()
    {
        var obj = new object();
        Assert.Equal(object.GetObjectId(obj), object.GetObjectId(obj));
    }

    [Fact]
    public void GetObjectId_DifferentInstances()
    {
        var obj = new object();
        var obj2 = new object();
        Assert.NotEqual(object.GetObjectId(obj), object.GetObjectId(obj2));
    }
}