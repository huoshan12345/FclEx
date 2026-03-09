namespace FclEx.Web;

public class TestUserClient(ILoggerFactory loggerFactory) : UserClient(loggerFactory: loggerFactory)
{
    protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
    {
        return Operation.Success();
    }
}