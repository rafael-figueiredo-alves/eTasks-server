namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Retorno da ação de exclusão de lista de compras
    /// </summary>
    public class DeletedShoppingListResponse
    {
        /// <summary>
        /// Identificador da exclusão
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data/Hora da ação da exclusão
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
