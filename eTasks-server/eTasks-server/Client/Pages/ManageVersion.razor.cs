using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Entities.Version;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageVersionBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IVersionService VersionService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        #region Variáveis
        protected eTasksVersion _model = new();
        protected bool isLoading = true;
        protected MudForm? _form;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            await LoadVersion();
        }

        /// <summary>
        /// Método para ler os dados da versão do app cliente
        /// </summary>
        /// <returns></returns>
        private async Task LoadVersion()
        {
            isLoading = true;
            try
            {
                _model = await VersionService.GetVersionAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Oops! Ocorreu um erro ao carregar a versão: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Salva alterações feitas na versão do aplicativo
        /// </summary>
        /// <returns></returns>
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
                Snackbar.Add("Versão salva com sucesso!", Severity.Success);
                GoBack();
            }
            else
            {
                Snackbar.Add("Erro ao salvar a versão. Tente novamente.", Severity.Error);
            }
        }

        protected void GoBack() => Navigation.NavigateTo("/version");
        #endregion
    }
}
