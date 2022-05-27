using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using EasyCaching.Core.Serialization;
using EasyCaching.Serialization.Json;
using FclEx.Abp.Caching;
using FclEx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Overby.Extensions.Attachments;

namespace FclEx.Abp.Aop
{
    internal class ReturnValueCacheAttributeInfo
    {
        public ReturnValueCacheAttributeInfo(ICacheManager cacheManager, IEasyCachingSerializer serializer, ILogger logger)
        {
            CacheManager = cacheManager;
            Logger = logger;
            Serializer = serializer;
        }

        public ICacheManager CacheManager { get; }
        public IEasyCachingSerializer Serializer { get; }
        public ILogger Logger { get; }

        public void Deconstruct(out ICacheManager cacheManager, out IEasyCachingSerializer serializer, out ILogger logger)
        {
            cacheManager = CacheManager;
            serializer = Serializer;
            logger = Logger;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ReturnValueCacheAttribute : AbstractInterceptorAttribute
    {
        public const string CacheName = "returnvaluecache";
        private bool? _isStatic = null;
        private TimeSpan? _expire;
        private static readonly MethodInfo _taskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!;
        private static readonly MethodInfo _toValueTask = typeof(ValueTask).GetMethod(nameof(ValueTask.FromResult))!;

        private static readonly AsyncLock _locker = new();
        private static ReturnValueCacheAttributeInfo? _info;

        public bool IsStatic
        {
            set => _isStatic = value;
            get => _isStatic ?? default;
        }

        public TimeSpan Expire
        {
            set => _expire = value;
            get => _expire ?? default;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var method = context.ServiceMethod;
            var returnType = method.ReturnType;
            var provider = context.ServiceProvider;

            if (returnType == typeof(void)
                || returnType == typeof(Task)
                || returnType == typeof(ValueTask))
            {
                await context.Invoke(next).DonotCapture();
                return;
            }

            var (cacheManager, serializer, logger) = GetOrSetInfo(provider);
            var separator = cacheManager.CacheOptions.Separator;
            var cache = cacheManager.GetCache<byte[]>(CacheName);

            var key = method.GetSignature().ToLower();
            var parasKey = context.Parameters.ToJson().ToUtf8Bytes().ToMd5String();
            key = key + separator + parasKey;
            if (!method.IsStatic && _isStatic != true)
            {
                var instance = context.Proxy;
                key = key + separator + instance.GetReferenceId();
            }

            if (!cache.TryGet(key, out var str))
            {
                await context.Invoke(next);
                var value = context.IsAsync()
                    ? (object)(await (dynamic)context.ReturnValue)
                    : context.ReturnValue;

                str = serializer.Serialize(value);
                cache.TrySet(key, str, _expire);
            }
            else
            {
                if (context.IsAsync())
                {
                    var returnTypeOfGeneric = returnType.GetGenericTypeDefinition();
                    var objType = returnType.GenericTypeArguments[0];

                    var item = serializer.Deserialize(str, objType);
                    if (returnTypeOfGeneric == typeof(Task<>))
                    {
                        var m = _taskFromResult.MakeGenericMethod(objType);
                        context.ReturnValue = m.Invoke(null, new[] { item });
                    }
                    else if (returnTypeOfGeneric == typeof(ValueTask<>))
                    {
                        var m = _toValueTask.MakeGenericMethod(objType);
                        context.ReturnValue = m.Invoke(null, new[] { item });
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    var item = serializer.Deserialize(str, returnType);
                    context.ReturnValue = item;
                }
                logger.LogTrace($"[{CacheName}][{cache.ProviderType.Name}][Cache hit][{method.GetFullName()}]");
            }
        }



        private static ReturnValueCacheAttributeInfo GetOrSetInfo(IServiceProvider provider)
        {
            _locker.DoubleCheckAndDo(() => _info == null, () =>
             {
                 var logger = provider.CreateLogger<ReturnValueCacheAttribute>();
                 var cacheManager = provider.GetRequiredService<ICacheManager>();
                 var serializer = cacheManager.ProviderInfo.Serializer ?? new DefaultJsonSerializer("json", default);
                 _info = new ReturnValueCacheAttributeInfo(cacheManager, serializer, logger);
             });
            return _info!;
        }
    }
}
