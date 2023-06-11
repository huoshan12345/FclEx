using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FclEx.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add<T, TImpl>(this IServiceCollection services, ServiceLifetime lifetime)
        where T : class where TImpl : class, T
    {
        Check.NotNull(services);
        services.Add(new ServiceDescriptor(typeof(T), typeof(TImpl), lifetime));
        return services;
    }

    public static IServiceCollection TryAdd<T, TImpl>(this IServiceCollection services, ServiceLifetime lifetime)
        where T : class where TImpl : class, T
    {
        Check.NotNull(services);
        services.TryAdd(new ServiceDescriptor(typeof(T), typeof(TImpl), lifetime));
        return services;
    }

    public static IServiceCollection AddSingletonBy<T, TDependency>(this IServiceCollection col, Func<TDependency, T> func) where TDependency : notnull
        where T : class
    {
        return col.AddSingleton(s => func(s.GetRequiredService<TDependency>()));
    }

    public static IServiceCollection TryAddSingletonBy<T, TDependency>(this IServiceCollection col, Func<TDependency, T> func) where TDependency : notnull
        where T : class
    {
        col.TryAddSingleton(s => func(s.GetRequiredService<TDependency>()));
        return col;
    }

    public static IServiceCollection WrapFor<T>(this IServiceCollection col, Func<T, T> func, ServiceLifetime lifetime = ServiceLifetime.Singleton) where T : notnull
    {
        var descriptor = col.FirstOrDefault(m => m.ServiceType == typeof(T));
        if (descriptor == null)
            throw new InvalidOperationException("There is no registered service of type: " + typeof(T).LongName());

        Func<IServiceProvider, object>? factory = null;
        var provider = col.BuildServiceProvider();
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
            {
                var instance = provider.GetRequiredService<T>();
                factory = _ => func(instance)!;
                break;
            }
            case ServiceLifetime.Scoped:
            {
                factory = _ =>
                {
                    using var scope = provider.CreateScope();
                    var instance = scope.ServiceProvider.GetRequiredService<T>();
                    return func(instance)!;
                };
                break;
            }
            case ServiceLifetime.Transient:
            {
                factory = _ =>
                {
                    var instance = provider.GetRequiredService<T>();
                    return func(instance)!;
                };
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        col.Add(new ServiceDescriptor(typeof(T), factory, lifetime));
        return col;
    }

    public static IServiceCollection AddIfNotExist(this IServiceCollection services, ServiceDescriptor descriptor)
    {
        if (!services.Contains(descriptor, ServiceDescriptorEqualityComparer.Instance))
            services.Add(descriptor);
        return services;
    }

    public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services, TImplementation implementationInstance)
        where TService : class
        where TImplementation : class, TService
    {
        var impl = typeof(TImplementation);
        services.RemoveAll(m => m.ServiceType == typeof(TService) && (m.ImplementationType == impl || m.ImplementationInstance?.GetType() == impl));
        services.AddSingleton<TService>(implementationInstance);
        return services;
    }

    public static IServiceCollection Replace<TService>(this IServiceCollection services, TService implementationInstance)
        where TService : class
    {
        services.RemoveAll(m => m.ServiceType == typeof(TService));
        services.AddSingleton<TService>(implementationInstance);
        return services;
    }

    public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime, params object[] args)
        where TService : class
        where TImplementation : class, TService
    {
        Check.NotNull(services);
        Check.NotNull(args);
        Check.HasNoNulls(args);

        if (args.IsEmpty())
        {
            services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));
        }
        else
        {
            var t = typeof(TImplementation);
            var ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors.IsEmpty())
                throw new InvalidOperationException("Cannot find any public constructors for type " + t.LongName());

            services.Add(ServiceDescriptor.Describe(typeof(TService), s => Create(s, t, ctors, args), lifetime));
        }
        return services;

        static MatchResult GetMatchResult(ParameterInfo[] paras, IReadOnlyList<Type> argTyps)
        {
            var matchItems = new List<MatchItem>(paras.Length);
            for (int i = 0, j = 0; i < paras.Length && j < argTyps.Count; i++)
            {
                var para = paras[i];
                if (para.ParameterType.IsAssignableFrom(argTyps[j]))
                {
                    matchItems.Add(new MatchItem(i, j));
                    ++j;
                }
                else
                {
                    matchItems.Add(new MatchItem(i, -1));
                }
            }

            return new MatchResult(matchItems);
        }

        static bool TryCreate(IServiceProvider provider, ConstructorInfo ctor, ParameterInfo[] paras, object[] args, MatchResult match, out object? obj)
        {
            var actualArgs = new object[match.MatchItems.Count];
            foreach (var (paraIndex, argIndex, matched) in match.MatchItems)
            {
                if (matched)
                {
                    actualArgs[paraIndex] = args[argIndex];
                }
                else
                {
                    var paraType = paras[paraIndex].ParameterType;
                    var p = provider.GetService(paraType);
                    if (p == null)
                    {
                        obj = null;
                        return false;
                    }
                    else
                    {
                        actualArgs[paraIndex] = p;
                    }
                }
            }
            obj = ctor.Invoke(actualArgs);
            return true;
        }

        static object Create(IServiceProvider provider, Type implementType, ConstructorInfo[] ctors, object[] args)
        {
            var argTypes = args.Select(m => m.GetType()).ToList();
            foreach (var (ctor, paras, match) in ctors.Select(m => (Ctor: m, Paras: m.GetParameters()))
                         .Select(m => (m.Ctor, m.Paras, Match: GetMatchResult(m.Paras, argTypes)))
                         .OrderByDescending(m => m.Match.MatchCount))
            {
                if (TryCreate(provider, ctor, paras, args, match, out var obj))
                    return obj!;
            }
            throw new InvalidOperationException("Cannot find any public constructors that can match all arguments for type " + implementType.LongName());
        }
    }

    public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, params object[] args)
        where TService : class
        where TImplementation : class, TService
    {
        return services.Add<TService, TImplementation>(ServiceLifetime.Singleton, args);
    }

    private readonly struct MatchResult
    {
        public MatchResult(List<MatchItem> matchItems)
        {
            MatchItems = matchItems;
            MatchCount = matchItems.Count(m => m.IsMatched);
        }

        public int MatchCount { get; }
        public List<MatchItem> MatchItems { get; }
    }

    private readonly struct MatchItem
    {
        public MatchItem(int paraIndex, int argIndex)
        {
            ParaIndex = paraIndex;
            ArgIndex = argIndex;
        }

        public int ParaIndex { get; }
        public int ArgIndex { get; }
        public bool IsMatched => ArgIndex >= 0;

        public void Deconstruct(out int paraIndex, out int argIndex, out bool isMatched)
        {
            paraIndex = ParaIndex;
            argIndex = ArgIndex;
            isMatched = IsMatched;
        }
    }
}