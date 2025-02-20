namespace FclEx.Extensions;

partial class ExceptionExtensionsTests
{
    private readonly ITestOutputHelper _output;

    public ExceptionExtensionsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void Print(Exception ex)
    {
        _output.WriteLine(ex.ToString());
        _output.WriteLine(Environment.NewLine);

        var (infos, _) = ex.BuildTree();

        foreach (var info in infos)
        {
            _output.WriteLine($"[{info.Index}->{info.ParentIndex}][{info.Type.Name}]: " + info.Message);
            foreach (var stackTrace in info.StackTraceLines)
            {
                _output.WriteLine(stackTrace);
            }
        }
    }

    [Fact]
    public async Task BuildTree_Complex_Test()
    {
        try
        {
            await ExceptionCreator.Run();
        }
        catch (Exception ex)
        {
            Print(ex);
        }
    }

    [Fact]
    public void GetInfo_Test()
    {
        var text = File.ReadAllText(Path.Combine("TestData", "StackTrace.txt"));
        var ex = new Exception().SetStackTrace(text);

        var index = 0;
        var info = ex.GetInfo(ref index, -1, null);

        foreach (var line in info.StackTraceLines)
        {
            Assert.True(ExceptionExtensions.ExcludeStackTracePrefixes.All(m => line.StartsWith(m) == false));
            _output.WriteLine(line);
        }
    }
}