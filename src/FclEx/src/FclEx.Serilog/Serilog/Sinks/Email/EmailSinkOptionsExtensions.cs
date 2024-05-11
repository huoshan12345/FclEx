using Serilog.Formatting.Display;

namespace Serilog.Sinks.Email;

public static class EmailSinkOptionsExtensions
{
    public static LoggerConfiguration Email(this LoggerSinkConfiguration loggerConfiguration, FclEx.Serilog.EmailSinkOptions settings)
    {
        Check.NotNull(settings);

        var info = new EmailSinkOptions()
        {
            Subject = new MessageTemplateTextFormatter(settings.SubjectTemplate),
            Body = new MessageTemplateTextFormatter(settings.BodyTemplate),
            ConnectionSecurity = settings.ConnectionSecurity,
            From = (settings.From, settings.UserName).FirstNotEmpty(),
            To = settings.To.EmptyIfNull().ToList(),
            IsBodyHtml = settings.IsBodyHtml,
            Host = settings.Host.IfEmpty(""),
            Credentials = new NetworkCredential(settings.UserName, settings.Password),
            Port = settings.Port,
            ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true,
        };
        return loggerConfiguration.Email(info, new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = settings.BatchSizeLimit,
            Period = TimeSpan.FromSeconds(settings.PeriodSeconds),
        }, LevelConvert.ToSerilogLevel(settings.LogLevel));
    }

}