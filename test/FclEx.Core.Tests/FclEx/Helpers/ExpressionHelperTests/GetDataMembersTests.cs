using static FclEx.Helpers.ExpressionHelper;

namespace FclEx.Helpers.ExpressionHelperTests;

public class GetDataMembersTests
{
    public class TestModel
    {
        public int Id { get; set; }
        public long Length { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public bool IsGood { get; set; }
        public ChildModel Child { get; set; } = new();

        public int Field;

        public int GetSomething() => 42;
    }

    public class ChildModel
    {
        public int Id { get; set; }
    }

    public interface IIdentifiable
    {
        int Id { get; }
    }

    public class IdentifiableModel : IIdentifiable
    {
        public int Id { get; set; }
    }

    [Fact]
    public void Should_Get_Single_Property()
    {
        var result = GetDataMembers<TestModel>(m => m.Id).ToArray();

        Assert.Single(result);
        Assert.Equal(nameof(TestModel.Id), result[0].Name);
        Assert.IsType<PropertyInfo>(result[0], false);
    }

    [Fact]
    public void Should_Get_Multiple_Properties()
    {
        var result = GetDataMembers<TestModel>(m => new
        {
            m.Id,
            m.Length,
            m.LastWriteTimeUtc,
            m.IsGood
        }).ToArray();

        Assert.Equal(4, result.Length);

        Assert.Contains(result, m => m.Name == nameof(TestModel.Id));
        Assert.Contains(result, m => m.Name == nameof(TestModel.Length));
        Assert.Contains(result, m => m.Name == nameof(TestModel.LastWriteTimeUtc));
        Assert.Contains(result, m => m.Name == nameof(TestModel.IsGood));
    }

    [Fact]
    public void Should_Get_Field()
    {
        var result = GetDataMembers<TestModel>(m => m.Field).ToArray();

        Assert.Single(result);
        Assert.Equal(nameof(TestModel.Field), result[0].Name);
        Assert.IsType<FieldInfo>(result[0], false);
    }

    [Fact]
    public void Should_Handle_Boxing()
    {
        var result = GetDataMembers<TestModel>(m => (object)m.Id).ToArray();

        Assert.Single(result);
        Assert.Equal(nameof(TestModel.Id), result[0].Name);
    }

    [Fact]
    public void Should_Throw_When_Using_MethodCall()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => m.GetSomething()).ToArray());

        Assert.Contains("Selector must", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Using_Constant()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => 123).ToArray());

        Assert.Contains("Selector must", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Using_Computed_Expression()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => new
            {
                m.Id,
                X = m.Length + 1
            }).ToArray());

        Assert.Contains("Only simple member access", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Using_Method_In_New()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => new
            {
                m.Id,
                Something = m.GetSomething()
            }).ToArray());

        Assert.Contains("Only simple member access", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Using_Nested_Member()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => m.Child.Id).ToArray());

        Assert.Contains("Only direct member access", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Anonymous_Object_Contains_Nested_Member()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => new { m.Id, ChildId = m.Child.Id }).ToArray());

        Assert.Contains("Only direct member access", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Using_Captured_Member()
    {
        var other = new TestModel();

        var ex = Assert.Throws<ArgumentException>(() =>
            GetDataMembers<TestModel>(m => other.Id).ToArray());

        Assert.Contains("Only direct member access", ex.Message);
    }

    [Fact]
    public void Should_Get_Direct_Interface_Member_Through_Conversion()
    {
        var result = GetDataMembers<IdentifiableModel>(m => ((IIdentifiable)m).Id).ToArray();

        Assert.Single(result);
        Assert.Equal(nameof(IIdentifiable.Id), result[0].Name);
        Assert.Equal(typeof(IIdentifiable), result[0].DeclaringType);
    }
}
