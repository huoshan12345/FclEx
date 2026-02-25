namespace FclEx.Utils.OperationResult;

public partial class OperationResultExtensionsTests
{
    [Fact]
    public async Task OperationResult_Task_Ok_Action_TimeSpan()
    {
        var elapsed = TimeSpan.FromHours(1);
        TimeSpan timeSpan = default;
        var result = await Operation.Success(elapsed)
            .ToTask()
            .OnValue((_, t) => timeSpan = t);

        Assert.True(result.IsSuccess);
        Assert.Equal(elapsed, timeSpan);
    }
}