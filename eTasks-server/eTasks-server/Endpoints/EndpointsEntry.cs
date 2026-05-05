using eTasks_server.Endpoints.Admin;
using eTasks_server.Endpoints.AI;
using eTasks_server.Endpoints.API_Resourcers.Goals;
using eTasks_server.Endpoints.API_Resourcers.Finances;
using eTasks_server.Endpoints.API_Resourcers.Notes;
using eTasks_server.Endpoints.API_Resourcers.Readings;
using eTasks_server.Endpoints.API_Resourcers.Shopping;
using eTasks_server.Endpoints.API_Resourcers.Tasks;
using eTasks_server.Endpoints.Auth;
using eTasks_server.Endpoints.Notifications;
using eTasks_server.Endpoints.Usuarios;
using eTasks_server.Endpoints.Utils;
using eTasks_server.Endpoints.Version;

namespace eTasks_server.Endpoints
{
    //Classe para adicionar endpoints de forma organizada e centralizada
    public static class EndpointsEntry
    {
        extension(WebApplication app)
        {
            public async Task AddAPIEndpoints()
            {
                //Define o prefixo para os endpoints da API, facilitando a organização e manutenção das rotas
                var API_V2 = app.MapGroup("/api")
                                    .MapGroup("/v2");

                //Adiciona endpoints de versão da API
                API_V2.MapVersionEndpoints();

                //Adiciona endpoints relacionados a serviços necessários do App, como saber se servidor está online
                API_V2.MapUtilsEndpoints();

                //Adiciona endpoints relacionados a autenticação, como login, registro, refresh token, etc
                API_V2.MapAuthEndpoints();

                //Adiciona endpoints relacionados a autenticação via web, como login, logout, etc
                API_V2.MapWebAuthEndpoints();

                //Adiciona endpoints relacionados a assistente de IA, como chat, etc
                API_V2.MapAiAssistantEndpoints();

                //Adiciona endpoints relacionados a administração de usuários, como banir, promover, etc
                API_V2.MapUserAdminEndpoints();

                //Adiciona endpoints relacionados a administração de bônus, como criar, editar, excluir, etc
                API_V2.MapBonusAdminEndpoints();

                //Adiciona endpoints relacionados a administração de tarefas, como criar, editar, excluir, etc
                API_V2.MapUsuariosEndpoints();

                //Adiciona endpoints relacionados a notificações, como criar, editar, excluir, etc
                API_V2.MapNotificationsEndpoints();

                //Adiciona endpoints relacionados a tarefas, como criar, editar, excluir, etc
                API_V2.MapTasksEndpoints();

                //Adiciona endpoints relacionados a metas, como criar, editar, excluir, etc
                API_V2.MapGoalsEndpoints();

                //Adiciona endpoints relacionados a notas, como criar, editar, excluir, etc
                API_V2.MapNotesEndpoints();

                //Adiciona endpoints relacionados a leituras, como criar, editar, excluir, etc
                API_V2.MapReadingsEndpoints();

                //Adiciona endpoints relacionados a listas de compras, como criar, editar, excluir, etc
                API_V2.MapShoppingListsEndpoints();

                //Adiciona endpoints relacionados a finanças, como criar, editar, excluir, etc
                API_V2.MapFinancesEndpoints();

                //Adiciona endpoints relacionados a dashboard, como gráficos, etc
                API_V2.MapDashboardEndpoints();

                //Adiciona endpoints relacionados a administração de banco de dados, como backup, restore, etc
                API_V2.MapDatabaseAdminEndpoints();

                //Adiciona endpoints relacionados a administração de logs, como visualizar, excluir, etc
                API_V2.MapApplicationLogAdminEndpoints();

                //Adiciona endpoints relacionados a administração de auditoria, como visualizar, excluir, etc
                API_V2.MapOperationAuditAdminEndpoints();

                //Adiciona endpoints relacionados a administração de notificações, como visualizar, excluir, etc
                API_V2.MapAdminNotificationEndpoints();
            }
        }
    }
}
