using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T GetServiceOr<T>(this IServiceProvider resolver, T defaultValue) where T : class
            => resolver.GetService<T>() ?? defaultValue;

        public static ILoggerFactory GetLoggerFactory(this IServiceProvider resolver)
        {
            return resolver.GetServiceOr<ILoggerFactory>(NullLoggerFactory.Instance);
        }

        public static ILogger CreateLogger(this IServiceProvider provider, string name)
            => provider.GetLoggerFactory().CreateLogger(name);

        public static ILogger CreateLogger(this IServiceProvider provider, Type type)
            => provider.GetLoggerFactory().CreateLogger(type);

        public static ILogger<T> CreateLogger<T>(this IServiceProvider provider)
            => provider.GetLoggerFactory().CreateLogger<T>();

        public static void ResolveAndDo<T>(this IServiceProvider provider, Action<T> action) where T : notnull
        {
            var service = provider.GetRequiredService<T>();
            action(service);
        }

        public static void ResolveAndDo<T1, T2>(this IServiceProvider provider, Action<T1, T2> action) where T1 : notnull where T2 : notnull
        {
            var t1 = provider.GetRequiredService<T1>();
            var t2 = provider.GetRequiredService<T2>();
            action(t1, t2);
        }

        public static Task ResolveAndDoAsync<T>(this IServiceProvider provider, Func<T, Task> action) where T : notnull
        {
            var service = provider.GetRequiredService<T>();
            return action(service);
        }

        public static Task ResolveAndDoAsync<T1, T2>(this IServiceProvider provider, Func<T1, T2, Task> action) where T1 : notnull where T2 : notnull
        {
            var t1 = provider.GetRequiredService<T1>();
            var t2 = provider.GetRequiredService<T2>();
            return action(t1, t2);
        }

        public static TResult ResolveAndDo<T, TResult>(this IServiceProvider provider, Func<T, TResult> action) where T : notnull
        {
            var service = provider.GetRequiredService<T>();
            return action(service);
        }

        public static TResult ResolveAndDo<T1, T2, TResult>(this IServiceProvider provider, Func<T1, T2, TResult> action) where T1 : notnull where T2 : notnull
        {
            var t1 = provider.GetRequiredService<T1>();
            var t2 = provider.GetRequiredService<T2>();
            return action(t1, t2);
        }

        public static Task<TResult> ResolveAndDoAsync<T, TResult>(this IServiceProvider provider, Func<T, Task<TResult>> action) where T : notnull
        {
            var service = provider.GetRequiredService<T>();
            return action(service);
        }

        public static Task<TResult> ResolveAndDoAsync<T1, T2, TResult>(this IServiceProvider provider, Func<T1, T2, Task<TResult>> action) where T1 : notnull where T2 : notnull
        {
            var t1 = provider.GetRequiredService<T1>();
            var t2 = provider.GetRequiredService<T2>();
            return action(t1, t2);
        }
    }
}
