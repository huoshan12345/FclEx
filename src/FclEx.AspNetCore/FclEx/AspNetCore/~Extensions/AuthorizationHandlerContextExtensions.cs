namespace FclEx.AspNetCore;

public static class AuthorizationHandlerContextExtensions
{
    /// <summary>
    /// Retrieves attributes of the specified type <typeparamref name="T"/> from the endpoint associated with the 
    /// <see cref="AuthorizationHandlerContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
    /// <param name="authContext">The <see cref="AuthorizationHandlerContext"/> containing the resource to inspect.</param>
    /// <param name="combineController">
    /// A boolean value indicating whether to combine attributes from both the controller and the action method.
    /// If <c>false</c>, only action-level attributes are returned unless none exist, in which case controller-level attributes are returned.
    /// If <see langword="true"/>, attributes from both the action and controller are returned.
    /// </param>
    /// <param name="inherit">
    /// A boolean value indicating whether to search the inheritance chain of the action and controller for attributes.
    /// Defaults to <see langword="true"/>.
    /// </param>
    /// <returns>
    /// An array of attributes of type <typeparamref name="T"/> found on the action, controller, or both, depending on the value of 
    /// <paramref name="combineController"/>. Returns an empty array if the resource is not an <see cref="HttpContext"/>,
    /// the <see cref="HttpContext"/> does not have an associated endpoint, or no matching attributes are found.
    /// </returns>
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
