using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Filtros de consulta para listas de compras.
    /// </summary>
    public class ListShoppingListsRequest
    {
        /// <summary>
        /// Indica se a consulta deve retornar apenas as listas de compras finalizadas. Se for nulo, retorna todas as listas.
        /// </summary>
        public bool? IsFinalized { get; set; }

        /// <summary>
        /// Tipo de lista de compras a ser filtrada. Se for nulo, retorna todas as listas independentemente do tipo.
        /// </summary>
        public ShoppingListType? Type { get; set; }
        
        /// <summary>
        /// Local onde a lista de compras será utilizada. Se for nulo, retorna todas as listas independentemente do local.
        /// </summary>
        public string? Place { get; set; }
        
        /// <summary>
        /// Termo de pesquisa para filtrar listas de compras pelo nome. Se for nulo, retorna todas as listas independentemente do nome.
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
