namespace eTasks_server.Client.Auth
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class ClientAuthorizeAttribute : Attribute
    {
        public string? Roles { get; init; }
    }
}
