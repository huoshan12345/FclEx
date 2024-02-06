namespace FclEx.Cooperation.NewRelic;

public class NewRelicHelperTests
{
    [Fact]
    public void GetCurrentTransaction_Test()
    {
        NewRelicHelper.GetCurrentTransaction()
            .AddCustomAttribute("xx", "xx")
            .AddCustomAttribute("yy", null!);
    }
}