using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Productivity
{
    /// <summary>
    /// Representa uma tarefa do usuario, simples ou gerada por recorrencia.
    /// </summary>
    public class TaskItem : IEntityModelConfiguration<TaskItem>
    {
        /// <summary>
        /// Identificador unico da tarefa.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuario dono da tarefa.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Identificador da tarefa de origem quando a tarefa foi gerada por recorrencia.
        /// </summary>
        public Guid? GeneratedFromTaskId { get; set; }

        /// <summary>
        /// Resumo curto da tarefa.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Anotacao complementar da tarefa.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Nivel de prioridade da tarefa.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>
        /// Data em que a tarefa deve aparecer para execucao.
        /// </summary>
        public DateTime TaskDate { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Indica se a tarefa foi concluida.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Data de conclusao da tarefa.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Data de criacao da tarefa.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data da ultima atualizacao da tarefa.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica se a tarefa foi removida logicamente.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Data da remocao logica da tarefa.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuario dono da tarefa.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Tarefa original que gerou esta instancia.
        /// </summary>
        public TaskItem? GeneratedFromTask { get; set; }

        /// <summary>
        /// Tarefas geradas a partir desta tarefa.
        /// </summary>
        public ICollection<TaskItem> GeneratedTasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// Configuracao de recorrencia associada a tarefa.
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
                .HasIndex(x => new { x.UserUid, x.IsDeleted });

            modelBuilder.Entity<TaskItem>()
                .HasIndex(x => new { x.UserUid, x.IsDeleted, x.TaskDate });

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
