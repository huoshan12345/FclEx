namespace Microsoft.AspNetCore.Authorization;

public class ScopeAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
    {
        var attributes = context.GetAttributes<RequiredScopeAttribute>(false);
        if (attributes.IsEmpty())
        {
            return context.User.Identity?.IsAuthenticated == true
                ? context.SucceedAsync(requirement)
                : Task.CompletedTask;
        }

        var scopes = context.User.Claims
            .Where(m => m.Type == JwtClaimTypes.Scope)
            .Select(m => m.Value)
            .SelectMany(m => m.Split(' '))
            .ToHashSet();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var attribute in attributes)
        {
            if (attribute.Scopes.All(m => scopes.Contains(m)) == false)
                continue;

            return context.SucceedAsync(requirement);
        }

        return Task.CompletedTask;
    }
}

public class ScopeRequirement : IAuthorizationRequirement
{
    public static readonly ScopeRequirement Instance = new();
}

public class ScopeAuthorizationHandler : ScopeAuthorizationHandler<ScopeRequirement>;