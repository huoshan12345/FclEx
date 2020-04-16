using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace FclEx.Utils
{
    public class Initializer
    {
        private volatile bool _isInitialized;
        private readonly AsyncLock _asyncLock;

        public Initializer(bool isThreadSafe = true)
        {
            if (isThreadSafe)
                _asyncLock = new AsyncLock();
        }


        public void Init(Action action)
        {
            if (!_isInitialized)
            {
                using (_asyncLock?.Lock())
                {
                    if (!_isInitialized)
                    {
                        action();
                        _isInitialized = true;
                    }
                }
            }
        }

        public async Task InitAsync(Func<Task> action)
        {
            if (!_isInitialized)
            {
                using (_asyncLock?.Lock())
                {
                    if (!_isInitialized)
                    {
                        await action().DonotCapture();
                        _isInitialized = true;
                    }
                }
            }
        }
    }
}
