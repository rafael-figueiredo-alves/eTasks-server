using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Notifications;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    /// <summary>
    /// Classe que representa uma mensagem de notificação a ser enviada aos usuários. Contém informações como título, corpo, tipo de destinatário e URL de ação. Cada mensagem pode ter múltiplos destinatários associados a ela.
    /// </summary>
    public class NotificationMessage : IEntityModelConfiguration<NotificationMessage>
    {
        /// <summary>
        /// Identificador único da mensagem de notificação, gerado usando o método CreateVersion7 para garantir unicidade e ordenação temporal. Este ID é a chave primária da entidade e é utilizado para referenciar a mensagem em outras partes do sistema, como na associação com os destinatários.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário que criou a mensagem de notificação. Este campo é opcional, pois nem todas as mensagens podem ser associadas a um usuário específico (por exemplo, mensagens automáticas do sistema). Se presente, este ID pode ser usado para rastrear a origem da mensagem e para fins de auditoria.
        /// </summary>
        public Guid? CreatedByUserUid { get; set; }
        /// <summary>
        /// Identificador do grupo de destinatários para o qual a mensagem de notificação deve ser enviada. Este campo é opcional e pode ser usado para categorizar mensagens destinadas a grupos específicos de usuários, como administradores, membros de um projeto ou todos os usuários. O tipo de destinatário é definido pela propriedade TargetType, que indica se a mensagem deve ser enviada para um grupo específico ou para todos os usuários.
        /// </summary>
        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.All;
        /// <summary>
        /// Título da mensagem de notificação, que deve ser conciso e informativo. O título é uma parte importante da mensagem, pois é a primeira coisa que os destinatários verão ao receber a notificação. Ele deve resumir o conteúdo da mensagem e chamar a atenção dos usuários para que eles se interessem em ler o corpo da notificação. O título tem um limite de 120 caracteres para garantir que seja exibido corretamente em diferentes dispositivos e interfaces de usuário.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Corpo da mensagem de notificação, que contém os detalhes e informações adicionais que os destinatários precisam saber. O corpo deve ser claro e direto ao ponto, fornecendo todas as informações necessárias para que os usuários entendam o propósito da notificação e possam tomar as ações apropriadas, se necessário. O corpo tem um limite de 500 caracteres para garantir que seja legível e não sobrecarregue os destinatários com informações excessivas.
        /// </summary>
        public string Body { get; set; } = string.Empty;
        /// <summary>
        /// URL de ação associada à mensagem de notificação, que pode ser usada para direcionar os destinatários a uma página específica ou recurso relacionado à notificação. Esta URL é opcional e pode ser incluída quando a notificação requer que os usuários realizem uma ação específica, como visualizar um relatório, acessar um projeto ou responder a uma solicitação. Se presente, a URL deve ser válida e levar os usuários a um destino relevante para o conteúdo da notificação.
        /// </summary>
        public string? ActionUrl { get; set; }
        /// <summary>
        /// Campo opcional para armazenar dados adicionais relacionados à mensagem de notificação em formato JSON. Este campo pode ser usado para incluir informações estruturadas que não se encaixam diretamente nos outros campos da entidade, como detalhes específicos do evento que gerou a notificação, parâmetros para personalização da mensagem ou qualquer outro dado relevante que possa ser necessário para processar ou exibir a notificação de maneira adequada. O uso deste campo é flexível e pode variar dependendo das necessidades específicas do sistema e dos casos de uso das notificações.
        /// </summary>
        public string? DataJson { get; set; }
        /// <summary>
        /// Data de criação da mensagem de notificação, que é definida automaticamente no momento da criação da entidade usando a data e hora atual de São Paulo. Este campo é importante para rastrear quando a mensagem foi criada e para fins de ordenação e filtragem das notificações. A data de criação pode ser usada para exibir as notificações em ordem cronológica, para determinar a relevância das mensagens com base em sua idade ou para implementar políticas de expiração de notificações, se necessário.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Propriedade de navegação opcional para o usuário que criou a mensagem de notificação. Esta propriedade é do tipo User e pode ser usada para acessar informações adicionais sobre o criador da mensagem, como nome, email ou outras propriedades do usuário. A associação entre a mensagem de notificação e o usuário é feita por meio do campo CreatedByUserUid, que armazena o identificador do usuário. Se a mensagem não estiver associada a um usuário específico, esta propriedade pode ser nula.
        /// </summary>
        public User? CreatedByUser { get; set; }
        /// <summary>
        /// Lista de destinatários associados à mensagem de notificação. Cada destinatário é representado pela entidade NotificationRecipient, que contém informações sobre o usuário destinatário, o status da notificação (lida ou não lida) e a data em que a notificação foi lida, se aplicável. Esta coleção é importante para gerenciar a entrega das notificações aos usuários e para rastrear quais usuários receberam e interagiram com a mensagem. A associação entre a mensagem de notificação e os destinatários é feita por meio do campo NotificationMessageId na entidade NotificationRecipient, que armazena o identificador da mensagem à qual o destinatário está associado.
        /// </summary>
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();

        /// <summary>
        /// Configurações adicionais para a entidade NotificationMessage usando o ModelBuilder do Entity Framework Core. Este método é chamado durante a configuração do modelo de dados e é responsável por definir as regras de mapeamento entre a classe NotificationMessage e a tabela correspondente no banco de dados, bem como as relações entre as entidades. As configurações incluem a definição da chave primária, o mapeamento dos campos para colunas, as restrições de comprimento para os campos de texto e a configuração da relação entre NotificationMessage e NotificationRecipient, garantindo que as operações de exclusão em cascata sejam aplicadas corretamente.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationMessage>()
                .ToTable("notification_messages")
                .HasKey(x => x.Id);

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.TargetType)
                .HasConversion<int>();

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.Title)
                .HasMaxLength(120);

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.Body)
                .HasMaxLength(500);

            modelBuilder.Entity<NotificationMessage>()
                .HasMany(x => x.Recipients)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.NotificationMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
