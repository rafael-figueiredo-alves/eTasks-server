namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Serviço de envio de e-mails para a aplicação eTasks.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia um e-mail de redefinição de senha para o usuário, contendo um código de redefinição.
        /// </summary>
        /// <param name="toEmail">Endereço para enviar email de redefinição de senha</param>
        /// <param name="resetCode">Código para redefinição de senha</param>
        /// <returns>Nada</returns>
        Task SendPasswordResetEmailAsync(string toEmail, string resetCode);

        /// <summary>
        /// Envia um e-mail de confirmação de conta para o usuário, contendo um link para confirmar a conta.
        /// </summary>
        /// <param name="toEmail">Endereço para enviar email de confirmação de conta</param>
        /// <param name="confirmationLink">Link para confirmar conta</param>
        /// <returns>Nada</returns>
        Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink);
    }
}
