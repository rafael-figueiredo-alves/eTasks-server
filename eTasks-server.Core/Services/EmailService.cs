using System.Net;
using System.Net.Mail;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe do serviço de envio de e-mails, responsável por enviar e-mails de recuperação de senha, confirmação de conta e reativação de conta.
    /// </summary>
    public class EmailService : IEmailService
    {
        /// <summary>
        /// Enumeração que representa os modelos de e-mail disponíveis.
        /// </summary>
        private enum EmailTemplate
        {
            PasswordReset,
            AccountConfirmation,
            AccountReactivation
        }

        private readonly IConfiguration _configuration;
        private readonly IServerSettingsProvider _settingsProvider;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Construtor da classe EmailService.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="settingsProvider"></param>
        /// <param name="logger"></param>
        public EmailService(
            IConfiguration configuration,
            IServerSettingsProvider settingsProvider,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        /// <summary>
        /// Método responsável por enviar um e-mail de recuperação de senha para o endereço de e-mail especificado, utilizando o código de redefinição fornecido.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="resetCode"></param>
        /// <returns></returns>
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
                _logger.LogError(ex, "Falha crítica ao tentar enviar e-mail de recuperação para {Email}", toEmail);
            }
        }

        /// <summary>
        /// Método responsável por enviar um e-mail de confirmação de conta para o endereço de e-mail especificado, utilizando o link de confirmação fornecido.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="confirmationLink"></param>
        /// <returns></returns>
        public async Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            var settings = await _settingsProvider.GetCurrentAsync();
            if (!settings.SmtpEnabled)
            {
                _logger.LogInformation("Serviço de e-mail desativado. Evitando envio de confirmação para {Email}", toEmail);
                return;
            }

            try
            {
                var emailConfig = MapEmailConfiguration(settings);
                using var smtpClient = CreateSmtpClient(emailConfig);
                using var mailMessage = await GetTemplateMailMessage(EmailTemplate.AccountConfirmation, confirmationLink, emailConfig);
                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Efetuando envio de e-mail de confirmação para {Email}", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-mail de confirmação para {Email} disparado com sucesso.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao tentar enviar e-mail de confirmação para {Email}", toEmail);
            }
        }

        /// <summary>
        /// Método responsável por enviar um e-mail de reativação de conta para o endereço de e-mail especificado, utilizando o link de reativação fornecido e a data de expiração do código.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="reactivationLink"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        public async Task SendAccountReactivationEmailAsync(string toEmail, string reactivationLink, DateTime expiresAt)
        {
            var settings = await _settingsProvider.GetCurrentAsync();
            if (!settings.SmtpEnabled)
            {
                _logger.LogInformation("Serviço de e-mail desativado. Evitando envio de reativação de conta para {Email}", toEmail);
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

                _logger.LogInformation("Efetuando envio de e-mail de reativação de conta para {Email}", toEmail);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-mail de reativação de conta para {Email} disparado com sucesso.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao tentar enviar e-mail de reativação de conta para {Email}", toEmail);
            }
        }

        /// <summary>
        /// Mapeia as configurações do servidor para a configuração de e-mail.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Cria um cliente SMTP com base na configuração de e-mail fornecida.
        /// </summary>
        /// <param name="emailConfig"></param>
        /// <returns></returns>
        private static SmtpClient CreateSmtpClient(EmailConfiguration emailConfig)
        {
            return new SmtpClient(emailConfig.host, emailConfig.port)
            {
                Credentials = new NetworkCredential(emailConfig.username, emailConfig.password),
                EnableSsl = emailConfig.enableSsl
            };
        }

        /// <summary>
        /// Obtém a mensagem de e-mail com base no modelo de e-mail especificado, substituindo o conteúdo
        /// </summary>
        /// <param name="emailTemplate"></param>
        /// <param name="content"></param>
        /// <param name="emailConfiguration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// Obtém o template de e-mail com base no modelo de e-mail especificado, substituindo os campos fornecidos no dicionário de substituições.
        /// </summary>
        /// <param name="emailTemplate"></param>
        /// <param name="replacements"></param>
        /// <param name="emailConfiguration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async Task<MailMessage> GetTemplateMailMessage(EmailTemplate emailTemplate, IReadOnlyDictionary<string, string> replacements, EmailConfiguration emailConfiguration)
        {
            string templateFilename;
            string emailSubject;

            switch (emailTemplate)
            {
                case EmailTemplate.PasswordReset:
                    templateFilename = "password-reset.html";
                    emailSubject = "Seu código de recuperação chegou! (eTasks)";
                    break;
                case EmailTemplate.AccountConfirmation:
                    templateFilename = "account-confirmation.html";
                    emailSubject = "Confirme sua conta no eTasks";
                    break;
                case EmailTemplate.AccountReactivation:
                    templateFilename = "account-reactivation.html";
                    emailSubject = "Recebemos sua solicitação de exclusão no eTasks";
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
