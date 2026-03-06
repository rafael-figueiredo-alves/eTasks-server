using eTasks_server.Endpoints;
using eTasks_server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();

app.RegisterMiddlewares();

await app.AddAPIEndpoints();

app.MapResourcesEndpoints();

app.Run();
