using System;
using System.Security.Cryptography;
using AspectCore.DynamicProxy;

namespace FclEx.Abp.Aop;

[AttributeUsage(AttributeTargets.Method)]
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

    // null instance means a static key
    private string GetKey(MethodInfo method, object? instance, object?[] parameters, char? separator)
    {
        return StringBuilderHelper.Build(m =>
        {
            m.Append(method.GetSignature());
            if (method.IsStatic == false && _isStatic != true)
            {
                m.Append(separator);
                m.Append(instance.GetHashCodeSafely());
            }
            var hash = Hash(parameters);
            m.Append(separator);
            m.Append(hash);
        });

        static string Hash(object?[] parameters)
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            foreach (var parameter in parameters)
            {
                var id = parameter.GetHashCodeSafely();
                var span = Span.AsBytes(ref id);
                hash.AppendData(span);
            }
            return hash.GetCurrentHash().ToHex();
        }
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
        var cache = cacheManager.GetCache<byte[]>(CacheName);
        var key = GetKey(method, context.Proxy, context.Parameters, cacheManager.CacheOptions.Separator);

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
                    context.ReturnValue = m.Invoke(null, [item]);
                }
                else if (returnTypeOfGeneric == typeof(ValueTask<>))
                {
                    var m = _toValueTask.MakeGenericMethod(objType);
                    context.ReturnValue = m.Invoke(null, [item]);
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