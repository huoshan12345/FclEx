// ReSharper disable CoVariantArrayConversion

namespace FclEx.AspNetCore.Extensions;

public class AuthorizationHandlerContextExtensionsTests
{
    [Fact]
    public void GetAttributes_ResourceNotHttpContext_ReturnsEmptyArray()
    {
        var requirements = Array.Empty<IAuthorizationRequirement>();
        var user = new ClaimsPrincipal();

        var authContext = new AuthorizationHandlerContext(requirements, user, resource: "NonHttpContextResource");

        var result = authContext.GetAttributes<CustomAttribute>(combineController: true);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAttributes_ResourceIsHttpContextWithoutEndpoint_ReturnsEmptyArray()
    {
        var requirements = Array.Empty<IAuthorizationRequirement>();
        var user = new ClaimsPrincipal();
        var httpContext = new DefaultHttpContext(); // No endpoint set.

        var authContext = new AuthorizationHandlerContext(requirements, user, httpContext);

        var result = authContext.GetAttributes<CustomAttribute>(combineController: true);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAttributes_EndpointWithoutActionDescriptor_ReturnsEmptyArray()
    {
        var requirements = Array.Empty<IAuthorizationRequirement>();
        var user = new ClaimsPrincipal();
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(), "TestEndpoint");
        httpContext.SetEndpoint(endpoint);

        var authContext = new AuthorizationHandlerContext(requirements, user, httpContext);

        var result = authContext.GetAttributes<CustomAttribute>(combineController: true);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAttributes_CombineControllerFalse_ReturnsOnlyActionAttributes()
    {
        var requirements = Array.Empty<IAuthorizationRequirement>();
        var user = new ClaimsPrincipal();
        var httpContext = new DefaultHttpContext();

        var actionAttributes = new[] { new CustomAttribute("ActionAttribute") };
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(new ControllerActionDescriptor
        {
            MethodInfo = TestUtils.MockMethodInfoWithAttributes(actionAttributes),
            ControllerTypeInfo = TestUtils.MockTypeInfoWithAttributes<Attribute>(),
        }), "TestEndpoint");
        httpContext.SetEndpoint(endpoint);

        var authContext = new AuthorizationHandlerContext(requirements, user, httpContext);

        var result = authContext.GetAttributes<CustomAttribute>(combineController: false);

        Assert.Equal(actionAttributes, result);
    }

    [Fact]
    public void GetAttributes_CombineControllerTrue_ReturnsCombinedAttributes()
    {
        var requirements = Array.Empty<IAuthorizationRequirement>();
        var user = new ClaimsPrincipal();
        var httpContext = new DefaultHttpContext();

        var actionAttributes = new[] { new CustomAttribute("ActionAttribute") };
        var controllerAttributes = new[] { new CustomAttribute("ControllerAttribute") };
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(new ControllerActionDescriptor
        {
            MethodInfo = TestUtils.MockMethodInfoWithAttributes(actionAttributes),
            ControllerTypeInfo = TestUtils.MockTypeInfoWithAttributes(controllerAttributes),
        }), "TestEndpoint");
        httpContext.SetEndpoint(endpoint);

        var authContext = new AuthorizationHandlerContext(requirements, user, httpContext);

        var result = authContext.GetAttributes<CustomAttribute>(combineController: true);

        Assert.Equal(actionAttributes.Concat(controllerAttributes), result);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CustomAttribute : Attribute
{
    public string Name { get; }
    public CustomAttribute(string name) => Name = name;
}

public static class TestUtils
{
    public static MethodInfo MockMethodInfoWithAttributes<T>(T[] attributes) where T : Attribute
    {
        var mock = new Mock<MethodInfo>();
        mock.Setup(m => m.GetCustomAttributes(typeof(CustomAttribute), It.IsAny<bool>()))
            .Returns(attributes);
        return mock.Object;
    }

    public static TypeInfo MockTypeInfoWithAttributes<T>(T[]? attributes = null) where T : Attribute
    {
        attributes ??= [];
        var mock = new Mock<TypeInfo>();
        mock.Setup(t => t.GetCustomAttributes(typeof(CustomAttribute), It.IsAny<bool>()))
            .Returns(attributes);
        return mock.Object;
    }
}