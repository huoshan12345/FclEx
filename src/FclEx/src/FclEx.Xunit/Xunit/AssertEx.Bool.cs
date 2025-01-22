namespace Xunit;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
partial class AssertEx
{
    public static void True(bool condition, Func<string>? userMessage = null)
    {
        if (!condition)
            Assert.True(condition, userMessage?.Invoke());
    }

    public static void False(bool condition, Func<string>? userMessage = null)
    {
        if (condition)
            Assert.False(condition, userMessage?.Invoke());
    }
}