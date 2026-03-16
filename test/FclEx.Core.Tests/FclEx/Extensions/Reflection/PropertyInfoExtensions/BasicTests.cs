namespace FclEx.Extensions.Reflection.PropertyInfoExtensions;

public class BasicTests
{
    private class TestModel
    {
        public string Name { get; set; } = "Hello";
        public int Number { get; set; } = 42;
        public string? NullableString { get; set; }
        public static string StaticProperty { get; set; } = "StaticValue";

        public string GetterOnly { get; } = "ReadOnly";

        private string? _setterOnly;
        public string SetterOnly
        {
            set => _setterOnly = value;
        }
    }

    [Fact]
    public void GetRequiredGetMethod_ShouldReturnGetterMethod()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Name))!;
        var getter = prop.GetRequiredGetMethod();

        Assert.NotNull(getter);
        Assert.True(getter.IsSpecialName);
    }

    [Fact]
    public void GetRequiredGetMethod_ShouldThrow_WhenNoGetter()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.SetterOnly))!;
        Assert.Throws<MissingMethodException>(() => prop.GetRequiredGetMethod());
    }

    [Fact]
    public void GetRequiredSetMethod_ShouldReturnSetterMethod()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Name))!;
        var setter = prop.GetRequiredSetMethod();

        Assert.NotNull(setter);
        Assert.True(setter.IsSpecialName);
    }

    [Fact]
    public void GetRequiredSetMethod_ShouldThrow_WhenNoSetter()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.GetterOnly))!;
        Assert.Throws<MissingMethodException>(() => prop.GetRequiredSetMethod());
    }

    [Fact]
    public void GetValue_Generic_ShouldReturnValue()
    {
        var obj = new TestModel { Number = 100 };
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Number))!;

        int result = prop.GetValue<int>(obj);

        Assert.Equal(100, result);
    }

    [Fact]
    public void GetRequiredValue_ShouldReturnNonNullValue()
    {
        var obj = new TestModel { Name = "Test" };
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Name))!;

        var value = prop.GetRequiredValue(obj);

        Assert.Equal("Test", value);
    }

    [Fact]
    public void GetRequiredValue_ShouldThrow_WhenValueIsNull()
    {
        var obj = new TestModel { NullableString = null };
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.NullableString))!;

        Assert.Throws<InvalidOperationException>(() => prop.GetRequiredValue(obj));
    }

    [Fact]
    public void GetRequiredValue_Generic_ShouldReturnNonNullValue()
    {
        var obj = new TestModel { Number = 77 };
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Number))!;

        int result = prop.GetRequiredValue<int>(obj);

        Assert.Equal(77, result);
    }

    [Fact]
    public void IsStatic_ShouldReturnTrue_ForStaticProperty()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.StaticProperty))!;
        Assert.True(prop.IsStatic());
    }

    [Fact]
    public void IsStatic_ShouldReturnFalse_ForInstanceProperty()
    {
        var prop = typeof(TestModel).GetProperty(nameof(TestModel.Name))!;
        Assert.False(prop.IsStatic());
    }
}