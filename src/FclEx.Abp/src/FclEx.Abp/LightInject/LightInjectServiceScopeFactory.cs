namespace LightInject;

internal class LightInjectServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceContainer _container;

    public LightInjectServiceScopeFactory(IServiceContainer container)
    {
        _container = container;
    }

    public IServiceScope CreateScope()
    {
        var scope = _container.BeginScope();

        return new LightInjectServiceScope(scope);
    }
}