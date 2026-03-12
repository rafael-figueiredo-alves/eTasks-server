using eTasks_server.Models.Exceptions;
using MudBlazor;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace eTasks_server.Client.Services
{
    public abstract class BaseService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;

        protected BaseService(HttpClient httpClient, IDialogService dialogService)
        {
            _httpClient    = httpClient;
            _dialogService = dialogService;
        }

        protected async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
            }
        }

        protected async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponseAsync<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                return await HandleResponseAsync<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<bool> PostAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                await HandleResponseAsync(response);
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                return await HandleResponseAsync<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                await HandleResponseAsync(response);
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
                var response = await _httpClient.SendAsync(request);
                return await HandleResponseAsync<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<bool> PatchAsync(string endpoint, object data)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
                var response = await _httpClient.SendAsync(request);
                await HandleResponseAsync(response);
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                await HandleResponseAsync(response);
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<HttpResponseMessage> OptionsAsync(string endpoint)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
                return await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }
    }
}
