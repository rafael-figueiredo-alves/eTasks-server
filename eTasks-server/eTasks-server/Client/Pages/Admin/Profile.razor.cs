using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Extensions;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Security.Claims;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class Profile
    {
        #region Serviços Injetados
        [Inject] protected IUserProfileService ProfileService { get; set; } = default!;
        [Inject] protected UserState UserState { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        #endregion

        #region Parâmetros Cascateados
        [CascadingParameter]
        private Task<AuthenticationState> AuthState { get; set; } = default!;
        #endregion

        #region Variáveis
        private UserProfileResponse? _profile;
        private bool _loading = true;
        private bool _saving = false;
        private bool _success = false;
        private MudForm? _form;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            await LoadProfile();
        }

        private async Task LoadProfile()
        {
            _loading = true;
            try
            {
                var authState = await AuthState;
                var uidStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(uidStr, out var uid))
                {
                    _profile = await ProfileService.GetProfileAsync(uid);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar perfil: {ex.Message}", Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task SaveProfile()
        {
            if (_profile == null || _form == null) return;
            await _form.ValidateAsync();
            if (!_form.IsValid) return;

            _saving = true;
            try
            {
                var request = new UpdateUserProfileRequest
                {
                    Name = _profile.Name,
                    Email = _profile.Email
                };

                var updated = await ProfileService.UpdateProfileAsync(_profile.Uid, request);
                _profile = updated;
                
                // Atualiza o estado global para a AppBar refletir o nome caso tenha mudado
                UserState.UpdateName(_profile.Name);
                
                Snackbar.Add("Perfil atualizado com sucesso!", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao salvar perfil: {ex.Message}", Severity.Error);
            }
            finally
            {
                _saving = false;
            }
        }

        private async Task UploadPhoto(IBrowserFile file)
        {
            if (file == null || _profile == null) return;

            if (file.Size > 2 * 1024 * 1024) // 2MB limit
            {
                Snackbar.Add("A imagem deve ter no máximo 2MB", Severity.Warning);
                return;
            }

            try
            {
                var buffer = new byte[file.Size];
                await file.OpenReadStream(2 * 1024 * 1024).ReadExactlyAsync(buffer);
                var base64 = Convert.ToBase64String(buffer);

                var request = new UpdateUserProfileRequest
                {
                    Name = _profile.Name,
                    Email = _profile.Email,
                    PhotoBase64 = base64
                };

                var updated = await ProfileService.UpdateProfileAsync(_profile.Uid, request);
                _profile = updated;

                // Atualiza o estado global para a AppBar refletir a foto instantaneamente
                UserState.UpdatePhoto(_profile.PhotoBase64);
                
                Snackbar.Add("Foto atualizada com sucesso!", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao fazer upload da foto: {ex.Message}", Severity.Error);
            }
        }

        private async Task RemovePhotoAsync()
        {
            try
            {
                var request = new UpdateUserProfileRequest
                {
                    Name = _profile!.Name,
                    Email = _profile.Email,
                    RemovePhoto = true
                };

                var updated = await ProfileService.UpdateProfileAsync(_profile.Uid, request);
                _profile = updated;

                // Atualiza o estado global
                UserState.UpdatePhoto(null);

                Snackbar.Add("Foto removida com sucesso!", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao remover foto: {ex.Message}", Severity.Error);
            }
        }

        private async Task RemovePhoto()
        {
            if (_profile == null) return;

            await DialogService.ShowConfirm(
                "Tem certeza que deseja remover sua foto de perfil?", "Remover Foto", EventCallback.Factory.Create(this, async () => { await RemovePhotoAsync(); }));        
        }
        #endregion
    }
}
