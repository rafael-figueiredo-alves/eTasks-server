using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Entities.Version;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageVersionBase : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IVersionService VersionService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected eTasksVersion _model = new();
        protected bool isLoading = true;
        protected MudForm? _form;

        protected override async Task OnInitializedAsync()
        {
            await LoadVersion();
        }

        private async Task LoadVersion()
        {
            isLoading = true;
            try
            {
                _model = await VersionService.GetVersionAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao carregar a versao: {ex.Message}");
                Snackbar.Add($"Oops! Ocorreu um erro ao carregar a versao: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task Save()
        {
            if (_form is null)
            {
                return;
            }

            await _form.ValidateAsync();
            if (!_form.IsValid)
            {
                return;
            }

            if (await VersionService.SaveVersionAsync(_model))
            {
                Snackbar.Add("Versao salva com sucesso!", Severity.Success);
                Navigation.NavigateTo("/version");
            }
            else
            {
                Snackbar.Add("Erro ao salvar a versao. Tente novamente.", Severity.Error);
            }
        }

        protected void GoBack() => Navigation.NavigateTo("/version");
    }
}
