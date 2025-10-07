using FclEx.TestModels;

namespace FclEx.Helpers.ExpressionHelperTests;

public class ToObjectSelectorTests
{
    [Fact]
    public void ToObjectSelector_ShouldConvertValueTypeSelector()
    {
        Expression<Func<Person, int>> selector = p => p.Age;
        var converted = ExpressionHelper.ToObjectSelector(selector);

        var func = converted.Compile();
        var person = new Person { Age = 25 };

        Assert.Equal(25, func(person));
        Assert.IsType<int>(func(person));
    }

    [Fact]
    public void ToObjectSelector_ShouldKeepReferenceTypeSelector()
    {
        Expression<Func<Person, string>> selector = p => p.Name;
        var converted = ExpressionHelper.ToObjectSelector(selector);

        var func = converted.Compile();
        var person = new Person { Name = "Bob" };

        Assert.Equal("Bob", func(person));
    }

    [Fact]
    public void ToObjectSelector_ShouldHandleObjectMemberWithoutExtraConvert()
    {
        Expression<Func<Person, object>> selector = p => p.Name;
        var converted = ExpressionHelper.ToObjectSelector(selector);

        // Expression should not wrap in Convert if already object
        Assert.Equal(selector.Body.ToString(), converted.Body.ToString());
    }

    [Fact]
    public void ToObjectSelector_ShouldReturnNullForNullableValue()
    {
        Expression<Func<Person, int?>> selector = p => null;
        var converted = ExpressionHelper.ToObjectSelector(selector);

        var func = converted.Compile();
        var person = new Person();
        Assert.Null(func(person));
    }
}