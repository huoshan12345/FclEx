
namespace FclEx.Extensions.Reflection.FieldInfoExtensions;

public class TryGetAutoPropertyTests
{
    [Fact]
    public void AutoPropertyBackingField_ShouldReturnProperty()
    {
        var field = typeof(TestClass).GetField(
            "<AutoProperty>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(TestClass.AutoProperty), property!.Name);
    }

    [Fact]
    public void ReadOnlyAutoBackingField_ShouldReturnProperty()
    {
        var field = typeof(TestClass).GetField(
            "<ReadOnlyAuto>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(TestClass.ReadOnlyAuto), property!.Name);
    }

    [Fact]
    public void InitOnlyBackingField_ShouldReturnProperty()
    {
        var field = typeof(TestClass).GetField(
            "<InitOnly>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(TestClass.InitOnly), property!.Name);
    }

    [Fact]
    public void StaticAutoBackingField_ShouldReturnProperty()
    {
        var field = typeof(TestClass).GetField(
            "<StaticAuto>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(TestClass.StaticAuto), property!.Name);
    }

    [Fact]
    public void StructAutoPropertyBackingField_ShouldReturnProperty()
    {
        var field = typeof(TestStruct).GetField(
            "<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Value", property!.Name);
    }

    [Fact]
    public void InheritedAutoPropertyBackingField_ShouldReturnProperty()
    {
        var field = typeof(BaseClass).GetField(
            "<BaseAuto>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(BaseClass.BaseAuto), property!.Name);
    }

    [Fact]
    public void NormalField_ShouldReturnFalse()
    {
        var field = typeof(TestClass).GetField(
            "_field",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.False(result);
        Assert.Null(property);
    }

    [Fact]
    public void InheritedBackingField_ShouldResolveProperty()
    {
        var field = typeof(DerivedClass).GetRequiredField("<BaseAuto>k__BackingField", true);

        Assert.NotNull(field);

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal(nameof(BaseClass.BaseAuto), property!.Name);
    }

    [Fact]
    public void GenericBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<>).GetField(
            "<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Value", property!.Name);
    }

    [Fact]
    public void ClosedGenericBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<int>).GetField(
            "<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Value", property!.Name);
    }

    [Fact]
    public void GenericStructBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericStruct<>).GetField(
            "<Value>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Value", property!.Name);
    }

    [Fact]
    public void GenericStaticBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<>).GetField(
            "<StaticValue>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("StaticValue", property!.Name);
    }

    [Fact]
    public void GenericType_NonGenericBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<>)
            .GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Id", property!.Name);
    }

    [Fact]
    public void ClosedGenericType_NonGenericBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<int>)
            .GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("Id", property!.Name);
    }

    [Fact]
    public void GenericType_StaticNonGenericBackingField_ShouldReturnProperty()
    {
        var field = typeof(GenericClass<>)
            .GetField("<StaticId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = field.TryGetAutoProperty(out var property);

        Assert.True(result);
        Assert.Equal("StaticId", property!.Name);
    }
}