using eTasks_server.Endpoints;
using eTasks_server.Extensions;

//Constroi a aplicação e registra os serviços e dependências
var builder = WebApplication.CreateBuilder(args);

//Registra serviços e dependências
builder.RegisterServices();

var app = builder.Build();

//Registra os middlewares
app.RegisterMiddlewares();

//Adiciona os endpoints da API
await app.AddAPIEndpoints();

//Mapeia os endpoints de recursos da aplicação
app.MapResourcesEndpoints();

//Inicializa a aplicação
app.Run();
