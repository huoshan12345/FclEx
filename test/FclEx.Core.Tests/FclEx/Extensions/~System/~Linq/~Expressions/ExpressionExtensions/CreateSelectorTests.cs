namespace FclEx.Extensions.ExpressionExtensions;

public class CreateSelectorTests
{
    private class Sample
    {
        public int Id { get; set; }
        public string Name = "default";
        public double Value { get; set; }
    }

    // ---------- Tests for Expression.CreateSelector<T, TMember> ----------

    [Fact]
    public void GenericSelector_Should_Select_Property_Value()
    {
        var selector = Expression.CreateSelector<Sample, int>("Id");
        var func = selector.Compile();
        var instance = new Sample { Id = 123 };
        Assert.Equal(123, func(instance));
    }

    [Fact]
    public void GenericSelector_Should_Select_Field_Value()
    {
        var selector = Expression.CreateSelector<Sample, string>("Name");
        var func = selector.Compile();
        var instance = new Sample { Name = "Alice" };
        Assert.Equal("Alice", func(instance));
    }

    [Fact]
    public void GenericSelector_Should_Throw_For_Invalid_Member_Name()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Expression.CreateSelector<Sample, int>("Unknown");
        });
    }

    [Fact]
    public void GenericSelector_Should_Throw_For_Type_Mismatch()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Expression.CreateSelector<Sample, int>("Name");
        });
    }

    // ---------- Tests for Expression.CreateSelector<T> (object? version) ----------

    [Fact]
    public void ObjectSelector_Should_Select_Property_Value()
    {
        var selector = Expression.CreateSelector<Sample>("Id");
        var func = selector.Compile();
        var instance = new Sample { Id = 123 };
        Assert.Equal(123, func(instance));
    }

    [Fact]
    public void ObjectSelector_Should_Select_Field_Value()
    {
        var selector = Expression.CreateSelector<Sample>("Name");
        var func = selector.Compile();
        var instance = new Sample { Name = "Alice" };
        Assert.Equal("Alice", func(instance));
    }

    [Fact]
    public void ObjectSelector_Should_Box_Value_Type()
    {
        var selector = Expression.CreateSelector<Sample>("Value");
        var func = selector.Compile();
        var instance = new Sample { Value = 3.14 };
        Assert.IsType<double>(func(instance));
        Assert.Equal(3.14, (double)func(instance)!);
    }

    [Fact]
    public void ObjectSelector_Should_Throw_For_Invalid_Member_Name()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Expression.CreateSelector<Sample>("Unknown");
        });
    }
}
