using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FclEx.AspNetCore.Extensions;

public class ModelStateDictionaryExtensionsTests
{
    static ModelStateDictionary CreateModelState(params (string key, string? error)[] errors)
    {
        var modelState = new ModelStateDictionary();

        foreach (var (key, error) in errors)
        {
            modelState.AddModelError(key, error ?? string.Empty);
        }

        return modelState;
    }

    [Fact]
    public void GetErrors_ShouldReturnEmpty_WhenModelStateIsValid()
    {
        var modelState = CreateModelState();

        var result = modelState.GetErrors();

        Assert.Empty(result);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void GetErrors_ShouldReturnSingleError()
    {
        var modelState = CreateModelState(
            ("Name", "Name is required")
        );

        var result = modelState.GetErrors();

        Assert.Single(result);
        Assert.True(result.ContainsKey("Name"));

        var values = result["Name"].AsIList();

        Assert.Single(values);
        Assert.Equal("Name is required", values[0]);
    }

    [Fact]
    public void GetErrors_ShouldReturnMultipleErrors_ForSameKey()
    {
        var modelState = CreateModelState(
            ("Name", "Name is required"),
            ("Name", "Name must be longer")
        );

        var result = modelState.GetErrors();

        Assert.Single(result);

        var values = result["Name"].AsIList();

        Assert.Equal(2, values.Count);
        Assert.Equal("Name is required", values[0]);
        Assert.Equal("Name must be longer", values[1]);
    }

    [Fact]
    public void GetErrors_ShouldReturnErrors_ForMultipleKeys()
    {
        var modelState = CreateModelState(
            ("Name", "Name is required"),
            ("Age", "Age must be >= 18")
        );

        var result = modelState.GetErrors();

        Assert.Equal(2, result.Count);

        Assert.True(result.ContainsKey("Name"));
        Assert.True(result.ContainsKey("Age"));

        Assert.Equal("Name is required", result["Name"].AsIList()[0]);
        Assert.Equal("Age must be >= 18", result["Age"].AsIList()[0]);
    }

    [Fact]
    public void GetErrors_ShouldSupportModelLevelError()
    {
        var modelState = CreateModelState(
            (string.Empty, "Model error")
        );

        var result = modelState.GetErrors();

        Assert.Single(result);
        Assert.True(result.ContainsKey(string.Empty));

        var values = result[string.Empty].AsIList();

        Assert.Single(values);
        Assert.Equal("Model error", values[0]);
    }

    [Fact]
    public void GetErrors_ShouldPreserveErrorOrder()
    {
        var modelState = CreateModelState(
            ("Name", "Error1"),
            ("Name", "Error2"),
            ("Name", "Error3")
        );

        var result = modelState.GetErrors();

        var values = result["Name"].AsIList();

        Assert.Equal(3, values.Count);
        Assert.Equal("Error1", values[0]);
        Assert.Equal("Error2", values[1]);
        Assert.Equal("Error3", values[2]);
    }

    [Fact]
    public void GetErrors_ShouldEnumerateCorrectly()
    {
        var modelState = CreateModelState(
            ("A", "A1"),
            ("B", "B1"),
            ("B", "B2")
        );

        var result = modelState.GetErrors();

        var count = 0;

        foreach (var (key, values) in result)
        {
            Assert.NotNull(key);
            Assert.NotEmpty(values);
            count++;
        }

        Assert.Equal(2, count);
    }

    [Fact]
    public void GetErrors_ShouldHandleEmptyErrorMessage()
    {
        var modelState = CreateModelState(
            ("Name", "")
        );

        var result = modelState.GetErrors();

        Assert.Single(result);
        Assert.Single(result["Name"]);
        Assert.Equal("", result["Name"].AsIList()[0]);
    }

    [Fact]
    public void GetErrors_ShouldHandleNullErrorMessage()
    {
        var modelState = CreateModelState(
            ("Name", null)
        );

        var result = modelState.GetErrors();

        Assert.Single(result);
        Assert.Single(result["Name"]);
        Assert.Equal("", result["Name"].AsIList()[0]);
    }

    [Fact]
    public void GetErrors_ShouldHandleMixedModelAndFieldErrors()
    {
        var modelState = CreateModelState(
            (string.Empty, "Model error"),
            ("Name", "Name required"),
            ("Age", "Invalid age")
        );

        var result = modelState.GetErrors();

        Assert.Equal(3, result.Count);

        Assert.Equal("Model error", result[string.Empty].AsIList()[0]);
        Assert.Equal("Name required", result["Name"].AsIList()[0]);
        Assert.Equal("Invalid age", result["Age"].AsIList()[0]);
    }

    [Fact]
    public void GetErrors_ShouldHandleComplexCombination()
    {
        var modelState = CreateModelState(
            ("Name", "Required"),
            ("Name", "Too short"),
            ("Age", "Invalid"),
            ("Age", "Too young"),
            (string.Empty, "Model error")
        );

        var result = modelState.GetErrors();

        Assert.Equal(3, result.Count);

        Assert.Equal(2, result["Name"].Count);
        Assert.Equal(2, result["Age"].Count);
        Assert.Single(result[string.Empty]);

        Assert.Equal("Required", result["Name"].AsIList()[0]);
        Assert.Equal("Too short", result["Name"].AsIList()[1]);

        Assert.Equal("Invalid", result["Age"].AsIList()[0]);
        Assert.Equal("Too young", result["Age"].AsIList()[1]);

        Assert.Equal("Model error", result[string.Empty].AsIList()[0]);
    }
}