// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoProperty
namespace FclEx.Extensions.Reflection.PropertyInfoExtensions;

public class TryGetAutoBackingFieldTests
{
    [Fact]
    public void AutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.AutoProperty))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<AutoProperty>k__BackingField", field!.Name);
    }

    [Fact]
    public void ReadOnlyAuto_ShouldReturnBackingField()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyAuto))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<ReadOnlyAuto>k__BackingField", field!.Name);
    }

    [Fact]
    public void InitOnlyProperty_ShouldReturnBackingField()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.InitOnly))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<InitOnly>k__BackingField", field!.Name);
    }

    [Fact]
    public void StaticAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(TestClass).GetProperty(
            nameof(TestClass.StaticAuto),
            BindingFlags.Public | BindingFlags.Static)!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<StaticAuto>k__BackingField", field!.Name);
    }

    [Fact]
    public void StructAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(TestStruct).GetProperty("Value")!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Value>k__BackingField", field!.Name);
    }

    [Fact]
    public void InheritedAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(BaseClass).GetProperty(nameof(BaseClass.BaseAuto))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<BaseAuto>k__BackingField", field!.Name);
    }

    [Fact]
    public void NormalProperty_ShouldReturnFalse()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.NormalProperty))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.False(result);
        Assert.Null(field);
    }

    [Fact]
    public void InheritedAutoProperty_FromDerived_ShouldReturnBackingField()
    {
        var property = typeof(DerivedClass).GetRequiredProperty(nameof(BaseClass.BaseAuto), true);

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<BaseAuto>k__BackingField", field!.Name);
    }

    [Fact]
    public void GenericAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<>).GetProperty(nameof(GenericClass<int>.Value))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Value>k__BackingField", field!.Name);
    }

    [Fact]
    public void ClosedGenericAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<int>).GetProperty(nameof(GenericClass<int>.Value))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Value>k__BackingField", field!.Name);
    }

    [Fact]
    public void GenericStructAutoProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericStruct<>).GetProperty(nameof(GenericStruct<int>.Value))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Value>k__BackingField", field!.Name);
    }

    [Fact]
    public void GenericStaticProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<>).GetProperty(nameof(GenericClass<int>.StaticValue))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<StaticValue>k__BackingField", field!.Name);
    }

    [Fact]
    public void GenericType_NonGenericProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<>)
            .GetProperty(nameof(GenericClass<int>.Id))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Id>k__BackingField", field!.Name);
    }

    [Fact]
    public void ClosedGenericType_NonGenericProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<int>)
            .GetProperty(nameof(GenericClass<int>.Id))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<Id>k__BackingField", field!.Name);
    }

    [Fact]
    public void GenericType_StaticNonGenericProperty_ShouldReturnBackingField()
    {
        var property = typeof(GenericClass<>)
            .GetProperty(nameof(GenericClass<int>.StaticId))!;

        var result = property.TryGetAutoBackingField(out var field);

        Assert.True(result);
        Assert.Equal("<StaticId>k__BackingField", field!.Name);
    }
}