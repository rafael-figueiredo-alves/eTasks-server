using eTasks_server.Client.Components;
using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace eTasks_server.Client.Layout
{
    public class AppBarBase : ComponentBase
    {
        [Inject] protected NavigationManager? Navigation { get; set; }
        [Inject] protected IDialogService? DialogService { get; set; }
        [Inject] protected IUserProfileService? ProfileService { get; set; }
        [Inject] protected IUserAdminService? UserAdminService { get; set; }
        [Inject] protected UserState? UserState { get; set; }
        [Inject] protected ISnackbar? Snackbar { get; set; }

        [CascadingParameter]
        protected Task<AuthenticationState> AuthState { get; set; } = default!;

        protected bool _drawerOpen;

        protected override void OnInitialized()
        {
            UserState?.OnChange += StateHasChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadUserDataAsync();
            }
        }

        protected async Task LoadUserDataAsync()
        {
            var authState = await AuthState;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var name = user.Identity.Name;
                var uidStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(UserState!.Name))
                {
                    UserState!.UpdateName(name);
                }

                if (Guid.TryParse(uidStr, out var uid))
                {
                    try
                    {
                        // Pequeno delay para garantir que a página principal (que usa o mesmo DbContext) 
                        // já tenha iniciado/terminado sua própria consulta durante o pre-render/load.
                        await Task.Delay(10);

                        var profile = await ProfileService!.GetProfileAsync(uid);

                        UserState!.UpdatePhoto(profile.PhotoBase64);
                        UserState!.UpdateName(profile.Name);
                        UserState!.UpdateTheme(profile.Settings.Theme.Equals("dark", StringComparison.OrdinalIgnoreCase));
                    }
                    catch
                    {
                        // Se falhar por concorrência, o Estado manterá apenas o nome vindo do cookie
                    }
                }
            }
        }

        protected async Task ToggleTheme()
        {
            var newIsDark = !UserState!.IsDarkTheme;
            UserState.UpdateTheme(newIsDark);

            var authState = await AuthState;
            var uidStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(uidStr, out var uid))
            {
                try
                {
                    await ProfileService!.UpdateSettingsAsync(uid, new PatchUserSettingsRequest
                    {
                        Theme = newIsDark ? "dark" : "light"
                    });
                }
                catch (Exception ex)
                {
                    Snackbar?.Add($"Erro ao salvar preferência de tema: {ex.Message}", Severity.Warning);
                }
            }
        }

        public void Dispose()
        {
            UserState?.OnChange -= StateHasChanged;
        }

        protected void ToggleDrawer()
        {
            _drawerOpen = !_drawerOpen;
        }

        protected void NavigateTo(string url, bool forceLoad = false)
        {
            Navigation?.NavigateTo(url, forceLoad);
        }

        protected async Task ChangePassword()
        {
            var authState = await AuthState;
            var uidStr = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(uidStr, out var uid))
            {
                var parameters = new DialogParameters();
                var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

                var dialog = await DialogService!.ShowAsync<SetPasswordDialog>("Alterar Minha Senha", parameters, options);
                var result = await dialog.Result;

                if (!result!.Canceled && result.Data is string newPassword)
                {
                    try
                    {
                        var success = await UserAdminService!.SetPasswordAsync(uid, newPassword);
                        if (success)
                        {
                            Snackbar?.Add("Senha alterada com sucesso!", Severity.Success);
                        }
                        else
                        {
                            Snackbar?.Add("Não foi possível alterar a senha.", Severity.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Snackbar?.Add($"Erro ao alterar senha: {ex.Message}", Severity.Error);
                    }
                }
            }
        }

        protected void Logout()
        {
            Navigation?.NavigateTo("/api/v2/web-auth/logout?returnUrl=%2Flogin", forceLoad: true);
        }
    }
}
