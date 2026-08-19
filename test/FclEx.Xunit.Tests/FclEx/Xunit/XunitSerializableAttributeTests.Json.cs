namespace FclEx.Xunit;

public class SelectorJsonConverter<T> : JsonConverter<Expression<Func<T, object?>>>
{
    private readonly record struct SelectorInfo(string? ParamName, string MemberName)
    {
        public override string ToString() => $"{ParamName}\0{MemberName}";

        public static SelectorInfo FromString(string str)
        {
            var parts = str.Partition("\0");
            return new SelectorInfo(parts.Left, parts.Right);
        }
    }

    public override Expression<Func<T, object?>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected a string token but got {reader.TokenType}.");

        var str = reader.GetString();
        if (str.IsNullOrEmpty())
            return null;

        var info = SelectorInfo.FromString(str);
        var parameter = Expression.Parameter(typeof(string), info.ParamName);
        var body = Expression.PropertyOrField(parameter, info.MemberName);
        return Expression.Lambda<Func<T, object?>>(Expression.Convert(body, typeof(object)), parameter);
    }

    public override void Write(Utf8JsonWriter writer, Expression<Func<T, object?>>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var member = Expression.GetDataMember(value);
        var info = new SelectorInfo(value.Parameters[0].Name, member.Name);

        writer.WriteStringValue(info.ToString());
    }
}

partial class XunitSerializableAttributeTests
{
    public interface IJsonTestModel<T>
    {
        Expression<Func<T, object>>? Selector { get; }
        Type? Type { get; }
        object? Value { get; }
        bool Valid { get; }
    }

    [XunitSerializable]
    public partial record WithJsonIgnoreOnField<T>(
        [field: JsonIgnore]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record WithJsonIgnoreOnProperty<T>(
        [property: JsonIgnore]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record WithJsonIgnoreConverterOnField<T>(
        [field: JsonConverter(typeof(IgnoreJsonConverter))]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record WithJsonIgnoreConverterOnProperty<T>(
        [property: JsonConverter(typeof(IgnoreJsonConverter))]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record WithJsonConverterOnField(
        [field: JsonConverter(typeof(SelectorJsonConverter<string>))]
        Expression<Func<string, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<string>;

    [XunitSerializable]
    public partial record WithJsonConverterOnProperty(
        [property: JsonConverter(typeof(SelectorJsonConverter<string>))]
        Expression<Func<string, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<string>;

    [XunitSerializable]
    public partial record WithFileSystemInfo(string[] Sources, (string Path, string Namespace, string[] UsedPaths)[] Targets)
    {
        public FileInfo[] SourceFiles { get; } = Sources.Select(m => new FileInfo(m)).ToArray();
        public DirectoryInfo[] SourceDirs { get; } = Sources.Select(m => new FileInfo(m).Directory).NotNull().ToArray();

        public override string ToString()
        {
            return StringBuilder.Build(m =>
            {
                m.AppendSquareBracketed(x => x.AppendJoin(", ", SourceFiles.Select(a => a.Name)));
                m.Append(", ");
                m.AppendSquareBracketed(x => x.AppendJoin(", ", SourceDirs.Select(a => a.Name)));
            });
        }
    }

    private static void AssertTestModel<TModel, T>(Func<TModel> creator, bool checkSelector = false) where TModel : IJsonTestModel<T>, IXunitSerializable
    {
        var original = creator();
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = RuntimeHelpers.GetUninitializedObject<TModel>();
        deserialized.Deserialize(info);

#if FCLEX_XUNIT_V3
        if (checkSelector)
        {
            Assert.Equal(original.Selector?.ToString(), deserialized.Selector?.ToString());
        }
        else
        {
            Assert.Null(deserialized.Selector);
        }
#else
        Assert.Equal(original.Selector, deserialized.Selector);
#endif

        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Equal(original.Valid, deserialized.Valid);
    }

    [Fact]
    public void Should_RoundTrip_WithJsonIgnoreOnField()
    {
        AssertTestModel<WithJsonIgnoreOnField<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<WithJsonIgnoreOnField<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_WithJsonIgnoreConverterOnField()
    {
        AssertTestModel<WithJsonIgnoreConverterOnField<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<WithJsonIgnoreConverterOnField<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_WithJsonIgnoreOnProperty()
    {
        AssertTestModel<WithJsonIgnoreOnProperty<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<WithJsonIgnoreOnProperty<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_WithJsonIgnoreConverterOnProperty()
    {
        AssertTestModel<WithJsonIgnoreConverterOnProperty<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<WithJsonIgnoreConverterOnProperty<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_WithJsonConverterOnField()
    {
        AssertTestModel<WithJsonConverterOnField, string>(() => new(m => m.Length, typeof(string), 1), checkSelector: true);
    }

    [Fact]
    public void Should_RoundTrip_WithJsonConverterOnProperty()
    {
        AssertTestModel<WithJsonConverterOnProperty, string>(() => new(m => m.Length, typeof(string), 1), checkSelector: true);
    }

    [Fact]
    public void Should_RoundTrip_WithFileSystemInfo()
    {
        var original = new WithFileSystemInfo(["source"], [("path", "namespace", ["used-path"])]);
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = new WithFileSystemInfo([], []);
        deserialized.Deserialize(info);

        Assert.Equal(original.Sources, deserialized.Sources);
        Assert.MembersEqual(original.Targets, deserialized.Targets); // use member equality for tuples instead of structural equality

        var comparer = FileSystemInfoEqualityComparer.CaseSensitive;
        Assert.Equal(original.SourceFiles, deserialized.SourceFiles, comparer);
        Assert.Equal(original.SourceDirs, deserialized.SourceDirs, comparer);

        Assert.Equal(original.ToString(), deserialized.ToString());
    }
}
