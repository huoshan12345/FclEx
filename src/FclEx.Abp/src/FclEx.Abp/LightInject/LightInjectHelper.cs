using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using AspectCore.Extensions.LightInject;
using FclEx;
using FclEx.Helpers;

namespace LightInject;

public static class LightInjectHelper
{
    private static readonly string[] _ignoreMsg =
    {
        "Unable to precompile open generic type"
    };

    private static void ThrowWhen(this LogEntry log, Func<LogEntry, bool> condition)
    {
        if (condition(log))
            throw new InvalidOperationException($"[{log.Level}]{log.Message}");
    }

    private static void ThrowWhenWarning(this LogEntry log)
    {
        ThrowWhen(log, l => l.Level == LogLevel.Warning
                            && _ignoreMsg.All(m => !log.Message.StartsWith(m)));
    }

    private static Action<LogEntry> CreateLogFactory(Type type, bool enableDebuggerLogging, bool throwOnWarningLog)
    {
        var name = type.ShortName();
        return (enableDebuggerLogging, throwOnWarningLog) switch
        {
            (true, true) => log =>
            {
                log.ThrowWhenWarning();
                DebuggerHepler.WriteLine(() => $"[{name}][{log.Level}]{log.Message}");
            },
            (true, false) => log =>
            {
                DebuggerHepler.WriteLine(() => $"[{name}][{log.Level}]{log.Message}");
            },
            (false, true) => log =>
            {
                log.ThrowWhenWarning();
            },
            _ => log => { }
        };
    }

    private static ContainerOptions CreateContainerOptions(bool enableDebuggerLogging, bool throwOnWarningLog)
    {
        var options = new ContainerOptions
        {
            EnableOptionalArguments = true,
            EnablePropertyInjection = false,
            EnableVariance = false,
            DefaultServiceSelector = services => services.Last(),
            LogFactory = t => CreateLogFactory(t, enableDebuggerLogging, throwOnWarningLog)
        };
        return options;
    }

    public static IServiceContainer CreateContainer(LightInjectOptions? options = null)
    {
        options ??= LightInjectOptions.Default;
        var containerOptions = CreateContainerOptions(options.EnableDebuggerLogging, options.ThrowOnWarningLog);
        var container = new ServiceContainer(containerOptions);
        if (options.UseAop)
        {
            container.RegisterDynamicProxy();
        }
        container.AddMsDiScope();
        return container;
    }

    public static (IServiceContainer container, IServiceProviderFactory<IServiceContainer> factory)
        CreateContainerAndFactory(Action<LightInjectOptions>? optionsBuilder = null)
    {
        var lightInjectOptions = new LightInjectOptions();
        optionsBuilder?.Invoke(lightInjectOptions);
        var container = CreateContainer(lightInjectOptions);
        var fac = LightInjectServiceProviderFactory.Create(container);
        return (container, fac);
    }
}