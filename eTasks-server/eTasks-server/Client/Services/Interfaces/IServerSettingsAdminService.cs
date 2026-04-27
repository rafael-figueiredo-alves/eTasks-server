using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IServerSettingsAdminService
    {
        Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default);
        Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
        Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
        Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
        Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
    }
}
