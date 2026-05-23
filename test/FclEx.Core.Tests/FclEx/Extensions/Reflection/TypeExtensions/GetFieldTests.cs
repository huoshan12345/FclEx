// ReSharper disable ConvertToConstant.Local
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetFieldTests
{
    private class BaseClass
    {
        public readonly int BaseField;
        private readonly string? PrivateBaseField;
    }

    private class DerivedClass : BaseClass
    {
        public readonly double DerivedField;
        private readonly int PrivateDerivedField;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetField_ShouldFindFieldInCurrentType(bool searchBaseTypes)
    {
        var type = typeof(DerivedClass);
        var fieldName = nameof(DerivedClass.DerivedField);
        var field = type.GetField(fieldName, searchBaseTypes);

        Assert.NotNull(field);
        Assert.Equal(fieldName, field.Name);
        Assert.Equal(typeof(double), field.FieldType);
    }

    [Fact]
    public void GetField_ShouldFindFieldInBaseType_WhenSearchBaseTypesIsTrue()
    {
        var type = typeof(DerivedClass);
        var fieldName = nameof(DerivedClass.BaseField);
        var field = type.GetField(fieldName, searchBaseTypes: true);

        Assert.NotNull(field);
        Assert.Equal(fieldName, field.Name);
        Assert.Equal(typeof(int), field.FieldType);
    }

    [Fact]
    public void GetField_ShouldNotFindFieldInBaseType_WhenSearchBaseTypesIsFalse()
    {
        var type = typeof(DerivedClass);
        var fieldName = nameof(DerivedClass.BaseField);
        var field = type.GetField(fieldName, searchBaseTypes: false);

        Assert.Null(field);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetField_ShouldReturnNull_ForNonExistentField(bool searchBaseTypes)
    {
        var type = typeof(DerivedClass);
        var fieldName = "NonExistentField";
        var field = type.GetField(fieldName, searchBaseTypes);

        Assert.Null(field);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetField_ShouldHandleNullBaseTypeGracefully(bool searchBaseTypes)
    {
        var type = typeof(object);
        var fieldName = "NonExistentField";
        var field = type.GetField(fieldName, searchBaseTypes);

        Assert.Null(field);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetField_ShouldFindPrivateFieldInCurrentType(bool searchBaseTypes)
    {
        var type = typeof(DerivedClass);
        var fieldName = "PrivateDerivedField";
        var field = type.GetField(fieldName, searchBaseTypes);

        Assert.NotNull(field);
        Assert.Equal(fieldName, field.Name);
        Assert.Equal(typeof(int), field.FieldType);
    }

    [Fact]
    public void GetField_ShouldFindPrivateFieldInBaseType_WhenSearchBaseTypesIsFalse()
    {
        var type = typeof(DerivedClass);
        var fieldName = "PrivateBaseField";
        var field = type.GetField(fieldName, searchBaseTypes: false);

        Assert.Null(field);
    }

    [Fact]
    public void GetField_ShouldFindPrivateFieldInBaseType_WhenSearchBaseTypesIsTrue()
    {
        var type = typeof(DerivedClass);
        var fieldName = "PrivateBaseField";
        var field = type.GetField(fieldName, searchBaseTypes: true);

        Assert.NotNull(field);
        Assert.Equal(fieldName, field.Name);
        Assert.Equal(typeof(string), field.FieldType);
    }

    public struct TestStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public unsafe void SetNameByReflection(string name)
        {
            var field = typeof(TestStruct).GetAutoPropertyBackingField(nameof(Name));

            fixed (void* p = &this)
            {
                ref var r = ref Unsafe.AsRef<TestStruct>(p);
                var t = __makeref(r);
                field.SetValueDirect(t, name);
            }
        }
    }

    [Fact]
    public void SetField_ByReflection_Test()
    {
        var obj = new TestStruct();
        obj.SetNameByReflection("test");
        Assert.Equal("test", obj.Name);
    }
}