namespace FclEx.Helpers.ObjectHelperTests;

public class GetObjectIdTests
{
    [Fact]
    public void GetObjectId_SameInstance()
    {
        var obj = new object();
        Assert.Equal(ObjectHelper.GetObjectId(obj), ObjectHelper.GetObjectId(obj));
    }

    [Fact]
    public void GetObjectId_DifferentInstances()
    {
        var obj = new object();
        var obj2 = new object();
        Assert.NotEqual(ObjectHelper.GetObjectId(obj), ObjectHelper.GetObjectId(obj2));
    }
}