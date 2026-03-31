using System;
using System.Threading.Tasks;

namespace eTasks_server.Client.Auth
{
    public interface IAuthToken
    {
        Task Login(string Token);
        Task Logout();
        Task<string> GetUsername();
        Task<string> GetUserID();
        Task<string> GetEmail();
        Task<string> GetRole();
        Task<DateTime> GetExpiration();
        Task<string> GetUserAgent();
    }
}
