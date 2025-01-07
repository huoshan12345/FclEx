namespace FclEx.AspNetCore;

public static class AuthorizationHandlerContextExtensions
{
    public static T[] GetAttributes<T>(this AuthorizationHandlerContext authContext, bool combineController, bool inherit = true) where T : Attribute
    {
        if (authContext.Resource is not HttpContext httpContext)
            return [];

        if (httpContext.GetEndpoint() is not { } endpoint)
            return [];

        return endpoint.GetAttributes<T>(combineController, inherit);
    }

    public static Task SucceedAsync(this AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
