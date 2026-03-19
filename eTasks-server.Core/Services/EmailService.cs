using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;

namespace eTasks_server.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

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
                string host = _configuration[Constants.SmtpHost]!;
                int port = int.Parse(_configuration[Constants.SmtpPort] ?? "587");
                bool enableSsl = bool.TryParse(_configuration[Constants.SmtpEnableSsl], out var ssl) ? ssl : true;
                string username = _configuration[Constants.SmtpUsername]!;
                string password = _configuration[Constants.SmtpPassword]!;
                string fromEmail = _configuration[Constants.SmtpFromEmail]!;
                string fromName = _configuration[Constants.SmtpFromName]!;

                string appName = "eTasks";
                string year = DateTime.UtcNow.Year.ToString();
                string baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
                string logoUrl = $"{baseUrl.TrimEnd('/')}/logo.png"; // Placeholder image

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "emails", "password-reset.html");
                string htmlBody = await File.ReadAllTextAsync(templatePath);

                htmlBody = htmlBody.Replace("{{resetCode}}", resetCode)
                                   .Replace("{{logoUrl}}", logoUrl)
                                   .Replace("{{appName}}", appName)
                                   .Replace("{{year}}", year);

                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Seu código de recuperação chegou! (eTasks)",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

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

        public async Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            var isEnabled = bool.TryParse(_configuration[Constants.SmtpEnabled], out var enabled) && enabled;
            if (!isEnabled)
            {
                _logger.LogInformation("Serviço de e-mail desativado. Evitando envio de confirmação de conta para {Email}", toEmail);
                return;
            }

            try
            {
                string host = _configuration[Constants.SmtpHost]!;
                int port = int.Parse(_configuration[Constants.SmtpPort] ?? "587");
                bool enableSsl = bool.TryParse(_configuration[Constants.SmtpEnableSsl], out var ssl) ? ssl : true;
                string username = _configuration[Constants.SmtpUsername]!;
                string password = _configuration[Constants.SmtpPassword]!;
                string fromEmail = _configuration[Constants.SmtpFromEmail]!;
                string fromName = _configuration[Constants.SmtpFromName]!;

                string appName = "eTasks";
                string year = DateTime.UtcNow.Year.ToString();
                string baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
                string logoUrl = $"{baseUrl.TrimEnd('/')}/logo.png";

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "emails", "account-confirmation.html");
                string htmlBody = await File.ReadAllTextAsync(templatePath);

                htmlBody = htmlBody.Replace("{{confirmationLink}}", confirmationLink)
                                   .Replace("{{logoUrl}}", logoUrl)
                                   .Replace("{{appName}}", appName)
                                   .Replace("{{year}}", year);

                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Confirme sua conta no eTasks",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

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
