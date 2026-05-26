namespace FclEx.Caching;

public class CacheTests : CachingTests
{
    [Fact]
    public void Basic_Test()
    {
        const string str = "test";
        var cache = CacheManager.GetCache<string>(str);
        var obj = cache.Get(str, k => str);
        Assert.True(obj.HasValue);
        var objNew = cache.Get(str);
        Assert.True(objNew.HasValue);
        Assert.Equal(obj.Value, objNew.Value);

        cache.Remove(str);
        var objRemoved = cache.Get(str);
        Assert.False(objRemoved.HasValue);
    }
}