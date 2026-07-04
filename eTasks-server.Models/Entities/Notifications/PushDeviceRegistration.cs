using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Notifications;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    /// <summary>
    /// Registro de Push
    /// </summary>
    public class PushDeviceRegistration : IEntityModelConfiguration<PushDeviceRegistration>
    {
        /// <summary>
        /// Identificação
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador de usuário
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Plataforma
        /// </summary>
        public PushDevicePlatform Platform { get; set; } = PushDevicePlatform.Other;

        /// <summary>
        /// Identificador de dispositivo
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Nome do dispositivo
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Token
        /// </summary>
        public string? PushToken { get; set; }

        /// <summary>
        /// Endpoint
        /// </summary>
        public string? PushEndpoint { get; set; }

        /// <summary>
        /// Criptografia
        /// </summary>
        public string? P256dh { get; set; }

        /// <summary>
        /// Autenticação
        /// </summary>
        public string? Auth { get; set; }

        /// <summary>
        /// Indica se está ativo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data/Hora de criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data/Hora atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data/Hora última visuãlização
        /// </summary>
        public DateTime LastSeenAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Usuário
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configurações
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PushDeviceRegistration>()
                .ToTable("push_device_registrations")
                .HasKey(x => x.Id);

            modelBuilder.Entity<PushDeviceRegistration>()
                .HasIndex(x => new { x.UserUid, x.Platform, x.DeviceId })
                .IsUnique();

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.Platform)
                .HasConversion<int>();

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.DeviceId)
                .HasMaxLength(150);

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.DisplayName)
                .HasMaxLength(120);
        }
    }
}
