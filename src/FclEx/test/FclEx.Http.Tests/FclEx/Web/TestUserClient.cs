namespace FclEx.Web;

public class TestUserClient : UserClient
{
    public TestUserClient(ILoggerFactory loggerFactory) : base(loggerFactory: loggerFactory)
    {
    }

    protected override Task<OperateResult> LoginActionAsync(CancellationToken token)
    {
        return Operate.Success.ToTask();
    }
}