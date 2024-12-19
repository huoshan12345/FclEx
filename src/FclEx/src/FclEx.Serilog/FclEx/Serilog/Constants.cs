namespace FclEx.Serilog;

public static class Constants
{
    public const string DefaultOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] [{SourceContext}] {Message:l}{NewLine}{Exception}";
    public const string SourceContextPropertyName = global::Serilog.Core.Constants.SourceContextPropertyName;
}