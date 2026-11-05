using Serilog.Debugging;

namespace FclEx.Serilog.Extensions;

public class TextWriterExtensionsTests
{
    [Fact(DisableParallelization = true)]
    public void SetSelfLog_HandleDisposal_DisablesDiagnosticsWithoutDisposingWriter()
    {
        using var output = new StringWriter();
        using var handle = output.SetSelfLog();
        SelfLog.WriteLine("captured diagnostic");

        handle.Dispose();
        SelfLog.WriteLine("disabled diagnostic");
        output.Write("writer still usable");

        Assert.Contains("captured diagnostic", output.ToString());
        Assert.DoesNotContain("disabled diagnostic", output.ToString());
        Assert.Contains("writer still usable", output.ToString());
    }

    [Fact(DisableParallelization = true)]
    public void SetSelfLog_NestedHandle_DoesNotRestorePreviousDestination()
    {
        using var first = new StringWriter();
        using var second = new StringWriter();
        using var firstHandle = first.SetSelfLog();
        SelfLog.WriteLine("first diagnostic");
        using (second.SetSelfLog())
            SelfLog.WriteLine("second diagnostic");

        SelfLog.WriteLine("after nested disposal");

        Assert.Contains("first diagnostic", first.ToString());
        Assert.DoesNotContain("second diagnostic", first.ToString());
        Assert.Contains("second diagnostic", second.ToString());
        Assert.DoesNotContain("after nested disposal", first.ToString() + second.ToString());
    }
}
