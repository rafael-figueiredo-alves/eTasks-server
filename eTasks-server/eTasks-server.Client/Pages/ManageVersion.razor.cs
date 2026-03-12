using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Version;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageVersionBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IVersionService VersionService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        protected eTasksVersion _model = new eTasksVersion();
        protected bool isLoading = true;
        protected MudForm? _form;        

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _model = await VersionService.GetVersionAsync();
                isLoading = false;
            }
            catch (Exception ex)
            {
                // Trate o erro, por exemplo, exibindo uma mensagem de erro para o usuário
                Console.Error.WriteLine($"Erro ao carregar a versão: {ex.Message}");
                //Log.LogError(ex, "Erro ao carregar a versão");
                await DialogService.ShowMessageBoxAsync("Oops! Ocorreu um erro", $"Erro ao carregar a versão: {ex.Message}", yesText: "OK");
                isLoading = false;
                // Você pode usar um Snackbar ou outro componente para mostrar a mensagem de erro
            }
        }
        protected async Task Save()
        {
            await _form.ValidateAsync();
            if (_form.IsValid)
            {
                // Para agora, apenas log ou simule salvar
                if (await VersionService.SaveVersionAsync(_model))
                {
                    Snackbar.Add("Versão salva com sucesso!", Severity.Success);
                    Navigation.NavigateTo("/version");
                }
                else
                {
                    Snackbar.Add("Erro ao salvar a versão. Tente novamente.", Severity.Error);
                }
                // No futuro, salve em arquivo TXT ou similar

            }
        }
        protected void GoBack() => Navigation.NavigateTo("/version");
    }
}
