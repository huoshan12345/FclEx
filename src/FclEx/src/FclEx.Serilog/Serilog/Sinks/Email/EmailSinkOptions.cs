using Microsoft.Extensions.Logging;

namespace Serilog.Sinks.Email;

public class EmailSinkOptions
{
    public string? MailServer { get; set; }
    public int Port { get; set; } = 25;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string[]? ToEmails { get; set; }
    public string EmailSubject { get; set; } = "Log Email [{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] [{SourceContext}]";
    public bool EnableSsl { get; set; } = false;
    public bool IsBodyHtml { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Error;
    public int BatchPostingLimit { get; set; } = 2;
    public int PeriodSeconds { get; set; } = 10;
    public string OutputTemplate { get; set; } = FclEx.Serilog.Constants.DefaultOutputTemplate;
}