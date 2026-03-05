using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

namespace eTasks_server.Extensions
{
    public static class ServicesPayload
    {
        extension(IServiceCollection services)
        {
            public void AddServicesPayload(ConfigurationManager configuration)
            {
                services.SetupDatabase(configuration);

                services.SetupRazorComponents();

                services.SetupCors();

                services.SetupMudServices();

                services.SetupHttpClient(configuration);

                services.SetupHealthChecks(configuration);

                services.ServerAppServices();
            }

            private void SetupHealthChecks(ConfigurationManager configuration)
            {
                services.AddHealthChecks()
                    .AddMySql(
                        configuration.GetConnectionString(Constants.DatabaseConnection)!,
                        name: "mysql",
                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromSeconds(5)
                    );
            }

            private void SetupMudServices()
            {
                services.AddMudServices(options =>
                {
                    options.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
                    options.SnackbarConfiguration.PreventDuplicates = true;
                    options.SnackbarConfiguration.NewestOnTop = true;
                    options.SnackbarConfiguration.ShowCloseIcon = true;
                    options.SnackbarConfiguration.VisibleStateDuration = 1000;
                    options.SnackbarConfiguration.HideTransitionDuration = 500;
                    options.SnackbarConfiguration.ShowTransitionDuration = 500;
                });
            }

            private void SetupHttpClient(ConfigurationManager configuration)
            {
                services.AddScoped(sp =>
                    new HttpClient { BaseAddress = new Uri(configuration[Constants.ApiBaseUrl]!) });
            }

            private void SetupRazorComponents()
            {
                services.AddRazorComponents()
                    .AddInteractiveServerComponents()
                    .AddInteractiveWebAssemblyComponents();
            }

            private void SetupCors()
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(Constants.CorsPolicyName,
                     policy =>
                     {
                         policy
                               .WithOrigins(Constants.AllowedOrigin)
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                     });
                });
            }

            private void SetupDatabase(ConfigurationManager configuration)
            {
                services.AddDbContext<AppDbContext>(options =>
                                                        options.UseMySql(
                                                        configuration.GetConnectionString(Constants.DatabaseConnection),
                                                        ServerVersion.AutoDetect(configuration.GetConnectionString(Constants.DatabaseConnection)),
                                                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()  // Opcional: retry em falhas
                                                        )
                                                    );
            }

            private void ServerAppServices()
            {
                services.AddScoped<VersionBLL>();
            }
        }
    }
}
