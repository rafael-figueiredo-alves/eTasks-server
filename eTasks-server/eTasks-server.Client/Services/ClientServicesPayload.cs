using eTasks_server.Client.Auth;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace eTasks_server.Client.Services
{
    public static class ClientServicesPayload
    {
        extension(WebAssemblyHostBuilder builder)
        {
            public void AddClientServices()
            {
                builder.SetupMudBlazorServices()
                       .SetupHttpClient()
                       .SetupAPIServices();
            }

            private WebAssemblyHostBuilder SetupMudBlazorServices()
            {
                builder.Services.AddMudServices(options =>
                {
                    options.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
                    options.SnackbarConfiguration.PreventDuplicates = true;
                    options.SnackbarConfiguration.NewestOnTop = true;
                    options.SnackbarConfiguration.ShowCloseIcon = true;
                    options.SnackbarConfiguration.VisibleStateDuration = 1000;
                    options.SnackbarConfiguration.HideTransitionDuration = 500;
                    options.SnackbarConfiguration.ShowTransitionDuration = 500;
                });

                return builder;
            }

            private WebAssemblyHostBuilder SetupHttpClient()
            {
                builder.Services.AddScoped(sp =>
                    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + Constants.URLClientServicesAPISegment) });

                return builder;
            }

            private WebAssemblyHostBuilder SetupAPIServices()
            {
                builder.Services.AddAuthorizationCore();
                builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
                builder.Services.AddScoped<IAuthServices, AuthService>();
                builder.Services.AddScoped<TokenAuthenticationProvider>();
                builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthenticationProvider>());
                builder.Services.AddScoped<IAuthToken>(sp => sp.GetRequiredService<TokenAuthenticationProvider>());

                builder.Services.AddScoped<IVersionService, VersionService>();
                builder.Services.AddScoped<IUserAdminService, UserAdminService>();
                builder.Services.AddScoped<UserLogsDrawerService>();

                return builder;
            }
        }
    }
}
