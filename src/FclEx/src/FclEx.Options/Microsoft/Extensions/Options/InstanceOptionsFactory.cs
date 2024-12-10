namespace Microsoft.Extensions.Options;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class InstanceOptionsFactory<TOptions> : OptionsFactory<TOptions> where TOptions : class
{
    private readonly IServiceProvider _provider;
    private readonly Func<IServiceProvider, string, TOptions> _optionFactory;

    public InstanceOptionsFactory(
        IServiceProvider provider,
        Func<IServiceProvider, string, TOptions> optionFactory)
        : base(
            provider.GetServices<IConfigureOptions<TOptions>>(),
            provider.GetServices<IPostConfigureOptions<TOptions>>(),
            provider.GetServices<IValidateOptions<TOptions>>())
    {
        _optionFactory = optionFactory;
        _provider = provider;
    }

    protected override TOptions CreateInstance(string name)
    {
        return _optionFactory.Invoke(_provider, name);
    }
}