using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Serviço de envio de e-mails utilizando SMTP, com suporte para templates HTML e configuração via appsettings.json.
    /// </summary>
    public class EmailService : IEmailService
    {
        private enum EmailTemplate
        {
            PasswordReset,
            AccountConfirmation
        }

        #region Campos Privados
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        #endregion

        /// <summary>
        /// Método construtor que recebe as dependências de configuração e logging via injeção de dependência.
        /// </summary>
        /// <param name="configuration">Serviço de configurações</param>
        /// <param name="logger">Serviço de log</param>
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        #region Private Helper Methods
        private bool IsSmtpEnabled()
        {
            return bool.TryParse(_configuration[Constants.SmtpEnabled], out var enabled) && enabled;
        }

        private EmailConfiguration GetEmailConfiguration()
        {
            return new EmailConfiguration
            {
                host = _configuration[Constants.SmtpHost]!,
                port = int.Parse(_configuration[Constants.SmtpPort] ?? "587"),
                enableSsl = bool.TryParse(_configuration[Constants.SmtpEnableSsl], out var ssl) ? ssl : true,
                username = _configuration[Constants.SmtpUsername]!,
                password = _configuration[Constants.SmtpPassword]!,
                fromEmail = _configuration[Constants.SmtpFromEmail]!,
                fromName = _configuration[Constants.SmtpFromName]!
            };
        }

        private SmtpClient CreateSmtpClient(EmailConfiguration emailConfig)
        {
            return new SmtpClient(emailConfig.host, emailConfig.port)
            {
                Credentials = new NetworkCredential(emailConfig.username, emailConfig.password),
                EnableSsl = emailConfig.enableSsl
            };
        }

        private async Task<MailMessage> GetTemplateMailMessage(EmailTemplate emailTemplate, string Content, EmailConfiguration emailConfiguration)
        {
            string TemplateFilename = string.Empty;
            string EmailSubject = string.Empty;
            string FieldToReplace = string.Empty;

            switch (emailTemplate)
            {
                case EmailTemplate.PasswordReset:
                    TemplateFilename = "password-reset.html";
                    EmailSubject = "Seu código de recuperação chegou! (eTasks)";
                    FieldToReplace = "{{resetCode}}";
                    break;
                case EmailTemplate.AccountConfirmation:
                    TemplateFilename = "account-confirmation.html";
                    EmailSubject = "Confirme sua conta no eTasks";
                    FieldToReplace = "{{confirmationLink}}";
                    break;
                default:
                    throw new ArgumentException("Tipo de template de e-mail desconhecido.");
            }

            string appName = "eTasks";
            string year = DateTime.UtcNow.Year.ToString();
            string baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
            string logoUrl = $"{baseUrl.TrimEnd('/')}/logo.png"; // Placeholder image

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "emails", TemplateFilename);
            string htmlBody = await File.ReadAllTextAsync(templatePath);

            htmlBody = htmlBody.Replace(FieldToReplace, Content)
               .Replace("{{logoUrl}}", logoUrl)
               .Replace("{{appName}}", appName)
               .Replace("{{year}}", year);

            return new MailMessage()
            {
                From = new MailAddress(emailConfiguration.fromEmail, emailConfiguration.fromName),
                Subject = EmailSubject,
                Body = htmlBody,
                IsBodyHtml = true
            };           
        }
        #endregion

        /// <summary>
        /// Método para enviar e-mail de recuperação de senha, utilizando um template HTML localizado em wwwroot/templates/emails/password-reset.html.
        /// </summary>
        /// <param name="toEmail">Endereço a enviar</param>
        /// <param name="resetCode">Código de redefinição</param>
        /// <returns></returns>
        public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            var isEnabled = bool.TryParse(_configuration[Constants.SmtpEnabled], out var enabled) && enabled;
            
            if (!isEnabled)
            {
                _logger.LogInformation("Serviço de e-mail desativado no Smtp:Enabled. Evitando envio para {Email} com código {Code}", toEmail, resetCode);
                return;
            }

            try
            {
                EmailConfiguration emailConfig = GetEmailConfiguration();

                var smtpClient = CreateSmtpClient(emailConfig);

                MailMessage mailMessage = await GetTemplateMailMessage(EmailTemplate.PasswordReset, resetCode, emailConfig);
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
        /// Método para enviar e-mail de confirmação de conta, utilizando um template HTML localizado em wwwroot/templates/emails/account-confirmation.html.
        /// </summary>
        /// <param name="toEmail">Endereço a enviar email</param>
        /// <param name="confirmationLink">Link de confirmação</param>
        /// <returns></returns>
        public async Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            if (!IsSmtpEnabled())
            {
                _logger.LogInformation("Serviço de e-mail desativado. Evitando envio de confirmação de conta para {Email}", toEmail);
                return;
            }

            try
            {
                EmailConfiguration emailConfig = GetEmailConfiguration();


                var smtpClient = CreateSmtpClient(emailConfig);

                var mailMessage = await GetTemplateMailMessage(EmailTemplate.AccountConfirmation, confirmationLink, emailConfig);
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
    }
}
