// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoProperty
namespace FclEx.Extensions.Reflection.PropertyInfoExtensions;

public class TryGetAutoBackingFieldTests
{
    private class TestClass
    {
        public int AutoProperty { get; set; }

        public int ReadOnlyAuto { get; }

        public int InitOnly { get; init; }

        public static int StaticAuto { get; set; }

        public int NormalProperty
        {
            get => _field;
            set => _field = value;
        }

        private int _field;
    }

    private struct TestStruct
    {
        public int Value { get; set; }
    }

    private class BaseClass
    {
        public int BaseAuto { get; set; }
    }

    private class DerivedClass : BaseClass;

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
}