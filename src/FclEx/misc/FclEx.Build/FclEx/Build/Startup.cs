using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Build.Locator;

namespace FclEx.Build;

public static class Startup
{
    [ModuleInitializer]
    public static void Init()
    {
        var instance = MSBuildLocator.QueryVisualStudioInstances()
            .OrderByDescending(instance => instance.Version)
            .First();
        // This method must be called, otherwise it will prompt that the msbuild file cannot be found
        MSBuildLocator.RegisterInstance(instance);
    }
}