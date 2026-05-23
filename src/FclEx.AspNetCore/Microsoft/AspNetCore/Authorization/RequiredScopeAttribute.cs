namespace Microsoft.AspNetCore.Authorization;


/// <summary>
/// Specifies the required OAuth 2.0 scopes for accessing a resource.
/// </summary>
/// <remarks>
/// This attribute can be applied to controllers or action methods to indicate the OAuth 2.0 scopes 
/// that a user must possess to access the resource. Scopes are validated during the authorization process.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiredScopeAttribute(params string[] scopes) : Attribute
{
    public string[] Scopes { get; } = scopes;
}