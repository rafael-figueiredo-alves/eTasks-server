using System.Net;
using System.Net.Mail;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.Services
{
    public class EmailService : IEmailService
    {
        private enum EmailTemplate
        {
            PasswordReset,
            AccountConfirmation
        }

        private readonly IConfiguration _configuration;
        private readonly IServerSettingsProvider _settingsProvider;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IServerSettingsProvider settingsProvider,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            var settings = await _settingsProvider.GetCurrentAsync();
            if (!settings.SmtpEnabled)
            {
                _logger.LogInformation("Servico de e-mail desativado. Evitando envio para {Email}.", toEmail);
                return;
            }

            try
            {
                var emailConfig = MapEmailConfiguration(settings);
                using var smtpClient = CreateSmtpClient(emailConfig);
                using var mailMessage = await GetTemplateMailMessage(EmailTemplate.PasswordReset, resetCode, emailConfig);
                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Efetuando envio de e-mail SMTP para {Email}", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-mail para {Email} disparado com sucesso.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha critica ao tentar enviar e-mail de recuperacao para {Email}", toEmail);
            }
        }

        public async Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            var settings = await _settingsProvider.GetCurrentAsync();
            if (!settings.SmtpEnabled)
            {
                _logger.LogInformation("Servico de e-mail desativado. Evitando envio de confirmacao para {Email}", toEmail);
                return;
            }

            try
            {
                var emailConfig = MapEmailConfiguration(settings);
                using var smtpClient = CreateSmtpClient(emailConfig);
                using var mailMessage = await GetTemplateMailMessage(EmailTemplate.AccountConfirmation, confirmationLink, emailConfig);
                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Efetuando envio de e-mail de confirmacao para {Email}", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-mail de confirmacao para {Email} disparado com sucesso.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha critica ao tentar enviar e-mail de confirmacao para {Email}", toEmail);
            }
        }

        private static EmailConfiguration MapEmailConfiguration(eTasks_server.Models.Entities.Settings.ServerSettings settings)
        {
            return new EmailConfiguration
            {
                host = settings.SmtpHost,
                port = settings.SmtpPort,
                enableSsl = settings.SmtpEnableSsl,
                username = settings.SmtpUsername,
                password = settings.SmtpPassword,
                fromEmail = settings.SmtpFromEmail,
                fromName = settings.SmtpFromName
            };
        }

        private static SmtpClient CreateSmtpClient(EmailConfiguration emailConfig)
        {
            return new SmtpClient(emailConfig.host, emailConfig.port)
            {
                Credentials = new NetworkCredential(emailConfig.username, emailConfig.password),
                EnableSsl = emailConfig.enableSsl
            };
        }

        private async Task<MailMessage> GetTemplateMailMessage(EmailTemplate emailTemplate, string content, EmailConfiguration emailConfiguration)
        {
            string templateFilename;
            string emailSubject;
            string fieldToReplace;

            switch (emailTemplate)
            {
                case EmailTemplate.PasswordReset:
                    templateFilename = "password-reset.html";
                    emailSubject = "Seu codigo de recuperacao chegou! (eTasks)";
                    fieldToReplace = "{{resetCode}}";
                    break;
                case EmailTemplate.AccountConfirmation:
                    templateFilename = "account-confirmation.html";
                    emailSubject = "Confirme sua conta no eTasks";
                    fieldToReplace = "{{confirmationLink}}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailTemplate));
            }

            var appName = "eTasks";
            var year = DateTime.UtcNow.Year.ToString();
            var baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
            var logoUrl = $"{baseUrl.TrimEnd('/')}/eTasks2.webp";
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "emails", templateFilename);
            var htmlBody = await File.ReadAllTextAsync(templatePath);

            htmlBody = htmlBody.Replace(fieldToReplace, content)
                .Replace("{{logoUrl}}", logoUrl)
                .Replace("{{appName}}", appName)
                .Replace("{{year}}", year);

            return new MailMessage
            {
                From = new MailAddress(emailConfiguration.fromEmail, emailConfiguration.fromName),
                Subject = emailSubject,
                Body = htmlBody,
                IsBodyHtml = true
            };
        }
    }
}
