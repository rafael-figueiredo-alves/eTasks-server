using eTasks_server.Models.DTOs.Auth.Requests;
using Microsoft.AspNetCore.Http;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do login web
    /// </summary>
    public interface IWebAuthBLL
    {
        /// <summary>
        /// Efetua o login
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="request"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        Task LoginAsync(HttpContext httpContext, WebLoginRequest request, string? ipAddress);

        /// <summary>
        /// Registra uma nova conta Adm
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        Task RegisterAdminAsync(WebAdminRegisterRequest request, string? ipAddress);

        /// <summary>
        /// efetua o logout
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task LogoutAsync(HttpContext httpContext);
    }
}
