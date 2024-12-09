using System;
using AspectCore.Extensions.LightInject;

namespace LightInject;

internal class LightInjectServiceScope : IServiceScope
{
    public LightInjectServiceScope(Scope scope)
    {
        Scope = scope;
        ServiceProvider = new LightInjectServiceResolver(scope);
    }

    public IServiceProvider ServiceProvider { get; }

    public Scope Scope { get; }

    public void Dispose()
    {
        Scope.Dispose();
    }
}