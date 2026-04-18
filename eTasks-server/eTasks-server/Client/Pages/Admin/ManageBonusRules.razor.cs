using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class ManageBonusRules : ComponentBase
    {
        [Inject] private IBonusAdminService BonusAdminService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;

        private List<BonusPointRuleDTO> _rules = new();
        private bool _isLoading = true;
        private bool _isProcessing = false;
        private string _searchString = string.Empty;

        // Dialog state
        private bool _isCreateDialogOpen;
        private MudForm? _form;
        private bool _isFormValid;
        private BonusPointRuleCreateRequest _createRequest = new();

        private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };

        protected void CommittedItemChangesHandler(BonusPointRuleDTO item)
        {
            _ = CommittedItemChanges(item);
        }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                _rules = await BonusAdminService.GetRulesAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar regras: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void OpenCreateDialog()
        {
            _createRequest = new BonusPointRuleCreateRequest();
            _isCreateDialogOpen = true;
        }

        private void CloseCreateDialog()
        {
            _isCreateDialogOpen = false;
        }

        private async Task SubmitCreateRule()
        {
            await _form!.ValidateAsync();
            if (!_isFormValid) return;

            try
            {
                _isProcessing = true;
                await BonusAdminService.CreateRuleAsync(_createRequest);
                Snackbar.Add("Regra criada com sucesso!", Severity.Success);
                CloseCreateDialog();
                await LoadData();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private void StartedEditingItem(BonusPointRuleDTO item)
        {
            // Opcional: Ação ao iniciar edição
        }

        private void CanceledEditingItem(BonusPointRuleDTO item)
        {
            // Opcional: Ação ao cancelar
        }

        private async Task<DataGridEditFormAction> CommittedItemChanges(BonusPointRuleDTO item)
        {
            try
            {
                var updateRequest = new BonusPointRuleUpdateRequest
                {
                    Name = item.Name,
                    Description = item.Description,
                    DefaultPoints = item.DefaultPoints,
                    AllowCustomPoints = item.AllowCustomPoints,
                    IsActive = item.IsActive
                };

                await BonusAdminService.UpdateRuleAsync(item.Id, updateRequest);
                Snackbar.Add("Regra atualizada com sucesso!", Severity.Success);
                return DataGridEditFormAction.Close;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao atualizar: {ex.Message}", Severity.Error);
                await LoadData(); // Reverte alterações locais recarregando
                return DataGridEditFormAction.KeepOpen;
            }
        }

        private async Task ToggleRuleStatus(BonusPointRuleDTO item, bool newValue)
        {
            if (item.IsActive == newValue) return;
            
            try
            {
                _isProcessing = true;
                item.IsActive = newValue;

                var updateRequest = new BonusPointRuleUpdateRequest
                {
                    Name = item.Name,
                    Description = item.Description,
                    DefaultPoints = item.DefaultPoints,
                    AllowCustomPoints = item.AllowCustomPoints,
                    IsActive = item.IsActive
                };

                await BonusAdminService.UpdateRuleAsync(item.Id, updateRequest);
                Snackbar.Add($"Regra '{(item.IsActive ? "ativada" : "desativada")}' com sucesso!", Severity.Success);
            }
            catch (Exception ex)
            {
                item.IsActive = !newValue; // Reverte localmente em caso de erro
                Snackbar.Add($"Erro ao alterar status: {ex.Message}", Severity.Error);
                await LoadData(); // Sincroniza com o servidor
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task ToggleRuleCustomPoints(BonusPointRuleDTO item, bool newValue)
        {
            if (item.AllowCustomPoints == newValue) return;

            try
            {
                _isProcessing = true;
                item.AllowCustomPoints = newValue;

                var updateRequest = new BonusPointRuleUpdateRequest
                {
                    Name = item.Name,
                    Description = item.Description,
                    DefaultPoints = item.DefaultPoints,
                    AllowCustomPoints = item.AllowCustomPoints,
                    IsActive = item.IsActive
                };

                await BonusAdminService.UpdateRuleAsync(item.Id, updateRequest);
                Snackbar.Add("Permissão de pontos customizados atualizada!", Severity.Success);
            }
            catch (Exception ex)
            {
                item.AllowCustomPoints = !newValue;
                Snackbar.Add($"Erro ao atualizar regra: {ex.Message}", Severity.Error);
                await LoadData();
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task DeleteRule(BonusPointRuleDTO item)
        {
            var confirm = await DialogService.ShowMessageBoxAsync(
                "Remover Regra",
                $"Deseja realmente remover a regra '{item.Name}'?",
                yesText: "Sim, remover", cancelText: "Cancelar");

            if (confirm != true) return;

            try
            {
                await BonusAdminService.DeleteRuleAsync(item.Id);
                Snackbar.Add("Regra removida.", Severity.Success);
                await LoadData();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
