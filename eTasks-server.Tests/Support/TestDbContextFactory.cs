using eTasks_server.Core.Data;
using eTasks_server.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace eTasks_server.Tests.Support
{
    /// <summary>
    /// Fábrica de contextos de base de datos para provas unitárias.
    /// </summary>
    internal static class TestDbContextFactory
    {
        /// <summary>
        /// Cria o contexto de banco de dados em memória para testes unitários.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static AppDbContext Create(string databaseName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Cria o contexto de banco de dados relacional para testes unitários.
        /// </summary>
        /// <returns></returns>
        public static AppDbContext CreateRelationalModelContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(
                    "Server=localhost;Database=etasks_tests;User=root;Password=test;GuidFormat=TimeSwapBinary16",
                    new MySqlServerVersion(new Version(8, 0, 36)),
                    mySqlOptions => mySqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore))
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Cria um usuário ativo para testes unitários.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
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
