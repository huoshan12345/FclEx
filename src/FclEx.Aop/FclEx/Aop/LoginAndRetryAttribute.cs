namespace FclEx.Aop;

[AttributeUsage(AttributeTargets.Method)]
public class LoginAndRetryAttribute : AbstractInterceptorAttribute
{
    public virtual Func<object?, bool> NeedRetry { get; } = o => o is IOperationResult { IsError: true };

    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        await context.Invoke(next);

        if (context.Implementation is UserClient client)
        {
            var result = context.IsAsync()
                ? (object?)await (dynamic)context.ReturnValue
                : context.ReturnValue;

            if (client.IsOnline) return;

            if (NeedRetry(result))
            {
                await client.FakeLoginAsync(true);
                await context.Invoke(next);
            }
        }
    }
}