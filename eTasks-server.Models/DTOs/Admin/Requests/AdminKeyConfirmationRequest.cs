namespace eTasks_server.Models.DTOs.Admin.Requests
{
    /// <summary>
    /// Classe DTO que representa a solicitação de confirmação da chave de administrador.
    /// </summary>
    public class AdminKeyConfirmationRequest
    {
        /// <summary>
        /// Chave de administrador a ser confirmada.
        /// </summary>
        public string AdminKey { get; set; } = string.Empty;
    }
}
