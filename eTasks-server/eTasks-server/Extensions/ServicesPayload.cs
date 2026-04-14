using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;

using eTasks_server.Core.BusinessLogicLayers;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
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

                return services;
            }

            private IServiceCollection SetupHttpClient(ConfigurationManager configuration)
            {
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
                services.AddDbContext<AppDbContext>(options =>
                                                        options.UseMySql(
                                                        configuration.GetConnectionString(Constants.DatabaseConnection),
                                                        ServerVersion.AutoDetect(configuration.GetConnectionString(Constants.DatabaseConnection)),
                                                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
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

                services.AddScoped<IVersionBLL, VersionBLL>();
                services.AddScoped<IUserAdminBLL, UserAdminBLL>();
                services.AddScoped<IVersionService, VersionService>();
                services.AddScoped<IUserAdminService, UserAdminService>();
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
