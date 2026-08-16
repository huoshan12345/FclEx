namespace System.ComponentModel.DataAnnotations;

public class ElementRequiredAttributeTests
{
    public class TestEnumerable(IEnumerable enumerable) : IEnumerable
    {
        public IEnumerator GetEnumerator() => enumerable.GetEnumerator();
    }

    private static ValidationContext CreateValidationContext(string displayName)
    {
        return new ValidationContext(new object(), null, null)
        {
            DisplayName = displayName
        };
    }

    [Fact]
    public void MinLength_Negative_Should_Throw()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ElementRequiredAttribute { MinLength = -1 });

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void MinLength_Zero_Should_Be_Valid_For_Empty_Value()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 0 };

        Assert.Equal(0, attribute.MinLength);
        Assert.Null(attribute.GetValidationResult("", CreateValidationContext("TestField")));
        Assert.Null(attribute.GetValidationResult(Array.Empty<object>(), CreateValidationContext("TestField")));
    }

    [Fact]
    public void Validate_NullValue_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 3 };
        var result = attribute.GetValidationResult(null, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_StringWithLengthLessThanMinLength_ReturnsError()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 5 };
        var value = "abc";
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.NotNull(result);
        Assert.Equal("The field TestField must be a string or array type with a minimum length of '5'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_StringWithLengthGreaterThanMinLength_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 3 };
        var value = "hello";
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_NonEnumerableValue_ReturnsError()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2 };
        var value = 42;
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.NotNull(result);
        Assert.Equal("The field TestField of type System.Int32 must be a IEnumerable type.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EnumerableWithNullElement_AllowNullElementFalse_ReturnsError()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2, AllowNullElement = false };
        var value = new object?[] { "item1", null, "item3" };
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.NotNull(result);
        Assert.Equal("The field TestField has a null element at 1.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EnumerableWithNullElement_AllowNullElementTrue_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2, AllowNullElement = true };
        var value = new object?[] { "item1", null };
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_EnumerableWithFewerElementsThanMinLength_ReturnsError()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 4 };
        var value = new[] { "item1", "item2" };
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.NotNull(result);
        Assert.Equal("The field TestField must be a string or array type with a minimum length of '4'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_EnumerableWithEnoughElements_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2 };
        var value = new[] { "item1", "item2", "item3" };
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_EnumerableStopsCheckingAfterMinLength_WhenAllowNullElementTrue_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2, AllowNullElement = true };
        var value = new object?[] { "item1", null, null, "item4" };
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_StringMinLengthEdgeCase_ReturnsSuccess()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 5 };
        var value = "12345";
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_EnumerableWithoutCount_ReturnsSuccess(bool allowNullElement)
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2, AllowNullElement = allowNullElement };
        var value = new TestEnumerable(new[] { "item1", "item2", "item3" });
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.Null(result);
    }

    [Fact]
    public void Validate_EnumerableWithoutCount_WithNullElement_AllowNullElementFalse_ReturnsError()
    {
        var attribute = new ElementRequiredAttribute { MinLength = 2, AllowNullElement = false };
        var value = new TestEnumerable(new object?[] { "item1", null, "item3" });
        var result = attribute.GetValidationResult(value, CreateValidationContext("TestField"));
        Assert.NotNull(result);
        Assert.Equal("The field TestField has a null element at 1.", result.ErrorMessage);
    }
}
