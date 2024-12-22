namespace FclEx.Extensions;

public static class ProcessExtensions
{
    public static async Task<string> GetOutput(this Process process)
    {
        var queue = new ConcurrentQueue<string?>();
        process.OutputDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.ErrorDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

#if NETSTANDARD2_0
        await Task.Yield();
        process.WaitForExit();
#else
        await process.WaitForExitAsync();
#endif
        var output = queue.Where(m => m is not null).JoinWith(Environment.NewLine);

        if (process.ExitCode != 0)
            throw new ProcessException(process.ExitCode, output);

        return output;
    }
}