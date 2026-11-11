using System.Text.Json.Serialization.Metadata;

namespace FclEx;

public class PropertyInfosTests
{
    [Fact]
    public void Empty_ProvidesCachedStaticResolverThatReturnsNoMetadata()
    {
        var property = PropertyInfos.JsonTypeInfoResolver.Empty;
        Assert.Same(property, PropertyInfos.JsonTypeInfoResolver.Empty);
        Assert.Equal(typeof(JsonTypeInfoResolver), property.DeclaringType);
        Assert.True(property.GetMethod!.IsStatic);
        var resolver = Assert.IsAssignableFrom<IJsonTypeInfoResolver>(property.GetValue(null));
        Assert.Same(resolver, property.GetValue(null));
        Assert.Null(resolver.GetTypeInfo(typeof(int), new JsonSerializerOptions()));
    }
}
