using eTasks_server.Core.Data;
using eTasks_server.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Tests.Support
{
    internal static class TestDbContextFactory
    {
        public static AppDbContext Create(string databaseName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        public static User CreateActiveUser(Guid? uid = null)
        {
            return new User
            {
                Uid = uid ?? Guid.CreateVersion7(),
                Name = "Test User",
                Email = $"{Guid.NewGuid():N}@example.com",
                PasswordHash = "hash",
                IsConfirmed = true,
                IsAdmin = false,
                IsBlocked = false,
                IsDeleted = false
            };
        }
    }
}
