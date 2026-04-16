using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class ManageBonusAchievements : ComponentBase
    {
        [Inject] private IBonusAdminService BonusAdminService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;

        private List<BonusAchievementDTO> _achievements = new();
        private bool _isLoading = true;
        private bool _isProcessing = false;
        private string _searchString = string.Empty;

        // Dialog state
        private bool _isCreateDialogOpen;
        private MudForm? _form;
        private bool _isFormValid;
        private BonusAchievementRequest _createRequest = new();

        private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _isLoading = true;
                _achievements = await BonusAdminService.GetAchievementsAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar conquistas: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void OpenCreateDialog()
        {
            _createRequest = new BonusAchievementRequest();
            _isCreateDialogOpen = true;
        }

        private void CloseCreateDialog()
        {
            _isCreateDialogOpen = false;
        }

        private async Task SubmitCreateAchievement()
        {
            await _form!.ValidateAsync();
            if (!_isFormValid) return;

            try
            {
                _isProcessing = true;
                await BonusAdminService.CreateAchievementAsync(_createRequest);
                Snackbar.Add("Conquista criada com sucesso!", Severity.Success);
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

        private void StartedEditingItem(BonusAchievementDTO item)
        {
            // Opcional
        }

        private void CanceledEditingItem(BonusAchievementDTO item)
        {
            // Opcional
        }

        private async Task<DataGridEditFormAction> CommittedItemChanges(BonusAchievementDTO item)
        {
            try
            {
                var updateRequest = new BonusAchievementRequest
                {
                    Code = item.Code,
                    Name = item.Name,
                    Description = item.Description,
                    PointsRequired = item.PointsRequired,
                    DisplayType = item.DisplayType,
                    IsActive = item.IsActive
                };

                await BonusAdminService.UpdateAchievementAsync(item.Id, updateRequest);
                Snackbar.Add("Conquista atualizada com sucesso!", Severity.Success);
                return DataGridEditFormAction.Close;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao atualizar: {ex.Message}", Severity.Error);
                await LoadData(); // Reverte alterações locais
                return DataGridEditFormAction.KeepOpen;
            }
        }

        private async Task DeleteAchievement(BonusAchievementDTO item)
        {
            var confirm = await DialogService.ShowMessageBoxAsync(
                "Remover Conquista",
                $"Deseja realmente remover a conquista '{item.Name}'?",
                yesText: "Sim, remover", cancelText: "Cancelar");

            if (confirm != true) return;

            try
            {
                await BonusAdminService.DeleteAchievementAsync(item.Id);
                Snackbar.Add("Conquista removida.", Severity.Success);
                await LoadData();
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
