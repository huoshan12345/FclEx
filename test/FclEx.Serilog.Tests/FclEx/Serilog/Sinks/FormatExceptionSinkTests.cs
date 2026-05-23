namespace FclEx.Serilog.Sinks;

[Collection(nameof(Console))]
public class FormatExceptionSinkTests
{
    private static readonly ILogger Logger = new LoggerConfiguration().WriteTo
        .FormatException(m => m.Console(outputTemplate: Constants.DefaultOutputTemplate))
        .CreateLogger()
        .ForContext<FormatExceptionSinkTests>();

    private readonly ITestOutputHelper _output;

    public FormatExceptionSinkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test()
    {
        using var writer = new StringWriter();
        using var x = writer.SetConsole();
        try
        {
            throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, ex.Message);
        }

        _output.WriteLine(writer.ToString());
    }
}