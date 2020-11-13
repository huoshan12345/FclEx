using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using FclEx.Http.HttpClientFactory;
using FclEx.Http.Proxy;

namespace FclEx.Utils
{
    public static class HttpHandlerHelper
    {
        private static readonly TimeSpan _defaultCleanupInterval = TimeSpan.FromSeconds(10);
        private static StatelessTimer? _cleanupTimer;
        private static readonly object _cleanupTimerLock = new object();
        private static readonly object _cleanupActiveLock = new object();

        private static readonly ConcurrentDictionary<IWebProxyExt, Lazy<ActiveHandlerTrackingEntry>> _activeHandlers
            = new ConcurrentDictionary<IWebProxyExt, Lazy<ActiveHandlerTrackingEntry>>();

        private static readonly Func<IWebProxyExt, Lazy<ActiveHandlerTrackingEntry>> _entryFactory
            = (proxy) => new Lazy<ActiveHandlerTrackingEntry>(() => CreateHandlerEntry(proxy), LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly ConcurrentQueue<ExpiredHandlerTrackingEntry> _expiredHandlers
            = new ConcurrentQueue<ExpiredHandlerTrackingEntry>();

        private static ActiveHandlerTrackingEntry CreateHandlerEntry(IWebProxyExt proxy)
        {
            // Wrap the handler so we can ensure the inner handler outlives the outer handler.
            var handler = new LifetimeTrackingHttpMessageHandler(CreateInternal(proxy));

            // Note that we can't start the timer here. That would introduce a very very subtle race condition
            // with very short expiry times. We need to wait until we've actually handed out the handler once
            // to start the timer.
            // 
            // Otherwise it would be possible that we start the timer here, immediately expire it (very short
            // timer) and then dispose it without ever creating a client. That would be bad. It's unlikely
            // this would happen, but we want to be sure.
            return new ActiveHandlerTrackingEntry(proxy, handler, TimeSpan.FromMinutes(2));
        }

        public static HttpMessageHandler Create(IWebProxyExt proxy)
        {
            var entry = _activeHandlers.GetOrAdd(proxy ?? WebProxyExt.None, _entryFactory).Value;
            entry.StartExpiryTimer(ExpiryTimer_Tick);
            return entry.Handler;
        }

        private static HttpMessageHandler CreateInternal(IWebProxyExt proxy)
        {
            switch (proxy.Type)
            {
                case ProxyType.None: return CreateDefaultHandler(null);

                case ProxyType.Http:
                case ProxyType.Https:
                    return CreateDefaultHandler(proxy);

                case ProxyType.Socks5:
                //return new ProxyClientHandler<Socks5>(new ProxySettings
                //{
                //    Port = proxy.Port,
                //    Host = proxy.Host,
                //    Credentials = proxy.Credentials as NetworkCredential
                //});
                default:
                    throw new NotSupportedException();
            }
        }

        private static HttpClientHandler CreateDefaultHandler(IWebProxy? proxy)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }
            else
            {
                handler.UseProxy = false;
                handler.Proxy = null;
            }
            return handler;
        }

        private static void ExpiryTimer_Tick(ActiveHandlerTrackingEntry active)
        {
            // The timer callback should be the only one removing from the active collection. If we can't find
            // our entry in the collection, then this is a bug.
            var removed = _activeHandlers.TryRemove(active.Proxy, out var found);
            Debug.Assert(removed, "Entry not found. We should always be able to remove the entry");
            Debug.Assert(ReferenceEquals(active, found.Value), "Different entry found. The entry should not have been replaced");

            // At this point the handler is no longer 'active' and will not be handed out to any new clients.
            // However we haven't dropped our strong reference to the handler, so we can't yet determine if
            // there are still any other outstanding references (we know there is at least one).
            //
            // We use a different state object to track expired handlers. This allows any other thread that acquired
            // the 'active' entry to use it without safety problems.
            var expired = new ExpiredHandlerTrackingEntry(active);
            _expiredHandlers.Enqueue(expired);
            StartCleanupTimer();
        }

        private static void StartCleanupTimer()
        {
            lock (_cleanupTimerLock)
            {
                if (_cleanupTimer == null)
                {
                    _cleanupTimer = NonCapturingTimer.Create(CleanupTimer_Tick, _defaultCleanupInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private static void StopCleanupTimer()
        {
            lock (_cleanupTimerLock)
            {
                _cleanupTimer!.Dispose();
                _cleanupTimer = null;
            }
        }

        private static void CleanupTimer_Tick()
        {
            // Stop any pending timers, we'll restart the timer if there's anything left to process after cleanup.
            //
            // With the scheme we're using it's possible we could end up with some redundant cleanup operations.
            // This is expected and fine.
            // 
            // An alternative would be to take a lock during the whole cleanup process. This isn't ideal because it
            // would result in threads executing ExpiryTimer_Tick as they would need to block on cleanup to figure out
            // whether we need to start the timer.
            StopCleanupTimer();

            if (!Monitor.TryEnter(_cleanupActiveLock))
            {
                // We don't want to run a concurrent cleanup cycle. This can happen if the cleanup cycle takes
                // a long time for some reason. Since we're running user code inside Dispose, it's definitely
                // possible.
                //
                // If we end up in that position, just make sure the timer gets started again. It should be cheap
                // to run a 'no-op' cleanup.
                StartCleanupTimer();
                return;
            }

            try
            {
                var initialCount = _expiredHandlers.Count;
                for (var i = 0; i < initialCount; i++)
                {
                    // Since we're the only one removing from _expired, TryDequeue must always succeed.
                    _expiredHandlers.TryDequeue(out var entry);
                    Debug.Assert(entry != null, "Entry was null, we should always get an entry back from TryDequeue");

                    if (entry!.CanDispose)
                    {
                        try
                        {
                            entry.InnerHandler.Dispose();
                        }
                        catch { }
                    }
                    else
                    {
                        // If the entry is still live, put it back in the queue so we can process it 
                        // during the next cleanup cycle.
                        _expiredHandlers.Enqueue(entry);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_cleanupActiveLock);
            }

            // We didn't totally empty the cleanup queue, try again later.
            if (_expiredHandlers.Count > 0)
            {
                StartCleanupTimer();
            }
        }
    }
}
