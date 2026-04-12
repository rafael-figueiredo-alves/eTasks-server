namespace eTasks_server.Models.Entities.Shopping
{
    /// <summary>
    /// Define o tipo principal da lista de compras.
    /// </summary>
    public enum ShoppingListType
    {
        /// <summary>
        /// Lista de mercado.
        /// </summary>
        Grocery = 0,
        /// <summary>
        /// Lista de hortifruti.
        /// </summary>
        Produce = 1,
        /// <summary>
        /// Lista para festa.
        /// </summary>
        Party = 2,
        /// <summary>
        /// Lista para aniversario.
        /// </summary>
        Birthday = 3,
        /// <summary>
        /// Lista de moveis.
        /// </summary>
        Furniture = 4,
        /// <summary>
        /// Lista de farmacia.
        /// </summary>
        Pharmacy = 5,
        /// <summary>
        /// Lista de papelaria.
        /// </summary>
        Stationery = 6,
        /// <summary>
        /// Lista de material de limpeza.
        /// </summary>
        Cleaning = 7,
        /// <summary>
        /// Outro tipo de lista.
        /// </summary>
        Other = 8
    }
}
