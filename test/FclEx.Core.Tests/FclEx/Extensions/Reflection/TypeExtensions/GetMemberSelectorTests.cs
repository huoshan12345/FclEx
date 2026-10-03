namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetMemberSelectorTests
{
    private class BaseModel
    {
        public void Method() { }
    }

    private class MiddleModel : BaseModel
    {
        public int Property { get; set; }
    }

    private class Model : MiddleModel;

    private interface IBaseInterface;

    private interface IDerivedInterface : IBaseInterface;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetMember_ShouldStopAfterFirstMatch(bool searchBaseTypes)
    {
        var expected = typeof(BaseModel).GetMethod(nameof(BaseModel.Method))!;
        var inspectedTypes = new List<Type>();

        var member = typeof(Model).GetMember(t =>
        {
            inspectedTypes.Add(t);
            return expected;
        }, searchBaseTypes);

        Assert.Same(expected, member);
        Assert.Equal(new[] { typeof(Model) }, inspectedTypes);
    }

    [Fact]
    public void GetMember_ShouldInspectOnlySpecifiedType_WhenBaseTypeSearchIsDisabled()
    {
        var inspectedTypes = new List<Type>();

        var member = typeof(Model).GetMember<MethodInfo>(t =>
        {
            inspectedTypes.Add(t);
            return null;
        }, searchBaseTypes: false);

        Assert.Null(member);
        Assert.Equal(new[] { typeof(Model) }, inspectedTypes);
    }

    [Fact]
    public void GetMember_ShouldSearchBaseTypesInOrder_AndStopAtFirstMatch()
    {
        var inspectedTypes = new List<Type>();

        var member = typeof(Model).GetMember(t =>
        {
            inspectedTypes.Add(t);
            return t.GetProperty(nameof(MiddleModel.Property), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }, searchBaseTypes: true);

        Assert.NotNull(member);
        Assert.Equal(typeof(MiddleModel), member.DeclaringType);
        Assert.Equal(new[] { typeof(Model), typeof(MiddleModel) }, inspectedTypes);
    }

    [Fact]
    public void GetMember_ShouldReturnNull_AfterInspectingEntireBaseTypeHierarchy()
    {
        var inspectedTypes = new List<Type>();

        var member = typeof(Model).GetMember<MethodInfo>(t =>
        {
            inspectedTypes.Add(t);
            return null;
        }, searchBaseTypes: true);

        Assert.Null(member);
        Assert.Equal(new[] { typeof(Model), typeof(MiddleModel), typeof(BaseModel), typeof(object) }, inspectedTypes);
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(IDerivedInterface))]
    public void GetMember_ShouldInspectTypeOnce_WhenItHasNoBaseType(Type type)
    {
        var inspectedTypes = new List<Type>();

        var member = type.GetMember<MemberInfo>(t =>
        {
            inspectedTypes.Add(t);
            return null;
        }, searchBaseTypes: true);

        Assert.Null(member);
        Assert.Equal(new[] { type }, inspectedTypes);
    }

    [Fact]
    public void GetMember_ShouldPropagateSelectorException_WithoutContinuingSearch()
    {
        var expected = new InvalidOperationException("Selector failed.");
        var inspectedTypes = new List<Type>();

        var exception = Assert.Throws<InvalidOperationException>(() => typeof(Model).GetMember<MemberInfo>(t =>
        {
            inspectedTypes.Add(t);
            throw expected;
        }, searchBaseTypes: true));

        Assert.Same(expected, exception);
        Assert.Equal(new[] { typeof(Model) }, inspectedTypes);
    }
}
