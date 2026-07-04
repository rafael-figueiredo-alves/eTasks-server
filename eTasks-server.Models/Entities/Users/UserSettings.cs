using eTasks_server.Models.Enums.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Representa as preferencias do usuario na aplicacao.
    /// </summary>
    public class UserSettings : IEntityModelConfiguration<UserSettings>
    {
        /// <summary>
        /// Identificador unico das configuracoes.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuario dono das configuracoes.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Tema visual preferido.
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma preferido.
        /// </summary>
        public string Language { get; set; } = "pt-BR";

        /// <summary>
        /// Tela inicial preferida ao abrir o aplicativo.
        /// </summary>
        public AppStartScreen InitialScreen { get; set; } = AppStartScreen.Home;

        /// <summary>
        /// Indica se o sistema de bonus esta habilitado.
        /// </summary>
        public bool EnableBonusSystem { get; set; }

        /// <summary>
        /// Data de criacao das configuracoes.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data da ultima atualizacao das configuracoes.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Usuario associado a estas configuracoes.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade no Entity Framework Core.
        /// </summary>
        /// <param name="modelBuilder">Construtor do modelo.</param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSettings>()
                .ToTable("user_settings")
                .HasKey(x => x.Id);

            modelBuilder.Entity<UserSettings>()
                .HasIndex(x => x.UserUid)
                .IsUnique();

            modelBuilder.Entity<UserSettings>()
                .Property(x => x.InitialScreen)
                .HasConversion<int>();
        }
    }
}
