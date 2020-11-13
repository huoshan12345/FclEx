using System;
using System.Net;
using Dawn;
using FclEx;
using FclEx.Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Configuration;
using Serilog.Extensions.Logging;

namespace Serilog.Sinks.Email
{
    public class EmailLogSettings
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
        public int PeriodSenconds { get; set; } = 10;
        public string OutputTemplate { get; set; } = AbpSerilogConstants.DefaultOutputTemplate;
    }

    public static class EmailLogSettingsExtensions
    {
        public static bool IsValid(this EmailLogSettings? settings)
        {
            return settings != null
                   && settings.MailServer.IsValid()
                   && settings.Port != 0
                   && settings.UserName.IsValid()
                   && settings.Password.IsValid()
                   && settings.ToEmails.IsValid();
        }

        public static LoggerConfiguration Email(this LoggerSinkConfiguration loggerConfiguration,
            EmailLogSettings settings)
        {
            Guard.Argument(settings, nameof(settings)).NotNull();

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
                ToEmail = settings.ToEmails.Touch().JoinWith(",")
            };
            return loggerConfiguration.Email(
                connectionInfo: info,
                outputTemplate: settings.OutputTemplate,
                restrictedToMinimumLevel: LevelConvert.ToSerilogLevel(settings.LogLevel),
                batchPostingLimit: settings.BatchPostingLimit,
                period: TimeSpan.FromSeconds(settings.PeriodSenconds)
            );
        }

    }
}