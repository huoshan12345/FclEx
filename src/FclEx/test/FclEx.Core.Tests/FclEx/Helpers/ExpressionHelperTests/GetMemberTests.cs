namespace FclEx.Helpers.ExpressionHelperTests;

public class GetMemberTests
{
    [Fact]
    public void GetMember_Field_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<Tester>(m => m.Field);

        Assert.Equal(typeof(Tester), memberInfo.DeclaringType);
        Assert.Equal(nameof(Tester.Field), memberInfo.Name);

        var info = memberInfo as FieldInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_Property_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<Tester>(m => m.Property);

        Assert.Equal(typeof(Tester), memberInfo.DeclaringType);
        Assert.Equal(nameof(Tester.Property), memberInfo.Name);

        var info = memberInfo as PropertyInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_VoidMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<Tester>(m => m.VoidMethod());

        Assert.Equal(typeof(Tester), memberInfo.DeclaringType);
        Assert.Equal(nameof(Tester.VoidMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_IntMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<Tester>(m => m.IntMethod());

        Assert.Equal(typeof(Tester), memberInfo.DeclaringType);
        Assert.Equal(nameof(Tester.IntMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_StaticIntMethod_Test()
    {
        var memberInfo = ExpressionHelper.GetMember<Tester>(m => Tester.StaticIntMethod());

        Assert.Equal(typeof(Tester), memberInfo.DeclaringType);
        Assert.Equal(nameof(Tester.StaticIntMethod), memberInfo.Name);

        var info = memberInfo as MethodInfo;
        Assert.NotNull(info);
    }

    [Fact]
    public void GetMember_Field_NonMember_Test()
    {
        var obj = new { Age = 1 };
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<Tester>(m => obj.Age));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_Property_NonMember_Test()
    {
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<Tester>(m => "test".Length));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    [Fact]
    public void GetMember_NonMemberMethod_Test()
    {
        var ex = Assert.Throws<ArgumentException>(() => ExpressionHelper.GetMember<Tester>(m => string.IsNullOrEmpty("")));
        Assert.Contains("refers to a member that is not from type", ex.Message);
    }

    public class Tester
    {
        public int Property { get; set; }
        public int Field;
        public void VoidMethod() { }
        public int IntMethod() => 1;
        public static int StaticIntMethod() => 1;
    }
}