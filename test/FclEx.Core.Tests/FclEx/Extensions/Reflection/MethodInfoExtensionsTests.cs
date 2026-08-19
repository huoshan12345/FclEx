namespace FclEx.Extensions.Reflection;

public class MethodInfoExtensionsTests
{
    private static T GenericMethod<T>(T value) => value;

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
