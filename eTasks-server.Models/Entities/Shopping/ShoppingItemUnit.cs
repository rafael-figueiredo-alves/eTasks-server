namespace eTasks_server.Models.Entities.Shopping
{
    /// <summary>
    /// Define a unidade usada no item de compra.
    /// </summary>
    public enum ShoppingItemUnit
    {
        /// <summary>
        /// Unidade individual.
        /// </summary>
        Unit = 0,
        /// <summary>
        /// Pacote.
        /// </summary>
        Package = 1,
        /// <summary>
        /// Quilograma.
        /// </summary>
        Kilogram = 2,
        /// <summary>
        /// Grama.
        /// </summary>
        Gram = 3,
        /// <summary>
        /// Litro.
        /// </summary>
        Liter = 4,
        /// <summary>
        /// Mililitro.
        /// </summary>
        Milliliter = 5,
        /// <summary>
        /// Centimetro.
        /// </summary>
        Centimeter = 6,
        /// <summary>
        /// Metro.
        /// </summary>
        Meter = 7,
        /// <summary>
        /// Caixa.
        /// </summary>
        Box = 8,
        /// <summary>
        /// Outro tipo de unidade.
        /// </summary>
        Other = 9
    }
}
