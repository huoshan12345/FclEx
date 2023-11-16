using Nito.AsyncEx;

namespace FclEx.Utils;

public class Initializer
{
    private volatile bool _isInitialized;
    private readonly AsyncLock? _asyncLock;

    public Initializer(bool isThreadSafe = true)
    {
        if (isThreadSafe)
            _asyncLock = new AsyncLock();
    }


    public void Init(Action action)
    {
        if (_isInitialized)
            return;

        using (_asyncLock?.Lock())
        {
            if (_isInitialized)
                return;

            action();
            _isInitialized = true;
        }
    }

    public async Task InitAsync(Func<Task> action)
    {
        if (_isInitialized)
            return;

        using (_asyncLock?.Lock())
        {
            if (_isInitialized)
                return;

            await action().IgnoreSyncContext();
            _isInitialized = true;
        }

    }
}