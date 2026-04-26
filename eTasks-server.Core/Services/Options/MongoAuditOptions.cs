namespace eTasks_server.Core.Services.Options
{
    public class MongoAuditOptions
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "etasks_server";
        public string CollectionName { get; set; } = "operation_audit_logs";
    }
}
