namespace FclEx.Extensions;

public static class TextWriterExtensions
{
    public static IDisposable SetSelfLog(this TextWriter tw)
    {
        SelfLog.Enable(tw);
        return Disposable.Create(SelfLog.Disable);
    }
}