// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using FclEx.Http.Proxy;
using FclEx.Utils;

namespace FclEx.Http.HttpClientFactory
{
    // Thread-safety: We treat this class as immutable except for the timer. Creating a new object
    // for the 'expiry' pool simplifies the threading requirements significantly.
    internal class ActiveHandlerTrackingEntry
    {
        private static readonly TimerCallback<ActiveHandlerTrackingEntry> _timerCallback = (s) => s.Timer_Tick();
        private readonly object _lock = new object();
        private bool _timerInitialized;
        private Timer<ActiveHandlerTrackingEntry>? _timer;
        private TimerCallback<ActiveHandlerTrackingEntry>? _callback;

        public ActiveHandlerTrackingEntry(IWebProxyExt proxy, LifetimeTrackingHttpMessageHandler handler, TimeSpan lifetime)
        {
            Handler = handler;
            Lifetime = lifetime;
            Proxy = proxy;
        }

        public IWebProxyExt Proxy { get; }

        public LifetimeTrackingHttpMessageHandler Handler { get; }

        public TimeSpan Lifetime { get; }

        public void StartExpiryTimer(TimerCallback<ActiveHandlerTrackingEntry> callback)
        {
            if (Lifetime == Timeout.InfiniteTimeSpan)
            {
                return; // never expires.
            }

            if (Volatile.Read(ref _timerInitialized))
            {
                return;
            }

            StartExpiryTimerSlow(callback);
        }

        private void StartExpiryTimerSlow(TimerCallback<ActiveHandlerTrackingEntry> callback)
        {
            Debug.Assert(Lifetime != Timeout.InfiniteTimeSpan);

            lock (_lock)
            {
                if (Volatile.Read(ref _timerInitialized))
                {
                    return;
                }

                _callback = callback;
                _timer = NonCapturingTimer.Create(_timerCallback, this, Lifetime, Timeout.InfiniteTimeSpan);
                _timerInitialized = true;
            }
        }

        private void Timer_Tick()
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;

                    _callback!(this);
                }
            }
        }
    }
}
