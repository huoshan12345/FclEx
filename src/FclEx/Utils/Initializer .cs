using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public class Initializer
    {
        private volatile bool _isInitialized;
        private readonly AsyncLocker _asyncLocker;

        public Initializer(bool isThreadSafe = true)
        {
            if (isThreadSafe)
                _asyncLocker = new AsyncLocker();
        }


        public void Init(Action action)
        {
            if (!_isInitialized)
            {
                using (_asyncLocker?.Lock())
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
                using (_asyncLocker?.Lock())
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
