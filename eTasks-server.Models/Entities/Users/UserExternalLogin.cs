using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Representa uma conta de login externo associada a um usuário. Permite que os usuários façam login usando provedores externos, como Google, Facebook, etc. Cada registro contém informações sobre o provedor de login, o ID do usuário no provedor e o email associado. A classe também inclui timestamps para rastrear quando a conta de login externo foi criada e atualizada pela última vez.
    /// </summary>
    public class UserExternalLogin : IEntityModelConfiguration<UserExternalLogin>
    {
        /// <summary>
        /// Identificador único para a conta de login externo. Utiliza o formato UUID versão 7 para garantir unicidade e ordenação temporal. Este ID é gerado automaticamente quando uma nova conta de login externo é criada.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuário associado a esta conta de login externo. Este campo é uma chave estrangeira que referencia o usuário principal na tabela de usuários. Ele é obrigatório para garantir que cada conta de login externo esteja vinculada a um usuário específico.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Provedor de login externo, como "Google", "Facebook", "GitHub", etc. Este campo é obrigatório para identificar qual serviço de autenticação externa está sendo utilizado pelo usuário. Ele é usado para diferenciar entre diferentes provedores de login e garantir que as credenciais sejam tratadas corretamente com base no provedor.
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do usuário no provedor de login externo. Este campo é obrigatório e deve ser único em combinação com o campo "Provider" para garantir que cada conta de login externo seja identificada de forma exclusiva. Ele armazena o ID fornecido pelo serviço de autenticação externa, permitindo que o sistema reconheça o usuário quando ele fizer login usando esse provedor.
        /// </summary>
        public string ProviderUserId { get; set; } = string.Empty;

        /// <summary>
        /// Email associado à conta de login externo. Este campo é obrigatório para garantir que haja um endereço de email válido vinculado à conta de login externo, o que pode ser usado para comunicação e recuperação de conta. Ele armazena o email fornecido pelo serviço de autenticação externa, permitindo que o sistema envie notificações ou mensagens relacionadas à conta de login externo, se necessário.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nome de exibição opcional para a conta de login externo. Este campo pode ser usado para armazenar um nome amigável ou apelido associado à conta de login externo, facilitando a identificação do usuário ao exibir informações relacionadas à conta de login externo. Ele é opcional, pois nem todos os provedores de login externo fornecem um nome de exibição, mas pode ser útil para melhorar a experiência do usuário ao interagir com o sistema usando autenticação externa.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Data de criação da conta de login externo. Este campo é preenchido automaticamente com a data e hora atuais no momento em que a conta de login externo é criada. Ele é usado para rastrear quando a conta de login externo foi registrada no sistema, o que pode ser útil para fins de auditoria, análise de uso e gerenciamento de contas.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data da última atualização da conta de login externo. Este campo é atualizado automaticamente com a data e hora atuais sempre que a conta de login externo é modificada. Ele é usado para rastrear quando a conta de login externo foi atualizada pela última vez, o que pode ser útil para fins de auditoria, análise de uso e gerenciamento de contas, especialmente se as informações associadas à conta de login externo forem alteradas ao longo do tempo.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Referência ao usuário associado a esta conta de login externo. Este campo é uma propriedade de navegação que permite acessar as informações do usuário principal vinculado a esta conta de login externo. Ele é usado para estabelecer um relacionamento entre a conta de login externo e o usuário correspondente, facilitando a recuperação de informações do usuário ao trabalhar com autenticação externa e gerenciamento de contas.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configurações adicionais para o modelo UserExternalLogin, definindo a estrutura da tabela no banco de dados, chaves primárias, índices e relacionamentos. Este método é chamado durante a configuração do modelo no contexto do Entity Framework para garantir que a tabela "user_external_logins" seja criada corretamente com as restrições e relacionamentos necessários para manter a integridade dos dados e otimizar as consultas relacionadas às contas de login externo.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserExternalLogin>()
                .ToTable("user_external_logins")
                .HasKey(x => x.Id);

            modelBuilder.Entity<UserExternalLogin>()
                .HasIndex(x => new { x.Provider, x.ProviderUserId })
                .IsUnique();

            modelBuilder.Entity<UserExternalLogin>()
                .HasIndex(x => x.UserUid);
        }
    }
}
