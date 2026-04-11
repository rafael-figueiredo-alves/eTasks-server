using eTasks_server.Core.Data;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public abstract class BaseBLL<TInterface> where TInterface : class
    {
        protected readonly AppDbContext _DbContext;
        protected readonly ILogger<TInterface> _Logger;

        protected BaseBLL(AppDbContext dbContext, ILogger<TInterface> logger)
        {
            _DbContext = dbContext;
            _Logger = logger;
        }
    }
}
