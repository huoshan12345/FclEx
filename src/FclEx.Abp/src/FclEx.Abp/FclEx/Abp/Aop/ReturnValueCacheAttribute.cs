using System;
using System.Collections.Concurrent;
using System.Reflection;
using AspectCore.DynamicProxy;
using EasyCaching.Core.Serialization;
using EasyCaching.Serialization.Json;
using FclEx.Abp.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Overby.Extensions.Attachments;

namespace FclEx.Abp.Aop;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ReturnValueCacheAttribute : AbstractInterceptorAttribute
{
    public const string CacheName = "ReturnValueCache";
    private static readonly MethodInfo _taskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!;
    private static readonly MethodInfo _toValueTask = typeof(ValueTask).GetMethod(nameof(ValueTask.FromResult))!;
    private static readonly ConcurrentDictionary<IServiceProvider, Context> _cache = new();

    private bool? _isStatic;
    public bool IsStatic
    {
        set => _isStatic = value;
        get => _isStatic ?? default;
    }

    private TimeSpan? _expire;
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
            await context.Invoke(next).IgnoreSyncContext();
            return;
        }

        var (cacheManager, serializer, logger) = GetContext(provider);
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

            if (logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("[{CacheName}][{CacheProvider}][{Method}]Cache hit", CacheName, cache.ProviderType.Name, method.GetFullName());
        }
    }

    private static Context GetContext(IServiceProvider provider)
    {
        return _cache.GetOrAdd(provider, static m =>
        {
            var logger = m.CreateLogger<ReturnValueCacheAttribute>();
            var cacheManager = m.GetRequiredService<ICacheManager>();
            var serializer = cacheManager.ProviderInfo.Serializer ?? new DefaultJsonSerializer("json", default);
            return new Context(cacheManager, serializer, logger);
        });
    }

    internal record Context(ICacheManager CacheManager, IEasyCachingSerializer Serializer, ILogger Logger);

}