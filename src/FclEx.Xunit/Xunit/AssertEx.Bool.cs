namespace Xunit;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
partial class AssertEx
{
    public static void True([DoesNotReturnIf(false)] bool condition, Func<string>? userMessage = null)
    {
        if (!condition)
            Assert.True(condition, userMessage?.Invoke());
    }

    public static void False([DoesNotReturnIf(true)] bool condition, Func<string>? userMessage = null)
    {
        if (condition)
            Assert.False(condition, userMessage?.Invoke());
    }
}