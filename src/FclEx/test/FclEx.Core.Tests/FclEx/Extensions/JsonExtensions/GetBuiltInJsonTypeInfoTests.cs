using System.Text.Json.Serialization.Metadata;
using FclEx.Json;

namespace FclEx.Extensions.JsonExtensions;

public class GetBuiltInJsonTypeInfoTests
{
    [Fact]
    public void GetBuiltInJsonTypeInfo_Test()
    {
        var option = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var info = option.TypeInfoResolver.GetTypeInfo(typeof(Unit), option);
        Assert.NotNull(info);
        Assert.IsType<IgnoreJsonConverterImpl<Unit>>(info.Converter);

        var builtInInfo = option.GetBuiltInJsonTypeInfo(typeof(Unit));
        Assert.IsNotType<IgnoreJsonConverterImpl<Unit>>(builtInInfo.Converter);
        Assert.Equal("ObjectDefaultConverter<Unit>", builtInInfo.Converter.GetType().ShortName());
    }

    [Fact]
    public void GetBuiltInJsonTypeInfo_T_Test()
    {
        var option = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var builtInInfo = option.GetBuiltInJsonTypeInfo<Unit>();
        Assert.Equal("ObjectDefaultConverter<Unit>", builtInInfo.Converter.GetType().ShortName());
    }
}