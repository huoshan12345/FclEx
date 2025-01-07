namespace Microsoft.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiredScopeAttribute(params string[] scopes) : Attribute
{
    public string[] Scopes { get; } = scopes;
}