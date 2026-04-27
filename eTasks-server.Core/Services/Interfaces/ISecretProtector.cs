namespace eTasks_server.Core.Services.Interfaces
{
    public interface ISecretProtector
    {
        string Protect(string value);
        string Unprotect(string value);
    }
}
