using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Version;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class VersionBase : ComponentBase
    {
        protected eTasksVersion _model = new eTasksVersion();
        protected bool isLoading = true;
        protected string? errorMessage;

        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private IVersionService VersionService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                isLoading = true;
                _model = await VersionService.GetVersionAsync();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Console.Error.WriteLine($"Erro ao carregar a versão: {ex.Message}");
                await DialogService.ShowMessageBoxAsync(
                    "Oops! Ocorreu um erro",
                    $"Erro ao carregar a versão: {ex.Message}",
                    yesText: "OK");
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void GoToEdit() => Navigation.NavigateTo("/version/edit");
        protected void GoBack() => Navigation.NavigateTo("/");
    }
}
