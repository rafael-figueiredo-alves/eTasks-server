namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface que define os métodos para proteger e desproteger segredos.
    /// </summary>
    public interface ISecretProtector
    {
        /// <summary>
        /// Protege um valor de segredo.
        /// </summary>
        /// <param name="value">Valor a ser protegido</param>
        /// <returns></returns>
        string Protect(string value);

        /// <summary>
        /// Desprotege um valor de segredo.
        /// </summary>
        /// <param name="value">Valor a ser desprotegido</param>
        /// <returns></returns>
        string Unprotect(string value);
    }
}
