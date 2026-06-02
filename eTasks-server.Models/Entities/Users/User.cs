using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Notes;
using eTasks_server.Models.Entities.Notifications;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Readings;
using eTasks_server.Models.Entities.Shopping;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Entidade principal para representar os usuários do sistema, contendo informações essenciais como nome, email, senha e status de confirmação. Esta entidade também mantém relacionamentos com outras entidades relacionadas à autenticação, configurações do usuário, pontos de bônus e conquistas, permitindo uma gestão completa do perfil do usuário dentro da aplicação.
    /// </summary>
    public class User : IEntityModelConfiguration<User>
    {
        /// <summary>
        /// Identificador único do usuário, gerado como um GUID versão 7 para garantir unicidade e ordenação temporal. Este campo é a chave primária da entidade e é utilizado para referenciar o usuário em outras partes do sistema, como tokens de autenticação, códigos de redefinição de senha e logs de login.
        /// </summary>
        public Guid Uid { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Nome de exibição do usuário, utilizado para identificar o usuário dentro da aplicação. Este campo é obrigatório e deve conter entre 3 e 30 caracteres, garantindo que o nome seja suficientemente descritivo sem ser excessivamente longo. O nome do usuário é uma parte fundamental do perfil do usuário e é exibido em várias partes da interface do usuário, como na barra de navegação, nas mensagens e nas listas de usuários.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuário precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuário não pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// E-mail do usuário, utilizado para autenticação e comunicação. Este campo é obrigatório e deve ser um endereço de e-mail válido, garantindo que o sistema possa enviar notificações, códigos de redefinição de senha e outras comunicações importantes para o usuário. O e-mail também é utilizado como identificador único para login, permitindo que os usuários acessem suas contas de forma segura.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Somente endereços de e-mail válidos são aceitos")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha encriptografada do usuário, armazenada como um hash seguro para garantir a proteção dos dados de autenticação. Este campo é obrigatório e deve ser gerado utilizando um algoritmo de hash robusto, como bcrypt ou Argon2, para proteger contra ataques de força bruta e vazamentos de dados. O sistema nunca deve armazenar senhas em texto simples, garantindo a segurança dos usuários mesmo em caso de comprometimento do banco de dados.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// CAmninho da foto do usuário, utilizado para armazenar a URL ou o caminho local da imagem de perfil do usuário. Este campo é opcional e pode ser utilizado para personalizar a experiência do usuário dentro da aplicação, permitindo que os usuários escolham uma imagem de perfil que os represente. O sistema deve garantir que as imagens sejam armazenadas de forma segura e que apenas arquivos de imagem válidos sejam aceitos para evitar vulnerabilidades de segurança.
        /// </summary>
        public string? PhotoPath { get; set; }

        /// <summary>
        /// Identificador se conta foi confirmada, utilizado para indicar se o usuário completou o processo de confirmação de conta, como a verificação de e-mail. Este campo é importante para garantir que apenas usuários legítimos tenham acesso completo às funcionalidades do sistema, protegendo contra contas falsas e garantindo a integridade da base de usuários. O sistema deve implementar um processo de confirmação robusto, enviando um link de verificação para o e-mail do usuário e atualizando este campo somente após a confirmação bem-sucedida.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Identificador se o usuário é um administrador, utilizado para conceder privilégios administrativos dentro da aplicação. Este campo é importante para controlar o acesso a funcionalidades sensíveis e garantir que apenas usuários autorizados possam realizar ações administrativas, como gerenciar outros usuários, configurar o sistema e acessar dados confidenciais. O sistema deve implementar uma lógica de autorização robusta, verificando este campo antes de permitir o acesso a áreas restritas da aplicação.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Identificador se o usuário está bloqueado, utilizado para impedir que usuários que violaram as políticas do sistema ou apresentaram comportamento inadequado tenham acesso à aplicação. Este campo é importante para manter a segurança e a integridade da comunidade de usuários, permitindo que os administradores bloqueiem contas problemáticas e protejam outros usuários de interações indesejadas. O sistema deve implementar uma lógica de bloqueio robusta, verificando este campo antes de permitir o acesso à aplicação e fornecendo feedback adequado ao usuário bloqueado.
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Data de criação da conta do usuário, utilizada para rastrear quando o usuário se registrou no sistema. Este campo é importante para análises de crescimento de usuários, monitoramento de atividades e para implementar funcionalidades baseadas em tempo, como expiração de tokens ou promoções especiais para usuários antigos. O sistema deve definir este campo automaticamente no momento do registro do usuário, garantindo que a data seja precisa e consistente.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data do último acesso do usuário, utilizada para monitorar a atividade do usuário e implementar funcionalidades baseadas em tempo, como expiração de sessões ou notificações de inatividade. Este campo é importante para garantir a segurança da conta do usuário, permitindo que o sistema detecte atividades suspeitas e tome medidas apropriadas, como solicitar uma nova autenticação ou bloquear a conta temporariamente. O sistema deve atualizar este campo automaticamente sempre que o usuário fizer login ou realizar ações significativas dentro da aplicação.
        /// </summary>
        public DateTime? LastAccessAt { get; set; }

        /// <summary>
        /// Marcador se conta foi removida, utilizado para implementar uma exclusão lógica de usuários, permitindo que os dados do usuário sejam mantidos no banco de dados para fins de auditoria ou recuperação, mas impedindo que o usuário tenha acesso à aplicação. Este campo é importante para garantir a integridade dos dados e permitir a recuperação de contas em caso de exclusão acidental, além de fornecer uma maneira de manter um histórico completo das atividades do usuário mesmo após a exclusão. O sistema deve verificar este campo antes de permitir o acesso à aplicação e fornecer feedback adequado ao usuário excluído.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Data da marcação de exclusão da conta do usuário, utilizada para rastrear quando o usuário foi marcado como excluído. Este campo é importante para análises de retenção de usuários, monitoramento de atividades e para implementar funcionalidades baseadas em tempo, como a exclusão permanente de contas após um período de tempo ou a recuperação de contas dentro de um período de carência. O sistema deve definir este campo automaticamente no momento em que o usuário for marcado como excluído, garantindo que a data seja precisa e consistente.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        #region Propriedades de navegação
        /// <summary>
        /// Relação de RefreshTokens de seções do usuário
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserExternalLogin> ExternalLogins { get; set; } = new List<UserExternalLogin>();
        /// <summary>
        /// Lista de códigos para troca de senhas
        /// </summary>
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = new List<PasswordResetCode>();
        public ICollection<AccountReactivationCode> AccountReactivationCodes { get; set; } = new List<AccountReactivationCode>();
        /// <summary>
        /// Acesso ao log de logins do usuário
        /// </summary>
        public ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
        /// <summary>
        /// Configurações do usuário
        /// </summary>
        public UserSettings? Settings { get; set; }
        /// <summary>
        /// Pontos ganhos pelo usuário, utilizados para recompensar a participação e o engajamento dentro da aplicação. Esta coleção permite que o sistema rastreie os pontos acumulados por cada usuário, possibilitando a implementação de funcionalidades como níveis de usuário, recompensas exclusivas e competições entre usuários. O sistema deve garantir que os pontos sejam concedidos de forma justa e transparente, incentivando a participação ativa dos usuários e promovendo um ambiente de comunidade saudável.
        /// </summary>
        public ICollection<UserBonusPoint> BonusPoints { get; set; } = new List<UserBonusPoint>();
        /// <summary>
        /// Conquistas alcançadas pelo usuário, utilizadas para reconhecer e recompensar marcos importantes na jornada do usuário dentro da aplicação. Esta coleção permite que o sistema rastreie as conquistas de cada usuário, possibilitando a implementação de funcionalidades como badges, níveis de conquista e reconhecimento público dentro da comunidade. O sistema deve garantir que as conquistas sejam concedidas de forma justa e transparente, incentivando a participação ativa dos usuários e promovendo um ambiente de comunidade saudável.
        /// </summary>
        public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
        /// <summary>
        /// Tarefas pertencentes ao usuário.
        /// </summary>
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        /// <summary>
        /// Metas cadastradas pelo usuário.
        /// </summary>
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
        /// <summary>
        /// Listas de compras do usuário.
        /// </summary>
        public ICollection<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
        /// <summary>
        /// Anotações registradas pelo usuário.
        /// </summary>
        public ICollection<NoteItem> Notes { get; set; } = new List<NoteItem>();
        /// <summary>
        /// Leituras registradas pelo usuário.
        /// </summary>
        public ICollection<ReadingItem> Readings { get; set; } = new List<ReadingItem>();
        /// <summary>
        /// Lançamentos financeiros do usuário.
        /// </summary>
        public ICollection<FinanceEntry> FinanceEntries { get; set; } = new List<FinanceEntry>();
        public ICollection<PushDeviceRegistration> PushDeviceRegistrations { get; set; } = new List<PushDeviceRegistration>();
        public ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
        #endregion

        /// <summary>
        /// Método para configurar a entidade User no Entity Framework, definindo as chaves primárias, índices e relacionamentos com outras entidades. Este método é essencial para garantir que a estrutura do banco de dados seja corretamente mapeada para a classe User, permitindo que o Entity Framework gerencie as operações de CRUD de forma eficiente e segura. A configuração inclui a definição da chave primária como o campo Uid, a criação de um índice único para o campo Email para garantir que não haja duplicatas, e a configuração dos relacionamentos com as entidades RefreshToken, PasswordResetCode, LoginLog, UserSettings, UserBonusPoint e UserAchievement, garantindo que as operações de exclusão em cascata sejam aplicadas corretamente para manter a integridade referencial do banco de dados.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                                .ToTable("users")
                                .HasKey(x => x.Uid);

            modelBuilder.Entity<User>()
                                .HasIndex(x => x.Email)
                                .IsUnique();

            modelBuilder.Entity<User>()
                                .HasMany(x => x.RefreshTokens)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.ExternalLogins)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.PasswordResetCodes)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.AccountReactivationCodes)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.LoginLogs)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasOne(x => x.Settings)
                                .WithOne(x => x.User)
                                .HasForeignKey<UserSettings>(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.BonusPoints)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.Achievements)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.Tasks)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.Goals)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.ShoppingLists)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.Notes)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.Readings)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.FinanceEntries)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.PushDeviceRegistrations)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                                .HasMany(x => x.NotificationRecipients)
                                .WithOne(x => x.User)
                                .HasForeignKey(x => x.UserUid)
                                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
