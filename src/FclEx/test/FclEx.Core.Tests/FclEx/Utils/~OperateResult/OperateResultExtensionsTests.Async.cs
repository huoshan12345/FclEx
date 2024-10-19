namespace FclEx.Utils;

public partial class OperateResultExtensionsTests
{
    [Fact]
    public async Task OperateResult_Task_Ok_Action_TimeSpan()
    {
        var elapsed = TimeSpan.FromHours(1);
        TimeSpan timeSpan = default;
        var result = await Operate.CreateSuccess(elapsed)
            .ToTask()
            .Ok((_, t) => timeSpan = t);

        Assert.True(result.Success);
        Assert.Equal(elapsed, timeSpan);
    }
}