namespace FclEx.Utils;

public partial class OperationResultExtensionsTests
{
    [Fact]
    public async Task OperationResult_Task_Ok_Action_TimeSpan()
    {
        var elapsed = TimeSpan.FromHours(1);
        TimeSpan timeSpan = default;
        var result = await Operation.CreateSuccess(elapsed)
            .ToTask()
            .Ok((_, t) => timeSpan = t);

        Assert.True(result.Success);
        Assert.Equal(elapsed, timeSpan);
    }
}