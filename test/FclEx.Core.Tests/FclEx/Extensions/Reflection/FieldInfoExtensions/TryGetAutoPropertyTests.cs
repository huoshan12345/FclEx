// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ConvertToAutoProperty
namespace FclEx.Extensions.Reflection.FieldInfoExtensions;

public class TryGetAutoPropertyTests
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
}