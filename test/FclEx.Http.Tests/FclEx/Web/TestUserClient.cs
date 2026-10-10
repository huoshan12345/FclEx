namespace FclEx.Web;

public class TestUserClient(ILoggerFactory loggerFactory, Func<OperationResult>? loginAction = null)
    : UserClient(loggerFactory: loggerFactory)
{
    protected override Task<OperationResult> LoginActionAsync(CancellationToken cancellationToken)
    {
        return loginAction?.Invoke() ?? Operation.Success();
    }
}