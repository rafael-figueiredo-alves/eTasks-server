using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Admin;
using eTasks_server.Core.BusinessLogicLayers.Admin.ServerSettings;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Goals;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Notes;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Readings;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Shopping;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Tasks;
using eTasks_server.Core.BusinessLogicLayers.Auth;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Notifications;
using eTasks_server.Core.BusinessLogicLayers.Usuarios;
using eTasks_server.Core.BusinessLogicLayers.Version;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Middlewares;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using System.Text;

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
                services.AddLocalization();

                return services;
            }

            private IServiceCollection SetupHttpClient(ConfigurationManager configuration)
            {
                services.AddHttpClient("LocalApi", client =>
                {
                    var baseUrl = configuration[Constants.ApiBaseUrl] + configuration[Constants.ApiV2Path] ?? "http://localhost:5033/api/v2";
                    client.BaseAddress = new Uri(baseUrl);
                });

                services.AddHttpClient("OpenRouter", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                });
                return services;
            }

            private IServiceCollection SetupRazorComponents()
            {
                services.AddRazorComponents()
                    .AddInteractiveServerComponents();

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
                var rawConnectionString = configuration.GetConnectionString(Constants.DatabaseConnection)
                    ?? throw new InvalidOperationException("Connection string DefaultConnection nao configurada.");
                var connectionStringBuilder = new MySqlConnectionStringBuilder(rawConnectionString)
                {
                    GuidFormat = MySqlGuidFormat.TimeSwapBinary16
                };
                var connectionString = connectionStringBuilder.ConnectionString;

                services.AddDbContext<AppDbContext>(options =>
                                                        options.UseMySql(
                                                        connectionString,
                                                        ServerVersion.AutoDetect(connectionString),
                                                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
                                                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                                                        )                                                     
                                                    );

                return services;
            }

            private IServiceCollection ServerAppServices()
            {
                services.AddCascadingAuthenticationState();
                services.AddAuthorizationCore();
                services.AddScoped<IWebAuthBLL, WebAuthBLL>();
                services.AddScoped<IUserProfileBLL, UserProfileBLL>();
                services.AddScoped<ITaskBLL, TaskBLL>();
                services.AddScoped<IGoalBLL, GoalBLL>();
                services.AddScoped<INoteBLL, NoteBLL>();
                services.AddScoped<IReadingBLL, ReadingBLL>();
                services.AddScoped<IShoppingListBLL, ShoppingListBLL>();
                services.AddScoped<IFinanceBLL, FinanceBLL>();

                services.AddScoped<IVersionBLL, VersionBLL>();
                services.AddScoped<IUserAdminBLL, UserAdminBLL>();
                services.AddScoped<IServerSettingsAdminBLL, ServerSettingsAdminBLL>();
                services.AddScoped<IBonusAdminBLL, BonusAdminBLL>();
                services.AddScoped<IDatabaseAdminBLL, DatabaseAdminBLL>();
                services.AddScoped<IApplicationLogAdminBLL, ApplicationLogAdminBLL>();
                services.AddScoped<IUserNotificationBLL, UserNotificationBLL>();
                services.AddScoped<IAdminNotificationBLL, AdminNotificationBLL>();
                services.AddScoped<IVersionService, VersionService>();
                services.AddScoped<IUserAdminService, UserAdminService>();
                services.AddScoped<IServerSettingsAdminService, ServerSettingsAdminService>();
                services.AddScoped<IBonusAdminService, BonusAdminService>();
                services.AddScoped<IDatabaseAdminService, DatabaseAdminService>();
                services.AddScoped<IApplicationLogAdminService, ApplicationLogAdminService>();
                services.AddScoped<IAdminNotificationService, AdminNotificationService>();
                services.AddScoped<IUserProfileService, UserProfileService>();
                services.AddScoped<UserState>();
                services.AddScoped<IDashboardBLL, DashboardBLL>();
                services.AddScoped<IDashboardService, DashboardService>(sp =>
                {
                    var bll = sp.GetRequiredService<IDashboardBLL>();
                    var factory = sp.GetRequiredService<IHttpClientFactory>();
                    var client = factory.CreateClient("LocalApi");
                    return new DashboardService(bll, client);
                });
                services.AddScoped<UserLogsDrawerService>();

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
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                    options.AddPolicy("WebAdmin", policy =>
                    {
                        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Admin");
                    });
                });
                services.AddControllers();

                services.AddScoped<IEmailService, EmailService>();
                services.AddSingleton<IAiCapabilityCatalog, AiCapabilityCatalog>();
                services.AddScoped<IAiPromptComposer, AiPromptComposer>();
                services.AddScoped<IAiAssistantService, OpenRouterAiAssistantService>();
                services.AddScoped<IServerSettingsProvider, ServerSettingsProvider>();
                services.AddScoped<IServerSettingsDiagnosticsService, ServerSettingsDiagnosticsService>();
                services.AddScoped<IOperationAuditLogger, MongoOperationAuditLogger>();
                services.AddSingleton<ISecretProtector, SecretProtector>();
                services.AddScoped<IAuthBLL, AuthBLL>();

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Hybrid";
                    options.DefaultAuthenticateScheme = "Hybrid";
                    options.DefaultChallengeScheme = "Hybrid";
                })
                .AddPolicyScheme("Hybrid", "JWT or Cookie", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers.Authorization.ToString();
                        if (!string.IsNullOrWhiteSpace(authHeader)
                            && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }

                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/login";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
                var logsDirectory = Path.Combine(
                    Directory.GetParent(env.ContentRootPath)?.FullName ?? env.ContentRootPath,
                    "logs");
                var realtimeLogStore = new RealtimeLogStore();

                builder.Services.AddSingleton<IRealtimeLogStore>(realtimeLogStore);
                builder.Services.Configure<ApplicationLogAdminOptions>(options =>
                {
                    options.LogsDirectoryPath = logsDirectory;
                });

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .WriteTo.Console()
                    .WriteTo.Sink(new RealtimeLogSink(realtimeLogStore))
                    .WriteTo.File(
                        Path.Combine(logsDirectory, "log-.txt"),
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
