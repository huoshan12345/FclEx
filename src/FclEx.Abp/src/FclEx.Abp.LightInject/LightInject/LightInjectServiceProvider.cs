using System;

namespace LightInject;

public class LightInjectServiceProvider : IServiceProvider, IDisposable
{
    private readonly IServiceFactory _serviceFactory;
    private bool _isDisposed = false;

    public LightInjectServiceProvider(IServiceFactory serviceFactory)
    {
        this._serviceFactory = serviceFactory;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        if (_serviceFactory is IDisposable disposable)
            disposable.Dispose();
    }

    public object GetService(Type serviceType)
    {
        return _serviceFactory.TryGetInstance(serviceType);
    }
}