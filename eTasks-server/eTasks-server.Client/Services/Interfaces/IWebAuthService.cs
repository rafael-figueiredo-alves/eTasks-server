using eTasks_server.Models.Auth;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IWebAuthService
    {
        Task LoginAsync(WebLoginRequest request);
        Task LogoutAsync();
    }
}
