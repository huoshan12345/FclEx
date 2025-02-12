namespace FclEx.Web;

public class TestUserClient : UserClient
{
    public TestUserClient(ILoggerFactory loggerFactory) : base(loggerFactory: loggerFactory)
    {
    }

    protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
    {
        return Operation.Success().ToTask();
    }
}