using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;

namespace eTasks_server.Client.Services
{
    public class ServerSettingsAdminService(IServerSettingsAdminBLL bll) : IServerSettingsAdminService
    {
        public Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default)
            => bll.GetAsync(cancellationToken);

        public Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
            => bll.UpdateAsync(request, cancellationToken);

        public Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
            => bll.TestEmailAsync(request, cancellationToken);

        public Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
            => bll.TestOpenRouterAsync(request, cancellationToken);

        public Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
            => bll.TestMongoAsync(request, cancellationToken);
    }
}
