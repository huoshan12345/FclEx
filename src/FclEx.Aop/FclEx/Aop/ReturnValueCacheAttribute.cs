namespace FclEx.Aop;

[AttributeUsage(AttributeTargets.Method)]
public class ReturnValueCacheAttribute : AbstractInterceptorAttribute
{
    public const string CacheName = "ReturnValueCache";
    private static readonly MethodInfo _taskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!;
    private static readonly ConcurrentDictionary<IServiceProvider, Context> _cache = new();

    private bool? _isStatic;
    public bool IsStatic
    {
        get => _isStatic ?? default;
        set => _isStatic = value;
    }

    private int? _expireSeconds;
    public int ExpireSeconds
    {
        get => _expireSeconds ?? default;
        set => _expireSeconds = value;
    }

    // null instance means a static key
    private string GetKey(MethodInfo method, object? instance, object?[] parameters, char? separator)
    {
        return StringBuilder.Build(m =>
        {
            m.Append(method.GetSignature());
            if (method.IsStatic == false && _isStatic != true)
            {
                m.Append(separator);
                m.Append(instance?.GetHashCode() ?? 0);
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
                var value = parameter?.GetHashCode() ?? 0;
                var bytes = BitConverter.GetBytes(value);
                hash.AppendData(bytes);
            }
            return hash.GetHashAndReset().ToHex();
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
            await context.Invoke(next);
            return;
        }

        var (cacheManager, serializer, logger) = GetContext(provider);
        var cache = cacheManager.GetCache<byte[]>(CacheName);

        // NOTE: we need to use context.Implementation instead of context.Proxy because we use the hashcode to identify the instance
        //      and the hashcode of the proxy is different from the hashcode of the implementation
        var key = GetKey(method, context.Implementation, context.Parameters, cacheManager.Options.Separator);

        if (cache.TryGet(key, out var str) == false)
        {
            await context.Invoke(next);
            var value = context.IsAsync()
                ? (object)(await (dynamic)context.ReturnValue)
                : context.ReturnValue;

            str = serializer.Serialize(value);

            TimeSpan? expire = _expireSeconds is null
                ? null
                : TimeSpan.FromSeconds(_expireSeconds.Value);

            cache.TrySet(key, str, expire);

            Log(false);

            return;
        }

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
                var m = typeof(ValueTask<>).MakeGenericType(objType);
                context.ReturnValue = Activator.CreateInstance(m, [item]);
            }
            else
            {
                throw new InvalidOperationException("unknown return type: " + returnType.ShortName());
            }
        }
        else
        {
            var item = serializer.Deserialize(str, returnType);
            context.ReturnValue = item;
        }

        Log(true);

        void Log(bool isHit)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("[{CacheName}][{CacheProvider}][{Method}]Cache {HitOrMiss}", CacheName, cacheManager.ProviderInfo.ProviderName, method.GetFullName(), isHit ? "hit" : "miss");
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