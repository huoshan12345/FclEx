using System;
using System.Text;

namespace RabbitMQ.Client;

public class AutoCloseableModel : IDisposable
{
    private readonly IModel _innerModel;
    private bool _disposed;

    public AutoCloseableModel(IModel innerModel)
    {
        _innerModel = innerModel;
    }

    public IModel Model
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(_innerModel));
            return _innerModel;
        }
    }

    public void Dispose()
    {
        if (_disposed) 
            return;

        _innerModel.Close();
        _innerModel.Dispose();
        _disposed = true;
    }
}