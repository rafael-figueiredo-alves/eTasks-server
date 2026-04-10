namespace eTasks_server.Models.DTOs.Auth.Responses
{
    /// <summary>
    /// Emntidade de resposta retornada quando solicitada alteração de senha, seja para recuperação ou atualização.
    /// </summary>
    public class PasswordResponse
    {
        /// <summary>
        /// Indica se a operação de alteração de senha foi bem-sucedida ou não.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Mensagem detalhando o resultado da operação de alteração de senha.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
