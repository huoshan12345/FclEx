using MailKit.Security;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public class EmailSinkOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 25;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }
    public string[]? To { get; set; }
    public string SubjectTemplate { get; set; } = "Log Email [{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] [{SourceContext}]";
    public SecureSocketOptions ConnectionSecurity { get; set; } = SecureSocketOptions.Auto;
    public bool IsBodyHtml { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Error;
    public int BatchSizeLimit { get; set; } = 2;
    public int PeriodSeconds { get; set; } = 10;
    public string BodyTemplate { get; set; } = Constants.DefaultOutputTemplate;
}

public static class EmailSinkOptionsExtensions
{
    public static bool IsValid([NotNullWhen(true)] this EmailSinkOptions? settings)
    {
        return settings != null
               && settings.Host.IsNotEmpty()
               && settings.Port != 0
               && settings.UserName.IsNotEmpty()
               && settings.Password.IsNotEmpty()
               && settings.To.IsNotEmpty();
    }
}