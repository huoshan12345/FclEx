using System;

namespace FclEx.Abp.Caching.Configuration;

public interface IReadOnlyCacheOptions
{
    string GlobalPrefix { get; }
    char? Separator { get; }
    TimeSpan DefaultExpiration { get; }
}