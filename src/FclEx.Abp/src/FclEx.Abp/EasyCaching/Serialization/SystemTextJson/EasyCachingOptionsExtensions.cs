using System;
using EasyCaching.Core.Configurations;

namespace EasyCaching.Serialization.SystemTextJson;

public static class EasyCachingOptionsExtensions
{
    public static EasyCachingOptions WithPatchedSystemTextJson(this EasyCachingOptions options,
        Action<JsonSerializerOptions>? configure = null, string name = "json")
    {
        options.RegisterExtension(new PatchedJsonOptionsExtension(name, configure));
        return options;
    }
}