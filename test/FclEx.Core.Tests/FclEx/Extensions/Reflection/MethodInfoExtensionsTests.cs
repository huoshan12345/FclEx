namespace FclEx.Extensions.Reflection;

public class MethodInfoExtensionsTests
{
    private static void MethodWithoutParameters() { }

    private static void MethodWithParameter(int value) { }

    [Fact]
    public void GetRuntimeIdentityTag_IsStableForAMethodInfoAndDistinctForDifferentMethods()
    {
        var withoutParameters = typeof(MethodInfoExtensionsTests).GetRequiredMethod(nameof(MethodWithoutParameters));
        var withParameter = typeof(MethodInfoExtensionsTests).GetRequiredMethod(nameof(MethodWithParameter), 0, [typeof(int)]);

        Assert.Equal(withoutParameters.GetRuntimeIdentityTag(), withoutParameters.GetRuntimeIdentityTag());
        Assert.NotEqual(withoutParameters.GetRuntimeIdentityTag(), withParameter.GetRuntimeIdentityTag());
    }
}
