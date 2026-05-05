using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Admin;
using eTasks_server.Core.BusinessLogicLayers.Admin.ServerSettings;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances;
using eTasks_server.Core.BusinessLogicLayers.API_Resources.Goals;
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
using eTasks_server.HostedServices;
using eTasks_server.Middlewares;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using MySqlConnector;
using Serilog;
using Serilog.Events;
using System.Text;

namespace eTasks_server.Extensions
{
    /// <summary>
    /// Classe carregadora de serviços
    /// </summary>
    public static class ServicesPayload
    {
        /// <summary>
        /// Extensão da classe serviços para organizar a configuração dos serviços do projeto
        /// </summary>
        /// <param name="services">Instância do IServiceCollection</param>
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Adiciona serviços no pipeline de injeção de dependências
            /// </summary>
            /// <param name="configuration">Configuração da aplicação</param>
            public void AddServicesPayload(ConfigurationManager configuration)
            {

                services
                        //Configura banco de dados
                        .SetupDatabase(configuration)

                        //Configura Razor Components
                        .SetupRazorComponents()

                        //Configura CORS
                        .SetupCors()

                        //Configura serviços do MudBlazor
                        .SetupMudServices()

                        //Configura HttpClient para chamadas à API
                        .SetupHttpClient(configuration)

                        //Configura health checks
                        .SetupHealthChecks(configuration)

                        //Configura tratamento global de exceções
                        .SetupGlobalExceptionHandler()

                        //Configura OpenAPI/Swagger
                        .SetupOpenApi()

                        //Configura segurança (autenticação e autorização)
                        .SetupSecurity(configuration)

                        //Registra serviços específicos da aplicação
                        .ServerAppServices();
            }

            #region Private Methods
            /// <summary>
            /// Configura os health checks do serviço, incluindo a verificação de integridade para o banco de dados
            /// MySQL.
            /// </summary>
            /// <param name="configuration">O gerenciador de configurações usado para obter a string de conexão do banco de dados MySQL.</param>
            /// <returns>A coleção de serviços com os health checks configurados.</returns>
            private IServiceCollection SetupHealthChecks(ConfigurationManager configuration)
            {
                //Adiciona serviços de health checks e configura um health check para o banco de dados MySQL usando a string de conexão fornecida na configuração. O health check é nomeado "mysql" e tem um status de falha definido como "Unhealthy" se a verificação falhar. O tempo limite para a verificação é definido como 5 segundos.
                services.AddHealthChecks()
                    .AddMySql(
                        configuration.GetConnectionString(Constants.DatabaseConnection)!,
                        name: "mysql",
                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                        timeout: TimeSpan.FromSeconds(5)
                    );

                return services;
            }

            /// <summary>
            /// Configura os serviços MudBlazor e localização para o contêiner de injeção de dependência da aplicação.
            /// </summary>
            /// <remarks>Inclui configuração personalizada para notificações Snackbar do MudBlazor,
            /// como posição, duração de exibição e comportamento de duplicidade. Deve ser chamado durante a
            /// configuração de serviços da aplicação para garantir que os componentes MudBlazor e recursos de
            /// localização estejam disponíveis.</remarks>
            /// <returns>A coleção de serviços configurada com suporte a MudBlazor e localização.</returns>
            private IServiceCollection SetupMudServices()
            {
                services.AddMudServices(options =>
                {
                    //Define posição do Snackbar no canto inferior direito da tela, previne mensagens duplicadas, exibe as mensagens mais recentes no topo, mostra um ícone de fechar, define a duração de exibição
                    options.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;

                    //Previne mensagens duplicadas, garantindo que a mesma mensagem não seja exibida várias vezes simultaneamente, o que pode melhorar a experiência do usuário e evitar confusão.
                    options.SnackbarConfiguration.PreventDuplicates = true;

                    //Exibe as mensagens mais recentes no topo da lista de notificações, garantindo que as informações mais recentes sejam mais visíveis para o usuário.
                    options.SnackbarConfiguration.NewestOnTop = true;

                    //Exibe um ícone de fechar em cada notificação, permitindo que os usuários fechem manualmente as mensagens de snackbar, o que pode melhorar a usabilidade e dar aos usuários mais controle sobre as notificações exibidas.
                    options.SnackbarConfiguration.ShowCloseIcon = true;

                    //Define a duração de exibição das mensagens de snackbar para 1000 milissegundos (1 segundo), o que significa que as notificações desaparecerão automaticamente após esse período, proporcionando uma experiência de usuário mais fluida e evitando que as mensagens permaneçam na tela por muito tempo.
                    options.SnackbarConfiguration.VisibleStateDuration = 1000;

                    //Define a duração da transição de ocultação e exibição das mensagens de snackbar para 500 milissegundos, o que proporciona uma animação suave ao mostrar ou ocultar as notificações, melhorando a experiência visual do usuário.
                    options.SnackbarConfiguration.HideTransitionDuration = 500;

                    //Define a duração da transição de exibição das mensagens de snackbar para 500 milissegundos, garantindo que as notificações apareçam suavemente na tela, melhorando a experiência visual do usuário.
                    options.SnackbarConfiguration.ShowTransitionDuration = 500;
                });

                //Adiciona serviços de localização para suportar recursos de internacionalização e localização na aplicação, permitindo que os componentes e mensagens sejam adaptados para diferentes culturas e idiomas conforme necessário.
                services.AddLocalization();

                return services;
            }

            /// <summary>
            /// Configura clientes HTTP nomeados para a aplicação usando as definições fornecidas.
            /// </summary>
            /// <remarks>Adiciona clientes HTTP nomeados 'LocalApi' e 'OpenRouter' ao contêiner de
            /// serviços. O cliente 'LocalApi' utiliza uma URL base definida nas configurações ou um valor padrão. O
            /// cliente 'OpenRouter' define um tempo limite de 60 segundos para as requisições.</remarks>
            /// <param name="configuration">O gerenciador de configurações utilizado para recuperar as URLs base e caminhos de API necessários para
            /// configurar os clientes HTTP.</param>
            /// <returns>O mesmo IServiceCollection com os clientes HTTP configurados.</returns>
            private IServiceCollection SetupHttpClient(ConfigurationManager configuration)
            {
                //Configura um cliente HTTP nomeado "LocalApi" com uma URL base definida nas configurações ou um valor padrão. Isso permite que o cliente seja injetado e utilizado em outras partes da aplicação para fazer requisições à API local.
                services.AddHttpClient("LocalApi", client =>
                {
                    var baseUrl = configuration[Constants.ApiBaseUrl] + configuration[Constants.ApiV2Path] ?? "http://localhost:5033/api/v2";
                    client.BaseAddress = new Uri(baseUrl);
                });

                //Configura um cliente HTTP nomeado "OpenRouter" com um tempo limite de 60 segundos para as requisições. Isso é útil para garantir que as chamadas à API do OpenRouter não fiquem pendentes por muito tempo, melhorando a resiliência da aplicação ao lidar com possíveis atrasos ou falhas na comunicação com o serviço externo.
                services.AddHttpClient("OpenRouter", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                });

                return services;
            }

            /// <summary>
            /// Configura os serviços necessários para suporte a Razor Components e componentes interativos no servidor.
            /// </summary>
            /// <remarks>Chame este método durante a configuração de serviços para habilitar o suporte
            /// a componentes Razor e funcionalidades interativas em aplicações ASP.NET Core.</remarks>
            /// <returns>A coleção de serviços atualizada com os serviços de Razor Components e componentes interativos.</returns>
            private IServiceCollection SetupRazorComponents()
            {
                //Adiciona serviços para suporte a Razor Components, que são componentes reutilizáveis usados para construir interfaces de usuário em aplicações ASP.NET Core. Além disso, habilita o suporte a componentes interativos no servidor, permitindo que os componentes Razor sejam renderizados e atualizados dinamicamente no lado do servidor, proporcionando uma experiência de usuário mais responsiva e interativa.
                services.AddRazorComponents()
                    .AddInteractiveServerComponents(); //Habilita suporte a componentes interativos no servidor

                return services;
            }

            /// <summary>
            /// Configura as políticas de CORS para o aplicativo, permitindo origens, cabeçalhos e métodos específicos,
            /// além de credenciais, conforme definido nas constantes da aplicação.
            /// </summary>
            /// <remarks>Utilize este método durante a configuração de serviços para garantir que as
            /// políticas de CORS estejam corretamente aplicadas antes de inicializar o pipeline de requisições. A
            /// política de CORS aplicada permite qualquer cabeçalho, qualquer método e credenciais para as origens
            /// especificadas.</remarks>
            /// <returns>O mesmo IServiceCollection fornecido, permitindo o encadeamento de chamadas de configuração.</returns>
            private IServiceCollection SetupCors()
            {
                //Configura as políticas de CORS para o aplicativo, permitindo origens, cabeçalhos e métodos específicos, além de credenciais, conforme definido nas constantes da aplicação. Isso é essencial para permitir que clientes de diferentes origens acessem os recursos da API de forma segura e controlada.
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

            /// <summary>
            /// Configura o contexto de banco de dados MySQL para a aplicação usando as informações fornecidas na
            /// configuração.
            /// </summary>
            /// <remarks>O contexto de banco de dados é configurado para usar MySQL com detecção
            /// automática de versão do servidor e política de retry em caso de falha. O comportamento de divisão de
            /// consultas é definido como SplitQuery.</remarks>
            /// <param name="configuration">O gerenciador de configuração utilizado para obter a string de conexão do banco de dados.</param>
            /// <returns>A coleção de serviços com o contexto de banco de dados configurado.</returns>
            /// <exception cref="InvalidOperationException">Lançada se a string de conexão do banco de dados não estiver configurada.</exception>
            private IServiceCollection SetupDatabase(ConfigurationManager configuration)
            {
                var rawConnectionString = configuration.GetConnectionString(Constants.DatabaseConnection)
                    ?? throw new InvalidOperationException("Connection string DefaultConnection não configurada.");

                //Configura o construtor de string de conexão do MySQL para usar o formato de GUID TimeSwapBinary16, que é recomendado para melhor desempenho e compatibilidade com o MySQL. Isso garante que os GUIDs sejam armazenados de forma eficiente no banco de dados, melhorando a performance das operações de leitura e escrita.
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

            /// <summary>
            /// Configura e registra os serviços de autenticação, autorização e as dependências de negócios e
            /// administrativas do aplicativo no contêiner de injeção de dependência do servidor.
            /// </summary>
            /// <remarks>Inclui o registro de serviços relacionados à autenticação, autorização,
            /// gerenciamento de usuários, tarefas, metas, notas, leituras, listas de compras, finanças, versões,
            /// notificações, auditoria e administração. Deve ser chamado durante a configuração do servidor para
            /// garantir que todas as dependências necessárias estejam disponíveis para injeção.</remarks>
            /// <returns>A coleção de serviços configurada para uso pelo aplicativo do servidor.</returns>
            private IServiceCollection ServerAppServices()
            {
                //Adiciona Autenticação e Autorização em cascata, permitindo que os componentes Razor e outros serviços possam acessar o estado de autenticação e autorização do usuário. Além disso, registra uma série de serviços de negócios (BLL) e serviços administrativos relacionados a autenticação, gerenciamento de usuários, tarefas, metas, notas, leituras, listas de compras, finanças, versões, notificações, auditoria e administração. Esses serviços são registrados com escopo para garantir que uma nova instância seja criada para cada solicitação, permitindo um gerenciamento eficiente dos recursos e garantindo que as dependências sejam resolvidas corretamente durante a execução do aplicativo.
                services.AddCascadingAuthenticationState();
                services.AddAuthorizationCore();

                //Adiciona Serviço de Login administrativo para o Blazor Server, permitindo que os componentes Razor acessem o estado de autenticação e autorização do usuário, facilitando a implementação de funcionalidades de login e controle de acesso em uma aplicação Blazor Server.
                services.AddScoped<IWebAuthBLL, WebAuthBLL>();

                //Adiciona serviços de negócios (BLL) para autenticação, gerenciamento de usuários, tarefas, metas, notas, leituras, listas de compras, finanças, versões, notificações, auditoria e administração. Esses serviços encapsulam a lógica de negócios da aplicação e são registrados com escopo para garantir que uma nova instância seja criada para cada solicitação.
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
                services.AddScoped<IOperationAuditAdminBLL, OperationAuditAdminBLL>();
                services.AddScoped<IUserNotificationBLL, UserNotificationBLL>();
                services.AddScoped<IAdminNotificationBLL, AdminNotificationBLL>();
                services.AddScoped<IVersionService, VersionService>();
                services.AddScoped<IUserAdminService, UserAdminService>();
                services.AddScoped<IServerSettingsAdminService, ServerSettingsAdminService>();
                services.AddScoped<IBonusAdminService, BonusAdminService>();
                services.AddScoped<IDatabaseAdminService, DatabaseAdminService>();
                services.AddScoped<IApplicationLogAdminService, ApplicationLogAdminService>();
                services.AddScoped<IOperationAuditAdminService, OperationAuditAdminService>();
                services.AddScoped<IAdminNotificationService, AdminNotificationService>();
                services.AddScoped<IUserProfileService, UserProfileService>();
                services.AddScoped<UserState>();
                services.AddScoped<IDashboardBLL, DashboardBLL>();
                services.AddScoped<IDashboardService, DashboardService>(sp =>
                    new DashboardService(
                        sp.GetRequiredService<IServiceScopeFactory>(),
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient("LocalApi")));
                services.AddScoped<UserLogsDrawerService>();

                return services;
            }

            /// <summary>
            /// Configura os serviços necessários para a documentação OpenAPI da aplicação.
            /// </summary>
            /// <remarks>Este método adiciona e personaliza a documentação OpenAPI, incluindo título,
            /// descrição e versão da API. Deve ser chamado durante a configuração dos serviços da aplicação para
            /// garantir que a documentação OpenAPI seja gerada corretamente.</remarks>
            /// <returns>Os serviços de dependência atualizados com o suporte à documentação OpenAPI.</returns>
            private IServiceCollection SetupOpenApi()
            {
                //Configura os serviços necessários para a documentação OpenAPI da aplicação, personalizando o título, descrição e versão da API. Isso é essencial para fornecer uma documentação clara e útil para os consumidores da API, facilitando a compreensão dos endpoints disponíveis e suas funcionalidades.
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

            /// <summary>
            /// Configura o manipulador global de exceções para o aplicativo e adiciona suporte ao formato Problem
            /// Details.
            /// </summary>
            /// <remarks>Adiciona o middleware de tratamento de exceções personalizado e o suporte ao
            /// padrão Problem Details, permitindo respostas padronizadas para erros HTTP. Deve ser chamado durante a
            /// configuração dos serviços da aplicação.</remarks>
            /// <returns>A instância de <see cref="IServiceCollection"/> com os serviços de tratamento de exceções configurados.</returns>
            private IServiceCollection SetupGlobalExceptionHandler()
            {
                //Adiciona suporte ao formato Problem Details para respostas de erro HTTP, permitindo que a aplicação retorne respostas padronizadas e estruturadas em caso de erros. Além disso, registra um manipulador global de exceções personalizado (GlobalExceptionHandler) para capturar e processar exceções não tratadas em toda a aplicação, garantindo que as respostas de erro sejam consistentes e informativas para os consumidores da API.
                services.AddProblemDetails();

                //Adiciona um manipulador global de exceções personalizado (GlobalExceptionHandler) para capturar e processar exceções não tratadas em toda a aplicação, garantindo que as respostas de erro sejam consistentes e informativas para os consumidores da API. O GlobalExceptionHandler deve ser implementado para lidar com diferentes tipos de exceções e retornar respostas apropriadas com base na natureza do erro.
                services.AddExceptionHandler<GlobalExceptionHandler>();
                
                return services;
            }

            /// <summary>
            /// Configura os serviços de autenticação e autorização para a aplicação, incluindo políticas de acesso e registro de serviços relacionados à segurança, como autenticação JWT, autenticação por cookie e serviços de negócios relacionados à autenticação e autorização. Isso é essencial para proteger os recursos da API e garantir que apenas usuários autorizados possam acessar determinadas funcionalidades. Deve ser chamado durante a configuração dos serviços da aplicação para garantir que as políticas de segurança sejam aplicadas corretamente.
            /// </summary>
            /// <param name="configuration">Configurações da aplicação</param>
            /// <returns>A instância de <see cref="IServiceCollection"/> com os serviços de autenticação e autorização configurados.</returns>
            private IServiceCollection SetupSecurity(ConfigurationManager configuration)
            {
                //Configura os serviços de autenticação e autorização para a aplicação, incluindo políticas de acesso e registro de serviços relacionados à segurança, como autenticação JWT, autenticação por cookie e serviços de negócios relacionados à autenticação e autorização. Isso é essencial para proteger os recursos da API e garantir que apenas usuários autorizados possam acessar determinadas funcionalidades. Deve ser chamado durante a configuração dos serviços da aplicação para garantir que as políticas de segurança sejam aplicadas corretamente.
                services.AddAuthorization(options =>
                {
                    //Adiciona Política de autorização "Admin" que requer que o usuário tenha a função "Admin". Essa política pode ser aplicada a endpoints ou controladores específicos para restringir o acesso apenas a usuários com privilégios administrativos, garantindo que apenas usuários autorizados possam acessar funcionalidades sensíveis ou administrativas da aplicação.
                    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));

                    //Adiciona Política de autorização "WebAdmin" que requer autenticação via cookie e a função "Admin". Essa política é específica para o painel administrativo da aplicação, garantindo que apenas usuários autenticados por cookie e com privilégios administrativos possam acessar as funcionalidades administrativas do painel, proporcionando uma camada adicional de segurança para as áreas sensíveis da aplicação.
                    options.AddPolicy("WebAdmin", policy =>
                    {
                        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Admin");
                    });
                });

                //Adiciona controladores para a aplicação, permitindo que os endpoints da API sejam definidos usando controladores MVC, o que é essencial para a construção de uma API RESTful e para a organização do código relacionado aos endpoints da API.
                services.AddControllers();

                //Adiciona serviço de email
                services.AddScoped<IEmailService, EmailService>();
                
                //Adiciona serviços relacionados à inteligência artificial
                services.AddSingleton<IAiCapabilityCatalog, AiCapabilityCatalog>();

                //Adiciona serviço de composição de prompts para inteligência artificial, permitindo que os prompts sejam gerados dinamicamente com base nas capacidades disponíveis e nas necessidades da aplicação. Isso é essencial para integrar funcionalidades de IA de forma flexível e adaptável, garantindo que os prompts sejam relevantes e eficazes para as interações com os usuários ou para a execução de tarefas específicas relacionadas à inteligência artificial.
                services.AddScoped<IAiPromptComposer, AiPromptComposer>();

                //Adiciona serviço de assistente de inteligência artificial usando o OpenRouter, permitindo que a aplicação se comunique com o serviço de IA do OpenRouter para fornecer funcionalidades avançadas de assistente virtual, como respostas a perguntas, execução de tarefas ou outras interações baseadas em IA. Isso é essencial para integrar capacidades de inteligência artificial na aplicação e oferecer uma experiência mais rica e interativa para os usuários.
                services.AddScoped<IAiAssistantService, OpenRouterAiAssistantService>();

                //Adiciona serviços relacionados à administração do servidor, como gerenciamento de configurações, auditoria e retenção de logs, além de um serviço de proteção de segredos para garantir que informações sensíveis sejam protegidas adequadamente. Esses serviços são essenciais para a manutenção e segurança do servidor, permitindo que os administradores gerenciem as configurações do servidor, monitorem as atividades e garantam a conformidade com as políticas de retenção de dados.
                services.AddScoped<IServerSettingsProvider, ServerSettingsProvider>();

                //Adiciona serviço de diagnóstico para as configurações do servidor, permitindo que os administradores verifiquem a integridade e a conformidade das configurações do servidor, identificando possíveis problemas ou inconsistências que possam afetar o desempenho ou a segurança do servidor.
                services.AddScoped<IServerSettingsDiagnosticsService, ServerSettingsDiagnosticsService>();

                //Adiciona serviço de retenção de logs da aplicação, permitindo que os administradores configurem e gerenciem as políticas de retenção de logs para garantir que os dados de log sejam mantidos por um período adequado para fins de auditoria e conformidade, enquanto também ajudam a gerenciar o armazenamento e a performance do sistema.
                services.AddScoped<IApplicationLogRetentionService, ApplicationLogRetentionService>();

                //Adiciona serviço de auditoria de operações usando MongoDB, permitindo que as atividades e operações realizadas na aplicação sejam registradas e armazenadas de forma eficiente em um banco de dados MongoDB, facilitando a análise e o monitoramento das ações dos usuários e do sistema para fins de segurança, conformidade e melhoria contínua.
                services.AddScoped<IOperationAuditLogger, MongoOperationAuditLogger>();

                //Adiciona serviço de proteção de segredos, garantindo que informações sensíveis, como chaves de API, senhas ou outros segredos, sejam protegidas adequadamente dentro da aplicação. Isso é essencial para manter a segurança dos dados e evitar exposições acidentais de informações confidenciais.
                services.AddSingleton<ISecretProtector, SecretProtector>();

                //Adiciona serviços de negócios relacionados à autenticação e autorização, permitindo que a lógica de autenticação e gerenciamento de usuários seja encapsulada em serviços dedicados, facilitando a manutenção e a escalabilidade da aplicação. Esses serviços são essenciais para implementar funcionalidades de login, controle de acesso e gerenciamento de usuários de forma eficiente e segura.
                services.AddScoped<IAuthBLL, AuthBLL>();

                //Adiciona um serviço hospedado para a retenção de logs da aplicação, permitindo que as políticas de retenção de logs sejam aplicadas de forma automática e contínua, garantindo que os dados de log sejam mantidos por um período adequado para fins de auditoria e conformidade, enquanto também ajudam a gerenciar o armazenamento e a performance do sistema.
                services.AddHostedService<ApplicationLogRetentionHostedService>();

                //Adiciona serviços de autenticação usando uma política híbrida que suporta tanto autenticação JWT quanto autenticação por cookie. A política de autenticação "Hybrid" é configurada para selecionar automaticamente o esquema de autenticação com base na presença do cabeçalho de autorização, permitindo que a aplicação suporte diferentes métodos de autenticação de forma flexível e adaptável às necessidades dos usuários e dos clientes da API.
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

        /// <summary>
        /// Extensão da classe WebApplicationBuilder para configurar o Serilog e o armazenamento de logs em tempo real
        /// </summary>
        /// <param name="builder">Instância do WebApplicationBuilder</param>
        extension(WebApplicationBuilder builder)
        {
            /// <summary>
            /// Método para configurar o Serilog e o armazenamento de logs em tempo real, além de registrar os serviços específicos da aplicação
            /// </summary>
            public void RegisterServices()
            {
                builder
                       .SetupSerilog(builder.Environment) //Configura o Serilog e o armazenamento de logs em tempo real
                       .AddServicesPayload(builder.Configuration); //Configura os serviços do projeto
            }

            #region Private Methods
            /// <summary>
            /// Configura o Serilog para o aplicativo, incluindo armazenamento de logs em tempo real, gravação em
            /// arquivo e enriquecimento de contexto de log.
            /// </summary>
            /// <remarks>Inclui configuração para logs em tempo real e persistência em arquivos com
            /// rotação diária. Os logs do Microsoft são registrados apenas em nível de aviso ou superior. O diretório
            /// de logs é definido como uma subpasta 'logs' do diretório raiz do conteúdo.</remarks>
            /// <param name="env">O ambiente de hospedagem web atual. Usado para determinar o caminho raiz do conteúdo e configurar o
            /// diretório de logs.</param>
            /// <returns>A coleção de serviços configurada com os provedores de log Serilog e serviços relacionados.</returns>
            private IServiceCollection SetupSerilog(IWebHostEnvironment env)
            {
                //Define o diretório de logs como uma subpasta 'logs' do diretório raiz do conteúdo
                var logsDirectory = Path.Combine(
                    Directory.GetParent(env.ContentRootPath)?.FullName ?? env.ContentRootPath,
                    "logs");

                //Inicializa o armazenamento de logs em tempo real e o registra como um serviço singleton para injeção de dependências
                var realtimeLogStore = new RealtimeLogStore();

                //Injeta o armazenamento de logs em tempo real como um serviço singleton para que possa ser usado em toda a aplicação
                builder.Services.AddSingleton<IRealtimeLogStore>(realtimeLogStore);

                //Configura as opções de log da aplicação para usar o diretório de logs definido
                builder.Services.Configure<ApplicationLogAdminOptions>(options =>
                {
                    options.LogsDirectoryPath = logsDirectory;
                });

                //Configura o Serilog para registrar logs em tempo real, gravar em arquivos com rotação diária e enriquecer os logs com informações de contexto
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
                        retainedFileCountLimit: 15)
                    .CreateLogger();

                //Configura o aplicativo para usar o Serilog como provedor de log
                builder.Host.UseSerilog();

                return builder.Services;
            }
            #endregion
        }
    }
}
