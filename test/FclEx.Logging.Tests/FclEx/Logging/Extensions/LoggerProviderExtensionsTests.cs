namespace FclEx.Logging.Extensions;

public class LoggerProviderExtensionsTests
{
    [Fact]
    public void CreateLogger_NullProviderOrType_ThrowsArgumentNullException()
    {
        using var provider = new CollectingLoggerProvider();
        Assert.Equal("provider", Assert.Throws<ArgumentNullException>(() =>
            LoggerProviderExtensions.CreateLogger(null!, typeof(string))).ParamName);
        Assert.Equal("type", Assert.Throws<ArgumentNullException>(() => provider.CreateLogger((Type)null!)).ParamName);
    }

    public static TheoryData<Type> CategoryTypes => new()
    {
        typeof(int), typeof(string), typeof(List<int>), typeof(Dictionary<,>),
        typeof(Outer<int>.Inner<string>), typeof(int[]), typeof(List<int>[,]),
        typeof(LoggerProviderExtensionsTests), typeof(Outer<>).GetGenericArguments()[0]
    };

    [Theory]
    [MemberData(nameof(CategoryTypes))]
    public void CreateLogger_Type_UsesTheStandardFactoryCategory(Type type)
    {
        using var provider = new CollectingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));

        factory.CreateLogger(type).LogInformation("factory");
        provider.CreateLogger(type).LogInformation("provider");

        Assert.Equal(provider.Entries[0].Category, provider.Entries[1].Category);
    }

    [Fact]
    public void CreateLogger_Generic_UsesTheStandardFactoryCategory()
    {
        using var provider = new CollectingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        factory.CreateLogger<List<int>>().LogInformation("factory");
        provider.CreateLogger<List<int>>().LogInformation("provider");
        Assert.Equal(provider.Entries[0].Category, provider.Entries[1].Category);
    }

    private class Outer<T>
    {
        public class Inner<TInner>;
    }
}
