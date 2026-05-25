namespace FclEx.Caching;

public class CacheOptions(string name)
{
    public string Name { get; } = name;
    public bool OnlyUseLowerCase { get; set; } = true;
    public bool UsePrefix { get; set; } = true;
    public bool UseGlobalPrefix { get; set; } = false;
    public TimeSpan? DefaultExpiration { get; set; }
}