namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// Handles the authorization requirement by validating the user's claims against the required scopes.
/// </summary>
/// <typeparam name="T">The type of the authorization requirement.</typeparam>
/// <returns>
/// A completed task that represents the asynchronous operation. 
/// If the user satisfies the requirement, the method marks the requirement as succeeded in the context.
/// Otherwise, the requirement is left unmarked.
/// </returns>
/// <remarks>
/// The method evaluates the required scopes specified by <see cref="RequiredScopeAttribute"/> on the endpoint being accessed:
/// <list type="bullet">
/// <item>If no <see cref="RequiredScopeAttribute"/> is present, the method succeeds if the user is authenticated.</item>
/// <item>If the endpoint specifies required scopes, the method checks if all required scopes are present in the user's claims.</item>
/// <item>The user's scopes are extracted from claims of type <c>JwtClaimTypes.Scope</c>, where multiple scopes in a single claim are space-separated.</item>
/// </list>
/// If any <see cref="RequiredScopeAttribute"/> on the endpoint has all its required scopes satisfied by the user's claims, the requirement is marked as succeeded. 
/// Otherwise, it remains unfulfilled.
/// </remarks>
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
            if (attribute.Scopes.All(scopes.Contains) == false)
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