namespace eTasks_server.Extensions
{
    public static class ServicesPayload
    {
        extension(IServiceCollection services)
        {
            public void AddServicesPayload()
            {
                services.AddScoped<Core.BusinessLayers.VersionBLL>();
            }
        }
    }
}
