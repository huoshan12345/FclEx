namespace FclEx.AspNetCore.Authorization;

public class ScopeAuthorizationHandlerTests
{
    private class TestRequirement : IAuthorizationRequirement;

    [Fact]
    public async Task HandleRequirementAsync_WithNoAttributesAndAuthenticatedUser_ShouldSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim> { new(ClaimTypes.Name, "test") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoAttributesAndUnauthenticatedUser_ShouldNotSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMatchingScope_ShouldSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Scope, "read write"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var attributes = new[] { new RequiredScopeAttribute("read") };
        var resource = MockHttpContextWithAttributes(attributes);
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonMatchingScope_Authenticated_ShouldNotSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Scope, "read write"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var attributes = new[] { new RequiredScopeAttribute("admin") };
        var resource = MockHttpContextWithAttributes(attributes);
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRequiredScopes_AllMatch_ShouldSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Scope, "read write delete"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var attributes = new[] { new RequiredScopeAttribute("read", "write") };
        var resource = MockHttpContextWithAttributes(attributes);
        var context = new AuthorizationHandlerContext([requirement], user, resource);


        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRequiredScopes_OneDoesNotMatch_Authenticated_ShouldNotSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Scope, "read write"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var attributes = new[] { new RequiredScopeAttribute("read", "admin") };
        var resource = MockHttpContextWithAttributes(attributes);
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRequiredScopeAttributes_OneMatches_ShouldSucceed()
    {
        var handler = new ScopeAuthorizationHandler<TestRequirement>();
        var requirement = new TestRequirement();
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Scope, "read write"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth"));
        var attributes = new[] { new RequiredScopeAttribute("read", "admin"), new RequiredScopeAttribute("read", "write") };
        var resource = MockHttpContextWithAttributes(attributes);
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    private static object MockHttpContextWithAttributes(IEnumerable<RequiredScopeAttribute> attributes)
    {
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(
            context => Task.CompletedTask,
            new EndpointMetadataCollection(attributes),
            "TestEndpoint"
        );
        httpContext.SetEndpoint(endpoint);
        return httpContext;
    }
}