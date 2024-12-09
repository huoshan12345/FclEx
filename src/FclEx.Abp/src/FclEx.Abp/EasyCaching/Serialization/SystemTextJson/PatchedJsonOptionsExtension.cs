using System;
using System.Collections.Generic;
using EasyCaching.Core.Configurations;
using FclEx.Json;
using Microsoft.Extensions.Options;

namespace EasyCaching.Serialization.SystemTextJson;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
internal sealed class PatchedJsonOptionsExtension : IEasyCachingOptionsExtension
{
    private readonly string _name;
    private readonly Action<JsonSerializerOptions> _configure;

    public PatchedJsonOptionsExtension(string name, Action<JsonSerializerOptions>? configure)
    {
        _name = name;
        _configure = configure ?? (m => { });
    }

    public void AddServices(IServiceCollection services)
    {
        services.AddOptions();
        services.AddOptionsInstance<JsonSerializerOptions>(JsonHelper.GetOptions());
        services.Configure(_name, _configure);
        services.AddSingleton<IEasyCachingSerializer, PatchedJsonSerializer>(x =>
        {
            var optionsMon = x.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>();
            var options = optionsMon.Get(_name);
            return new PatchedJsonSerializer(_name, options);
        });
    }
}