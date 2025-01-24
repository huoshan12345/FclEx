using System.IO;
using System.Threading.Tasks;

namespace Xunit.Abstractions;

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