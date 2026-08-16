namespace FclEx.Helpers.ExpressionHelperTests;

public class GetMemberTests
{
    [Fact]
    public void GetMember_Field_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<TestModel>(m => m.Field);

        Assert.Equal(typeof(TestModel), memberInfo.DeclaringType);
        Assert.Equal(nameof(TestModel.Field), memberInfo.Name);

        var info = memberInfo as FieldInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_Property_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<TestModel>(m => m.Property);

        Assert.Equal(typeof(TestModel), memberInfo.DeclaringType);
        Assert.Equal(nameof(TestModel.Property), memberInfo.Name);

        var info = memberInfo as PropertyInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_VoidMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<TestModel>(m => m.VoidMethod());

        Assert.Equal(typeof(TestModel), memberInfo.DeclaringType);
        Assert.Equal(nameof(TestModel.VoidMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_IntMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<TestModel>(m => m.IntMethod());

        Assert.Equal(typeof(TestModel), memberInfo.DeclaringType);
        Assert.Equal(nameof(TestModel.IntMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_StaticIntMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<TestModel>(m => TestModel.StaticIntMethod());

        Assert.Equal(typeof(TestModel), memberInfo.DeclaringType);
        Assert.Equal(nameof(TestModel.StaticIntMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_Field_NonMember_Test()
    {
        var obj = new { Age = 1 };
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<TestModel>(m => obj.Age));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_Field_NonMember_Value_Test()
    {
        // ReSharper disable once ConvertToConstant.Local
        var value = 1; // do not use const.
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<TestModel>(m => value));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_Field_NonMember_ConstValue_Test()
    {
        const int value = 1;
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<TestModel>(m => value));
        Assert.Contains("does not refer to a member", ex.Message);
    }

    [Fact]
    public void GetMember_Property_NonMember_Test()
    {
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<TestModel>(m => "test".Length));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_NonMemberMethod_Test()
    {
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<TestModel>(m => string.IsNullOrEmpty("")));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_InterfaceMemberImplementedByType_ShouldReturnMember()
    {
        var member = ExpressionHelper.GetMember<InterfaceModel>(m => ((IHasProperty)m).Property);

        Assert.Equal(typeof(IHasProperty), member.DeclaringType);
        Assert.Equal(nameof(IHasProperty.Property), member.Name);
    }

    public class TestModel
    {
        public int Property { get; set; }
        public int Field;
        public void VoidMethod() { }
        public int IntMethod() => 1;
        public static int StaticIntMethod() => 1;
    }

    private interface IHasProperty
    {
        int Property { get; }
    }

    private sealed class InterfaceModel : IHasProperty
    {
        public int Property { get; set; }
    }
}
