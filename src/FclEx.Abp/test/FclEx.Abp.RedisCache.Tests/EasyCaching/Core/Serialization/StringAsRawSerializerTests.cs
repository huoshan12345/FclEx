using EasyCaching.Core.Internal;
using EasyCaching.Serialization.SystemTextJson;

namespace EasyCaching.Core.Serialization;

public class StringAsRawSerializerTests
{
    public record Model(string? Name, int Age);

    // wait for fixing on https://github.com/dotnetcore/EasyCaching/pull/561 to use WithSystemTextJson
    [Fact]
    public void Serialize_JsonSerializer_Test()
    {
        var ioc = new ServiceCollection()
            .AddEasyCaching(o => o.WithPatchedSystemTextJson())
            .BuildServiceProvider();

        var jsonSerializer = ioc.GetRequiredService<IEasyCachingSerializer>();
        // Assert.IsType<DefaultJsonSerializer>(jsonSerializer);
        Assert.IsType<PatchedJsonSerializer> (jsonSerializer);
        var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(jsonSerializer, Encoding.UTF8);

        var obj = new Model("xxxxxxxxx", 10);
        var bytes = stringAsRawSerializer.SerializeObject(obj);
        string str = TypeHelper.BuildTypeName(obj.GetType());

        var newObj = stringAsRawSerializer.DeserializeObject(bytes);
        Assert.IsType<Model>(newObj);

        var newTypedObj = (Model)newObj;
        Assert.Equal(obj.Name, newTypedObj.Name);
        Assert.Equal(obj.Age, newTypedObj.Age);
    }

    [Fact]
    public void Serialize_MessagePack_Test()
    {
        var provider = new ServiceCollection()
            .AddEasyCaching(o => o.WithMessagePack())
            .BuildServiceProvider();

        var jsonSerializer = provider.GetRequiredService<IEasyCachingSerializer>();
        Assert.IsType<DefaultMessagePackSerializer>(jsonSerializer);
        var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(jsonSerializer, Encoding.UTF8);

        var obj = new Model("xxxxxxxxx", 10);
        var bytes = stringAsRawSerializer.SerializeObject(obj);
        var str = bytes.GetString();

        var newObj = stringAsRawSerializer.DeserializeObject(bytes);
        Assert.IsType<Model>(newObj);

        var newTypedObj = (Model)newObj;
        Assert.Equal(obj.Name, newTypedObj.Name);
        Assert.Equal(obj.Age, newTypedObj.Age);
    }
}