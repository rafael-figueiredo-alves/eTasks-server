using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using System.Text;
using eTasks_server.Middlewares;

namespace eTasks_server.Extensions
{
    public static class ServicesPayload
    {
        extension(IServiceCollection services)
        {
            public void AddServicesPayload(ConfigurationManager configuration)
            {
                services.SetupDatabase(configuration)
                        .SetupRazorComponents()
                        .SetupCors()
                        .SetupMudServices()
                        .SetupHttpClient(configuration)
                        .SetupHealthChecks(configuration)
                        .SetupGlobalExceptionHandler()
                        .SetupOpenApi()
                        .SetupSecurity(configuration)
                        .ServerAppServices();
            }

            #region Private Methods
            private IServiceCollection SetupHealthChecks(ConfigurationManager configuration)
            {
                services.AddHealthChecks()
                    .AddMySql(
                        configuration.GetConnectionString(Constants.DatabaseConnection)!,
                        name: "mysql",
                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromSeconds(5)
                    );

                return services;
            }

            private IServiceCollection SetupMudServices()
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

                return services;
            }

            private IServiceCollection SetupHttpClient(ConfigurationManager configuration)
            {
                services.AddScoped(sp =>
                    new HttpClient { BaseAddress = new Uri(configuration[Constants.ApiBaseUrl]!) });

                return services;
            }

            private IServiceCollection SetupRazorComponents()
            {
                services.AddRazorComponents()
                    .AddInteractiveServerComponents()
                    .AddInteractiveWebAssemblyComponents();

                return services;
            }

            private IServiceCollection SetupCors()
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

                return services;
            }

            private IServiceCollection SetupDatabase(ConfigurationManager configuration)
            {
                services.AddDbContext<AppDbContext>(options =>
                                                        options.UseMySql(
                                                        configuration.GetConnectionString(Constants.DatabaseConnection),
                                                        ServerVersion.AutoDetect(configuration.GetConnectionString(Constants.DatabaseConnection)),
                                                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()  // Opcional: retry em falhas
                                                        )
                                                    );

                return services;
            }

            private IServiceCollection ServerAppServices()
            {
                services.AddScoped<VersionBLL>();
                services.AddScoped<IVersionService, VersionService>();

                return services;
            }

            private IServiceCollection SetupOpenApi()
            {
                services.AddOpenApi(Constants.ApiVersion, options =>
                {
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Info.Title = Constants.AppTitle;
                        document.Info.Description = Constants.ApiDescription;
                        document.Info.Version = Constants.ApiVersion;
                        return Task.CompletedTask;
                    });
                });
                return services;
            }

            private IServiceCollection SetupGlobalExceptionHandler()
            {
                services.AddProblemDetails();
                services.AddExceptionHandler<GlobalExceptionHandler>();
                return services;
            }

            private IServiceCollection SetupSecurity(ConfigurationManager configuration)
            {
                services.AddAuthorization();
                services.AddControllers();

                // Dependency Injection mapping
                services.AddScoped<eTasks_server.Core.Services.Interfaces.IEmailService, eTasks_server.Core.Services.EmailService>();
                services.AddScoped<eTasks_server.Core.BusinessLogicLayers.Interfaces.IAuthBLL, eTasks_server.Core.BusinessLogicLayers.AuthBLL>();

                services.AddAuthentication("Bearer")
                    .AddJwtBearer(options =>
                    {
                        var jwtKey = configuration[Constants.JwtKeyConfig] ?? "defaultSecretKey_1234567890_min32chars!";
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration[Constants.JwtIssuerConfig] ?? "eTasksServer",
                            ValidAudience = configuration[Constants.JwtAudienceConfig] ?? "eTasksClient",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                    });

                return services;
            }
            #endregion
        }

        extension(WebApplicationBuilder builder)
        {
            public void RegisterServices()
            {
                builder.SetupSerilog(builder.Environment)
                       .AddServicesPayload(builder.Configuration);
            }

            #region Private Methods
            private IServiceCollection SetupSerilog(IWebHostEnvironment env)
            {
                // Configuração do Serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .WriteTo.Console()
                    .WriteTo.File(
                        Directory.GetParent(env.ContentRootPath)?.FullName + Path.DirectorySeparatorChar +
                        "logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 10)
                    .CreateLogger();

                builder.Host.UseSerilog();

                return builder.Services;
            }
            #endregion
        }
    }
}
