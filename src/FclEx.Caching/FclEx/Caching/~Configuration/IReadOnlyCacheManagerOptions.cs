namespace FclEx.Caching;

public interface IReadOnlyCacheManagerOptions
{
    char? Separator { get; }
    TimeSpan DefaultExpiration { get; }
    string GlobalPrefix { get; }
}