namespace Xunit;

public class TestOutputWriter : TextWriter
{
    public TestOutputWriter(ITestOutputHelper output)
    {
        Output = output;
    }

    public ITestOutputHelper Output { get; }

    public override Encoding Encoding { get; } = Encoding.UTF8;

    public override void Write(char value) => Output.Write(value.ToString());
}