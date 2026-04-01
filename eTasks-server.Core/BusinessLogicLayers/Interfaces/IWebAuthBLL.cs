using eTasks_server.Models.Auth;
using Microsoft.AspNetCore.Http;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IWebAuthBLL
    {
        Task LoginAsync(HttpContext httpContext, WebLoginRequest request, string? ipAddress);
        Task LogoutAsync(HttpContext httpContext);
    }
}
