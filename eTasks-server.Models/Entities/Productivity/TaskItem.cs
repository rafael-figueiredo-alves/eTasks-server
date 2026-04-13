using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Productivity
{
    /// <summary>
    /// Representa uma tarefa do usuário, simples ou gerada por recorrência.
    /// </summary>
    public class TaskItem : IEntityModelConfiguration<TaskItem>
    {
        /// <summary>
        /// Identificador único da tarefa.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono da tarefa.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Identificador da tarefa de origem quando a tarefa foi gerada por recorrência.
        /// </summary>
        public Guid? GeneratedFromTaskId { get; set; }
        /// <summary>
        /// Resumo curto da tarefa.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        /// <summary>
        /// Anotação complementar da tarefa.
        /// </summary>
        public string? Notes { get; set; }
        /// <summary>
        /// Nível de prioridade da tarefa.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        /// <summary>
        /// Data em que a tarefa deve aparecer para execução.
        /// </summary>
        public DateTime TaskDate { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Indica se a tarefa foi concluída.
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Data de conclusão da tarefa.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        /// <summary>
        /// Data de criação da tarefa.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização da tarefa.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuário dono da tarefa.
        /// </summary>
        public User? User { get; set; }
        /// <summary>
        /// Tarefa original que gerou esta instância.
        /// </summary>
        public TaskItem? GeneratedFromTask { get; set; }
        /// <summary>
        /// Tarefas geradas a partir desta tarefa.
        /// </summary>
        public ICollection<TaskItem> GeneratedTasks { get; set; } = new List<TaskItem>();
        /// <summary>
        /// Configuração de recorrência associada à tarefa.
        /// </summary>
        public TaskRecurrence? Recurrence { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de tarefas.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .ToTable("task_items")
                .HasKey(x => x.Id);

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Priority)
                .HasConversion<int>();

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.UserUid, x.TaskDate });

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.UserUid, x.IsCompleted });

            modelBuilder.Entity<TaskItem>()
                .HasOne(x => x.GeneratedFromTask)
                .WithMany(x => x.GeneratedTasks)
                .HasForeignKey(x => x.GeneratedFromTaskId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskItem>()
                .HasOne(x => x.Recurrence)
                .WithOne(x => x.TaskItem)
                .HasForeignKey<TaskRecurrence>(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
