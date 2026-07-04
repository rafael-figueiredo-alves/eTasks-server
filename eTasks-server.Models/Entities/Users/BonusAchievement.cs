using eTasks_server.Models.Enums.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Entidade responsável por representar uma conquista de bônus no sistema, que pode ser associada a um usuário e concedida com base em critérios específicos, como pontos acumulados ou ações realizadas. Cada conquista de bônus possui um código único, um nome descritivo, uma descrição opcional, a quantidade de pontos necessários para alcançá-la, um tipo de exibição (troféu ou medalha) e um status de atividade. A entidade também mantém um registro da data de criação e uma coleção de conquistas associadas aos usuários.
    /// </summary>
    public class BonusAchievement : IEntityModelConfiguration<BonusAchievement>
    {
        /// <summary>
        /// Identificador único da conquista de bônus, gerado automaticamente usando o método CreateVersion7 para garantir a unicidade e a ordenação temporal dos registros. Este identificador é utilizado como chave primária na tabela de conquistas de bônus e é essencial para a associação correta com os usuários e para a integridade dos dados no sistema.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Código único da conquista de bônus, utilizado para identificar e referenciar a conquista de forma consistente em todo o sistema. Este código é essencial para garantir a unicidade da conquista e facilitar a associação correta com os usuários, além de ser utilizado em consultas e operações relacionadas às conquistas de bônus. O código deve ser único para cada conquista e é uma parte fundamental da estrutura de dados do sistema.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nome/Título da conquista de bônus, que descreve de forma clara e concisa a realização ou o marco que o usuário alcançou ao conquistar essa conquista. O nome é uma parte importante da experiência do usuário, pois ajuda a identificar e valorizar as conquistas alcançadas, além de fornecer um contexto para os usuários entenderem o significado e a importância de cada conquista de bônus no sistema.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da conquista de bônus, que fornece informações adicionais sobre o que o usuário fez para conquistar essa conquista, os critérios específicos que foram atendidos ou qualquer outra informação relevante que ajude a contextualizar a conquista e a valorizar a realização do usuário. A descrição é opcional, mas pode ser útil para fornecer uma compreensão mais completa da conquista e para motivar os usuários a alcançarem mais conquistas de bônus no sistema.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Pontos necessários para alcançar a conquista de bônus, que indica a quantidade de pontos que o usuário precisa acumular ou as ações específicas que precisa realizar para conquistar essa conquista. Este valor é fundamental para definir os critérios de conquista e para motivar os usuários a se engajarem mais ativamente no sistema, buscando alcançar as conquistas de bônus e os benefícios associados a elas.
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// Tipo de exibição da conquista de bônus, que indica se a conquista é representada por um troféu ou uma medalha. Este tipo de exibição é importante para diferenciar visualmente as conquistas e para fornecer uma representação simbólica do significado e da importância de cada conquista de bônus no sistema. O tipo de exibição pode influenciar a percepção dos usuários sobre a conquista e pode ser utilizado para criar uma experiência mais envolvente e motivadora.
        /// </summary>
        public AchievementDisplayType DisplayType { get; set; } = AchievementDisplayType.Trophy;

        /// <summary>
        /// Indica se a conquista de bônus está ativa ou inativa no sistema. Uma conquista ativa é aquela que pode ser conquistada pelos usuários, enquanto uma conquista inativa é aquela que não está disponível para conquista, seja por motivos de manutenção, atualização ou descontinuação. Este status é importante para garantir que os usuários tenham acesso apenas às conquistas que estão atualmente disponíveis e para permitir a gestão eficiente das conquistas de bônus no sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data de criação da conquista de bônus, que indica quando a conquista foi criada no sistema. Esta informação é importante para fins de auditoria, histórico e para entender a evolução das conquistas de bônus ao longo do tempo. A data de criação é definida automaticamente usando a função SaoPauloDateTime.Now() para garantir que o registro seja criado com a data e hora corretas, considerando o fuso horário de São Paulo.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Coleção de conquistas associadas aos usuários, que representa a relação entre as conquistas de bônus e os usuários que as conquistaram. Cada item nesta coleção é uma instância da entidade UserAchievement, que contém informações sobre o usuário que conquistou a conquista de bônus, a data em que a conquista foi alcançada e outros detalhes relevantes. Esta coleção é fundamental para rastrear quais conquistas de bônus foram conquistadas por quais usuários e para fornecer uma experiência personalizada e motivadora para os usuários com base em suas conquistas no sistema.
        /// </summary>
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

        /// <summary>
        /// Método de configuração da entidade BonusAchievement para o Entity Framework Core, que define as regras de mapeamento entre a classe BonusAchievement e a tabela correspondente no banco de dados. Este método é responsável por configurar a tabela "bonus_achievements", definir a chave primária, configurar a conversão do tipo de exibição para um valor inteiro e garantir que o código da conquista seja único por meio de um índice. A configuração adequada da entidade é essencial para garantir a integridade dos dados e o desempenho das operações relacionadas às conquistas de bônus no sistema.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BonusAchievement>()
                                    .ToTable("bonus_achievements")
                                    .HasKey(x => x.Id);

            modelBuilder.Entity<BonusAchievement>()
                                    .Property(x => x.DisplayType)
                                    .HasConversion<int>();

            modelBuilder.Entity<BonusAchievement>()
                                    .HasIndex(x => x.Code)
                                    .IsUnique();
        }
    }
}
