// ReSharper disable ClassNeverInstantiated.Local
namespace FclEx.AspNetCore.Extensions;

public class EndpointExtensionsTests
{
    [Fact]
    public void GetAttributes_WithActionAttributesOnly_ShouldReturnActionAttributes()
    {
        var endpoint = CreateEndpoint<TestController>(nameof(TestController.ActionOnly));

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: false);

        Assert.Single(attributes);
        Assert.Equal("Action", attributes[0].Value);
    }

    [Fact]
    public void GetAttributes_CombineControllerAttributesOnly_ShouldReturnControllerAttributesWhenActionHasNone()
    {
        var endpoint = CreateEndpoint<TestController>(nameof(TestController.NoActionAttributes));

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: false);

        Assert.Single(attributes);
        Assert.Equal("Controller", attributes[0].Value);
    }

    [Fact]
    public void GetAttributes_WithBothAttributes_ShouldReturnOnlyActionAttributesWhenCombineControllerIsFalse()
    {
        var endpoint = CreateEndpoint<TestController>(nameof(TestController.ActionAndController));

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: false);

        Assert.Single(attributes);
        Assert.Equal("Action", attributes[0].Value);
    }

    [Fact]
    public void GetAttributes_WithBothAttributes_ShouldReturnCombinedAttributesWhenCombineControllerIsTrue()
    {
        var endpoint = CreateEndpoint<TestController>(nameof(TestController.ActionAndController));

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: true);

        Assert.Equal(2, attributes.Length);
        Assert.Contains(attributes, attr => attr.Value == "Action");
        Assert.Contains(attributes, attr => attr.Value == "Controller");
    }

    [Fact]
    public void GetAttributes_WithNoAttributes_ShouldReturnEmptyArray()
    {
        var endpoint = CreateEndpoint<TestController>(nameof(TestController.NoAttributes));

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: true);

        Assert.Single(attributes);
        Assert.Equal("Controller", attributes[0].Value);
    }

    [Fact]
    public void GetAttributes_WhenNoActionDescriptor_ShouldReturnEmptyArray()
    {
        var endpoint = new Endpoint(
            requestDelegate: null,
            new EndpointMetadataCollection(),
            displayName: null);

        var attributes = endpoint.GetAttributes<TestAttribute>(combineController: true);

        Assert.Empty(attributes);
    }

    private static Endpoint CreateEndpoint<TController>(string actionName)
    {
        var methodInfo = typeof(TController).GetRequiredMethod(actionName);
        var controllerTypeInfo = typeof(TController).GetTypeInfo();

        var actionDescriptor = new ControllerActionDescriptor
        {
            MethodInfo = methodInfo,
            ControllerTypeInfo = controllerTypeInfo,
        };

        return new Endpoint(
            requestDelegate: null,
            new EndpointMetadataCollection(actionDescriptor),
            displayName: null);
    }

    [Test("Controller")]
    private class TestController
    {
        [Test("Action")]
        public void ActionOnly() { }

        public void NoActionAttributes() { }

        [Test("Action")]
        public void ActionAndController() { }

        public void NoAttributes() { }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    private class TestAttribute : Attribute
    {
        public string Value { get; }
        public TestAttribute(string value) => Value = value;
    }
}