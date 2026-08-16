namespace FclEx.Utils;

public class OptionalTests
{
    [Fact]
    public void Some_Null_Should_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Optional.Some<string?>(null));
    }

    [Fact]
    public void Some_NonNull_Should_Create_Value()
    {
        var optional = Optional.Some("value");

        Assert.True(optional.HasValue);
        Assert.Equal("value", optional.Value);
    }

    [Fact]
    public void HasValue_ShouldReturnTrue_WhenValueIsNotNull()
    {
        var optional = new Optional<int>(5);
        var hasValue = optional.HasValue;
        Assert.True(hasValue);
    }

    [Fact]
    public void HasValue_ShouldReturnFalse_WhenValueIsNull()
    {
        var optional = new Optional<int?>(null);
        var hasValue = optional.HasValue;
        Assert.False(hasValue);
    }

    [Fact]
    public void ImplicitConversion_FromNullableValue_ShouldCreateOptionalWithValue()
    {
        int? nullableValue = 10;
        Optional<int?> optional = nullableValue;
        Assert.True(optional.HasValue);
        Assert.Equal(10, optional.Value);
    }

    [Fact]
    public void ImplicitConversion_FromNullNullableValue_ShouldCreateOptionalWithoutValue()
    {
        int? nullableValue = null;
        Optional<int?> optional = nullableValue;
        Assert.False(optional.HasValue);
        Assert.Null(optional.Value);
    }

    [Fact]
    public void ImplicitConversion_ToNullableValue_ShouldReturnValue()
    {
        var optional = new Optional<int>(5);
        int? result = optional;
        Assert.Equal(5, result);
    }

    [Fact]
    public void ImplicitConversion_ToNullNullableValue_ShouldReturnNull()
    {
        var optional = new Optional<int?>(null);
        int? result = optional;
        Assert.Null(result);
    }
}
