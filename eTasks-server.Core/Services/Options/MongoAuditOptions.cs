namespace eTasks_server.Core.Services.Options
{
    /// <summary>
    /// Representa as configurações de auditoria do MongoDB para o serviço de auditoria.
    /// </summary>
    public class MongoAuditOptions
    {
        /// <summary>
        /// Indica se a auditoria do MongoDB está habilitada ou não.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Obtém ou define a string de conexão do MongoDB.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o nome do banco de dados do MongoDB.
        /// </summary>
        public string DatabaseName { get; set; } = "etasks_server";

        /// <summary>
        /// Obtém ou define o nome da coleção do MongoDB.
        /// </summary>
        public string CollectionName { get; set; } = "operation_audit_logs";
    }
}
