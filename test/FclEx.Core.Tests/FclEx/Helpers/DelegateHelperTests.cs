namespace FclEx.Helpers;

public class DelegateHelperTests
{
    [Fact]
    public void MakeNewCustomDelegate_CreatesInvocableDelegateType()
    {
        var delegateType = DelegateHelper.MakeNewCustomDelegate(typeof(int), [typeof(int)]);
        var method = typeof(DelegateHelperTests).GetMethod(nameof(Increment), BindingFlags.Static | BindingFlags.NonPublic)!;

        var @delegate = Delegate.CreateDelegate(delegateType, method);

        Assert.True(typeof(MulticastDelegate).IsAssignableFrom(delegateType));
        Assert.Equal(2, @delegate.DynamicInvoke(1));
    }

    private static int Increment(int value) => value + 1;
}
