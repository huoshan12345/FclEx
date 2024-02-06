namespace Serilog.Sinks.Email;

public static class EmailSinkOptionsExtensions
{
    public static bool IsValid([NotNullWhen(true)] this EmailSinkOptions? settings)
    {
        return settings != null
               && settings.MailServer.IsValid()
               && settings.Port != 0
               && settings.UserName.IsValid()
               && settings.Password.IsValid()
               && settings.ToEmails.IsValid();
    }

    public static LoggerConfiguration Email(this LoggerSinkConfiguration loggerConfiguration, EmailSinkOptions settings)
    {
        Check.NotNull(settings);

        var info = new EmailConnectionInfo
        {
            EmailSubject = settings.EmailSubject,
            EnableSsl = settings.EnableSsl,
            FromEmail = (settings.FromEmail, settings.UserName).FirstValid(),
            IsBodyHtml = settings.IsBodyHtml,
            MailServer = settings.MailServer,
            NetworkCredentials = new NetworkCredential(settings.UserName, settings.Password),
            Port = settings.Port,
            ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true,
            ToEmail = settings.ToEmails.EmptyIfNull().JoinWith(",")
        };
        return loggerConfiguration.Email(
            connectionInfo: info,
            outputTemplate: settings.OutputTemplate,
            restrictedToMinimumLevel: LevelConvert.ToSerilogLevel(settings.LogLevel),
            batchPostingLimit: settings.BatchPostingLimit,
            period: TimeSpan.FromSeconds(settings.PeriodSeconds)
        );
    }

}