namespace eTasks_server.Models.Exceptions
{
    /// <summary>
    /// Classe de exceção personalizada para erros de validação.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Dicionário que contém os erros de validação, onde a chave é o nome da propriedade e o valor é um array de mensagens de erro associadas a essa propriedade.
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with the specified validation errors.
        /// </summary>
        /// <param name="errors">A dictionary containing validation errors, where each key is the name of a field and the value is an array
        /// of error messages associated with that field. Cannot be null.</param>
        public ValidationException(IDictionary<string, string[]> errors)
            : base("Um ou mais erros de validação ocorreram:")
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified property name and error
        /// message.
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation. Cannot be null or empty.</param>
        /// <param name="errorMessage">The error message that describes the validation failure. Cannot be null or empty.</param>
        public ValidationException(string propertyName, string errorMessage)
            : base("Um ou mais erros de validação ocorreram:")
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMessage } }
            };
        }
    }
}
