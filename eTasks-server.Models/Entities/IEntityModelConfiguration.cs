using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities
{
    public interface IEntityModelConfiguration<TSelf>
        where TSelf : IEntityModelConfiguration<TSelf>
    {
        static abstract void Configure(ModelBuilder modelBuilder);
    }
}
