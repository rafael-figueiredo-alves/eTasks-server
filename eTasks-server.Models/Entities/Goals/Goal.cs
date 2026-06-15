using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Goals
{
    /// <summary>
    /// Representa uma meta ou objetivo definido pelo usuario.
    /// </summary>
    public class Goal : IEntityModelConfiguration<Goal>
    {
        /// <summary>
        /// Identificador único da meta, gerado usando o algoritmo UUID versão 7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário ao qual a meta pertence, referenciando a entidade User. Utiliza o mesmo formato UUID para garantir consistência e facilidade de associação entre as entidades.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Resumo do objetivo/meta, fornecendo uma descrição breve e concisa do que se trata a meta. Este campo é obrigatório e deve conter um texto significativo para que o usuário possa identificar facilmente o propósito da meta.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        /// <summary>
        /// Descrição detalhada do objetivo/meta, permitindo que o usuário forneça informações adicionais, contexto ou detalhes específicos relacionados à meta. Este campo é opcional e pode ser utilizado para esclarecer o que a meta envolve ou para registrar informações relevantes que possam ajudar na realização da meta.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Tipo da meta, indicando a categoria ou natureza do objetivo. O tipo pode ser utilizado para classificar as metas em diferentes categorias, como pessoal, profissional, saúde, finanças, entre outros. Isso permite que os usuários organizem suas metas de acordo com suas áreas de interesse ou prioridades.
        /// </summary>
        public GoalType Type { get; set; } = GoalType.Personal;
        /// <summary>
        /// Prioridade da meta, indicando a importância ou urgência do objetivo. A prioridade pode ser utilizada para ajudar os usuários a focarem nas metas mais importantes ou urgentes, permitindo que eles organizem suas tarefas e esforços de acordo com a prioridade atribuída a cada meta.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        /// <summary>
        /// Pontos de recompensa associados à meta, representando um sistema de gamificação onde os usuários podem acumular pontos ao alcançar suas metas. Esses pontos podem ser utilizados para motivar os usuários a se empenharem na realização de suas metas, oferecendo recompensas ou incentivos com base no número de pontos acumulados. Este campo é opcional e pode ser utilizado para incentivar a participação ativa dos usuários na definição e alcance de suas metas.
        /// </summary>
        public int? RewardPoints { get; set; }
        /// <summary>
        /// Status da meta, indicando o estado atual do objetivo. O status pode ser utilizado para acompanhar o progresso das metas, permitindo que os usuários saibam se uma meta está ativa, concluída, em andamento ou cancelada. Isso ajuda os usuários a gerenciarem suas metas de forma mais eficaz, fornecendo uma visão clara do progresso e do estado de cada objetivo.
        /// </summary>
        public GoalStatus Status { get; set; } = GoalStatus.Active;
        /// <summary>
        /// Data de criação da meta, registrando o momento em que a meta foi definida ou criada pelo usuário. Esta informação é importante para acompanhar o histórico das metas e para fins de organização e análise do progresso ao longo do tempo. A data de criação é automaticamente definida no momento da criação da meta, utilizando a data e hora atual do sistema, garantindo que cada meta tenha um registro preciso de quando foi estabelecida.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data de atualização da meta, registrando o momento em que a meta foi modificada pela última vez. Esta informação é importante para acompanhar as alterações realizadas nas metas e para fins de organização e análise do progresso ao longo do tempo.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// Identificador de exclusão da meta, indicando se a meta foi marcada como excluída. Este campo é utilizado para implementar a funcionalidade de exclusão lógica, permitindo que as metas sejam "excluídas" sem serem removidas fisicamente do banco de dados. Isso é útil para manter um histórico completo das metas e para possibilitar a recuperação de metas excluídas, caso necessário. Quando uma meta é marcada como excluída, o campo IsDeleted é definido como true, e a data de exclusão é registrada no campo DeletedAt.
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Data de exclusão da meta, registrando o momento em que a meta foi marcada como excluída. Esta informação é importante para acompanhar o histórico das metas e para fins de organização e análise do progresso ao longo do tempo. A data de exclusão é automaticamente definida no momento em que a meta é marcada como excluída, utilizando a data e hora atual do sistema, garantindo que cada meta tenha um registro preciso de quando foi excluída.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
        /// <summary>
        /// Usuário associado à meta, representando a relação entre a meta e o usuário ao qual ela pertence. Este campo é utilizado para estabelecer uma associação entre as entidades Goal e User, permitindo que cada meta seja vinculada a um usuário específico. A propriedade User é do tipo User? (nullable), indicando que uma meta pode ou não estar associada a um usuário, dependendo do contexto de uso. Essa associação é importante para garantir que as metas sejam corretamente atribuídas aos usuários e para facilitar a consulta e o gerenciamento das metas com base no usuário associado.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Método configurador para a entidade Goal, utilizado para definir as configurações de mapeamento e relacionamento da entidade com o banco de dados. Este método é chamado durante a configuração do modelo no contexto do Entity Framework, permitindo que as propriedades da entidade sejam mapeadas corretamente para as colunas do banco de dados, e que os relacionamentos entre as entidades sejam estabelecidos de acordo com as necessidades do aplicativo. No caso da entidade Goal, o método Configure define a tabela "goals", a chave primária, as conversões de enumeração para os campos Status, Type e Priority, e os índices para otimizar consultas baseadas em UserUid e Status ou IsDeleted.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>()
                .ToTable("goals")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Goal>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Type)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Priority)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .HasIndex(x => new { x.UserUid, x.Status });

            modelBuilder.Entity<Goal>()
                .HasIndex(x => new { x.UserUid, x.IsDeleted });
        }
    }
}
