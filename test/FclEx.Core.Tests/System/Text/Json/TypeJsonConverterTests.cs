// ReSharper disable UnusedType.Local
// ReSharper disable UnusedTypeParameter
namespace System.Text.Json;

public class TypeJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TypeJsonConverter());
        return options;
    }

    [Fact]
    public void Serialize_Type_ShouldWriteAssemblyQualifiedName()
    {
        var options = CreateOptions();

        var json = JsonSerializer.Serialize(typeof(string), options);

        Assert.Contains(typeof(string).AssemblyQualifiedName!, json);
    }

    [Fact]
    public void Deserialize_ValidType_ShouldReturnType()
    {
        var options = CreateOptions();

        var typeName = typeof(int).AssemblyQualifiedName!;
        var json = $"\"{typeName}\"";

        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(typeof(int), result);
    }

    [Fact]
    public void RoundTrip_Type_ShouldBeEqual()
    {
        var options = CreateOptions();

        var original = typeof(Dictionary<string, int>);

        var json = JsonSerializer.Serialize(original, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(original, result);
    }

    [Fact]
    public void Deserialize_Null_ShouldReturnNull()
    {
        var options = CreateOptions();

        var json = "null";

        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_InvalidType_ShouldThrow()
    {
        var options = CreateOptions();

        var json = "\"Not.A.Real.Type\"";

        Assert.Throws<TypeLoadException>(() =>
            JsonSerializer.Deserialize<Type>(json, options));
    }

    [Fact]
    public void Deserialize_CaseInsensitive_ShouldWork()
    {
        var options = CreateOptions();

        var typeName = typeof(string).AssemblyQualifiedName!.ToLowerInvariant();
        var json = $"\"{typeName}\"";

        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void Deserialize_GenericType_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(List<int>);
        var json = JsonSerializer.Serialize(type, options);

        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    private class PrivateTestType;

    [Fact]
    public void Deserialize_OpenGenericType_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(List<>);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
        Assert.True(result!.IsGenericTypeDefinition);
    }

    [Fact]
    public void Deserialize_NestedGeneric_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(Dictionary<string, List<int?>>);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    [Fact]
    public void Deserialize_ArrayType_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(int[][]);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    [Fact]
    public void Deserialize_PrivateType_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(PrivateTestType);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    private class Container
    {
        private class HiddenType;
    }

    [Fact]
    public void Deserialize_ReflectedPrivateNestedType_ShouldWork()
    {
        var options = CreateOptions();

        var type = typeof(Container).GetNestedType(
            "HiddenType",
            BindingFlags.NonPublic);

        Assert.NotNull(type);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    private class GenericContainer
    {
        private class HiddenGeneric<T>;
    }

    [Fact]
    public void Deserialize_PrivateGenericType_ShouldWork()
    {
        var options = CreateOptions();

        var open = typeof(GenericContainer)
            .GetNestedType("HiddenGeneric`1", BindingFlags.NonPublic)!;

        var closed = open.MakeGenericType(typeof(int));

        var json = JsonSerializer.Serialize(closed, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(closed, result);
    }

    [Fact]
    public void Deserialize_BclPrivateNestedType_ShouldWork()
    {
        var options = CreateOptions();

        var enumerator = new List<int>().GetEnumerator();
        var type = enumerator.GetType();

        Assert.False(type.IsPublic);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }

    [Fact]
    public void Deserialize_XunitInternalReflectedType_ShouldWork()
    {
        var options = CreateOptions();

        var assembly = typeof(FactAttribute).Assembly;

        var type = assembly
            .GetTypes()
            .First(t => t is { IsPublic: false, IsNestedPublic: false });

        Assert.False(type.IsPublic);

        var json = JsonSerializer.Serialize(type, options);
        var result = JsonSerializer.Deserialize<Type>(json, options);

        Assert.Equal(type, result);
    }
}