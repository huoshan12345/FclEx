namespace EasyCaching.Core.Serialization;

public class StringAsRawSerializerTests
{
    public record Model(string? Name, int Age);

    [Fact]
    public void Serialize_JsonSerializer_Test()
    {
        var ioc = new ServiceCollection()
            .AddEasyCaching(o => o.WithJson())
            .BuildServiceProvider();
        var jsonSerializer = ioc.GetRequiredService<IEasyCachingSerializer>();
        Assert.IsType<DefaultJsonSerializer>(jsonSerializer);
        var stringAsRawSerializer = new StringAsRawEasyCachingSerializer(jsonSerializer, Encoding.UTF8);

        var obj = new Model("xxxxxxxxx", 10);
        var bytes = stringAsRawSerializer.SerializeObject(obj);

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

        var newObj = stringAsRawSerializer.DeserializeObject(bytes);
        Assert.IsType<Model>(newObj);

        var newTypedObj = (Model)newObj;
        Assert.Equal(obj.Name, newTypedObj.Name);
        Assert.Equal(obj.Age, newTypedObj.Age);
    }
}