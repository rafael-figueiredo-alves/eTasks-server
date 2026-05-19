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
            AccountConfirmation,
            AccountReactivation
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

        public async Task SendAccountReactivationEmailAsync(string toEmail, string reactivationLink, DateTime expiresAt)
        {
            var settings = await _settingsProvider.GetCurrentAsync();
            if (!settings.SmtpEnabled)
            {
                _logger.LogInformation("Servico de e-mail desativado. Evitando envio de reativacao de conta para {Email}", toEmail);
                return;
            }

            try
            {
                var emailConfig = MapEmailConfiguration(settings);
                using var smtpClient = CreateSmtpClient(emailConfig);
                using var mailMessage = await GetTemplateMailMessage(
                    EmailTemplate.AccountReactivation,
                    new Dictionary<string, string>
                    {
                        ["{{reactivationLink}}"] = reactivationLink,
                        ["{{expiresAt}}"] = expiresAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                    },
                    emailConfig);
                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Efetuando envio de e-mail de reativacao de conta para {Email}", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-mail de reativacao de conta para {Email} disparado com sucesso.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha critica ao tentar enviar e-mail de reativacao de conta para {Email}", toEmail);
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
            string fieldToReplace;

            switch (emailTemplate)
            {
                case EmailTemplate.PasswordReset:
                    fieldToReplace = "{{resetCode}}";
                    break;
                case EmailTemplate.AccountConfirmation:
                    fieldToReplace = "{{confirmationLink}}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(emailTemplate));
            }

            return await GetTemplateMailMessage(
                emailTemplate,
                new Dictionary<string, string> { [fieldToReplace] = content },
                emailConfiguration);
        }

        private async Task<MailMessage> GetTemplateMailMessage(EmailTemplate emailTemplate, IReadOnlyDictionary<string, string> replacements, EmailConfiguration emailConfiguration)
        {
            string templateFilename;
            string emailSubject;

            switch (emailTemplate)
            {
                case EmailTemplate.PasswordReset:
                    templateFilename = "password-reset.html";
                    emailSubject = "Seu codigo de recuperacao chegou! (eTasks)";
                    break;
                case EmailTemplate.AccountConfirmation:
                    templateFilename = "account-confirmation.html";
                    emailSubject = "Confirme sua conta no eTasks";
                    break;
                case EmailTemplate.AccountReactivation:
                    templateFilename = "account-reactivation.html";
                    emailSubject = "Recebemos sua solicitacao de exclusao no eTasks";
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

            htmlBody = htmlBody.Replace("{{logoUrl}}", logoUrl)
                .Replace("{{appName}}", appName)
                .Replace("{{year}}", year);

            foreach (var replacement in replacements)
            {
                htmlBody = htmlBody.Replace(replacement.Key, replacement.Value);
            }

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
