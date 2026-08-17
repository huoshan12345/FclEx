namespace FclEx.Extensions.Reflection;

public class MethodInfoExtensionsTests
{
    private static void MethodWithoutParameters() { }

    private static void MethodWithParameter(int value) { }

    private static T GenericMethod<T>(T value) => value;

    [Fact]
    public void GetRuntimeIdentityTag_IsStableForAMethodInfoAndDistinctForDifferentMethods()
    {
        var withoutParameters = typeof(MethodInfoExtensionsTests).GetRequiredMethod(nameof(MethodWithoutParameters));
        var withParameter = typeof(MethodInfoExtensionsTests).GetRequiredMethod(nameof(MethodWithParameter), 0, [typeof(int)]);

        Assert.Equal(withoutParameters.GetRuntimeIdentityTag(), withoutParameters.GetRuntimeIdentityTag());
        Assert.NotEqual(withoutParameters.GetRuntimeIdentityTag(), withParameter.GetRuntimeIdentityTag());
    }

    [Fact]
    public void GetSignature_IncludesGenericArgumentsForDisplay()
    {
        var method = typeof(MethodInfoExtensionsTests).GetMethod(
            nameof(GenericMethod),
            BindingFlags.Static | BindingFlags.NonPublic)!;

        var signature = method.GetSignature();

        Assert.Contains("<", signature);
        Assert.Contains(">", signature);
        Assert.Contains("T", signature);
    }
}
