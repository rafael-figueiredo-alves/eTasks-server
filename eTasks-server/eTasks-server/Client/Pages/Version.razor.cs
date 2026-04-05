using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Entities.Version;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class VersionBase : ComponentBase
    {
        protected eTasksVersion _model = new();
        protected bool isLoading = true;
        protected string? errorMessage;

        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private IVersionService VersionService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            isLoading = true;
            try
            {
                _model = await VersionService.GetVersionAsync();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Console.Error.WriteLine($"Erro ao carregar a versao: {ex.Message}");
                Snackbar.Add($"Oops! Ocorreu um erro ao carregar a versao: {ex.Message}", Severity.Error);
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
