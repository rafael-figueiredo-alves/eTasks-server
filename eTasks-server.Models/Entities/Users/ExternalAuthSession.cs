using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Registro de sessão de autenticação externa, utilizado para rastrear o processo de autenticação com provedores externos (Google, Facebook, etc.).
    /// </summary>
    public class ExternalAuthSession : IEntityModelConfiguration<ExternalAuthSession>
    {
        /// <summary>
        /// Identificador único da sessão de autenticação externa, gerado como um GUID versão 7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Código de sessão único, utilizado para correlacionar as etapas do processo de autenticação externa. Gerado como um GUID versão 7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid SessionCode { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Provedor de autenticação externa (ex: "Google", "Facebook", "GitHub", etc.) com o qual o usuário está tentando se autenticar.
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Informações do agente do usuário (user agent) do cliente que iniciou a sessão de autenticação externa, utilizado para fins de auditoria e segurança.
        /// </summary>
        public string ClientUserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único da instância do cliente que iniciou a sessão de autenticação externa, utilizado para rastrear sessões em dispositivos móveis ou navegadores específicos.
        /// </summary>
        public string ClientInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Código fixo de estado, utilizado para validar a integridade e a sequência das etapas do processo de autenticação externa.
        /// </summary>
        public string FixedStateCode { get; set; } = string.Empty;

        /// <summary>
        /// Status atual da sessão de autenticação externa, representando o estágio do processo de autenticação. Os valores possíveis são:
        /// </summary>
        public string Status { get; set; } = ExternalAuthSessionStatus.Pending;

        /// <summary>
        /// Código de erro específico retornado pelo provedor de autenticação externa em caso de falha na autenticação, utilizado para diagnóstico e tratamento de erros. Pode ser nulo se a autenticação for bem-sucedida ou se o erro não fornecer um código específico.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Descrição detalhada do erro retornado pelo provedor de autenticação externa em caso de falha na autenticação, utilizado para diagnóstico e tratamento de erros. Pode ser nulo se a autenticação for bem-sucedida ou se o erro não fornecer uma descrição específica.
        /// </summary>
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// Dados protegidos da resposta de login, utilizados para armazenar informações sensíveis durante o processo de autenticação externa.
        /// </summary>
        public string? ProtectedLoginResponseJson { get; set; }

        /// <summary>
        /// Data e hora de expiração da sessão de autenticação externa, utilizada para invalidar automaticamente sessões que não foram concluídas dentro do tempo limite.
        /// </summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

        /// <summary>
        /// Data e hora de conclusão da sessão de autenticação externa, utilizada para rastrear quando o processo de autenticação foi finalizado, seja com sucesso ou falha. Pode ser nulo se a sessão ainda estiver em andamento.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Data e hora de consumo da sessão de autenticação externa, utilizada para rastrear quando a sessão foi consumida para autenticar o usuário no sistema, garantindo que sessões não sejam reutilizadas. Pode ser nulo se a sessão ainda não tiver sido consumida.
        /// </summary>
        public DateTime? ConsumedAt { get; set; }

        /// <summary>
        /// Data e hora de criação da sessão de autenticação externa, utilizada para rastrear quando a sessão foi iniciada. Definida automaticamente no momento da criação da entidade, utilizando a data e hora atual no fuso horário de São Paulo para garantir consistência temporal.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Método de configuração do modelo para a entidade ExternalAuthSession, utilizado para definir as propriedades da tabela, chaves primárias, índices e outras configurações relacionadas ao mapeamento da entidade para o banco de dados.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalAuthSession>()
                .ToTable("external_auth_sessions")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => x.SessionCode)
                .IsUnique();

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => new { x.Provider, x.ClientUserAgent, x.ClientInstanceId });

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => new { x.Status, x.ExpiresAt });
        }
    }

    /// <summary>
    /// Constantes de status para a sessão de autenticação externa, representando os diferentes estágios do processo de autenticação. Os valores possíveis são:
    /// </summary>
    public static class ExternalAuthSessionStatus
    {
        /// <summary>
        /// Status inicial da sessão de autenticação externa, indicando que o processo de autenticação foi iniciado, mas ainda não foi concluído. Nesse estado, a sessão está aguardando a resposta do provedor de autenticação externa ou a conclusão das etapas necessárias para autenticar o usuário.
        /// </summary>
        public const string Pending = "Pending";

        /// <summary>
        /// Status indicando que a sessão de autenticação externa foi concluída com sucesso, ou seja, o usuário foi autenticado corretamente pelo provedor externo e a sessão está pronta para ser consumida para autenticar o usuário no sistema. Nesse estado, a propriedade CompletedAt deve estar preenchida com a data e hora da conclusão da sessão.
        /// </summary>
        public const string Success = "Success";

        /// <summary>
        /// Status indicando que a sessão de autenticação externa falhou, ou seja, ocorreu um erro durante o processo de autenticação com o provedor externo, impedindo que o usuário fosse autenticado. Nesse estado, as propriedades ErrorCode e ErrorDescription podem conter informações adicionais sobre o motivo da falha, e a propriedade CompletedAt deve estar preenchida com a data e hora da conclusão da sessão.
        /// </summary>
        public const string Failed = "Failed";

        /// <summary>
        /// Status indicando que a sessão de autenticação externa foi consumida para autenticar o usuário no sistema, ou seja, a sessão foi utilizada para concluir o processo de autenticação e autenticar o usuário com sucesso. Nesse estado, a propriedade ConsumedAt deve estar preenchida com a data e hora do consumo da sessão, garantindo que a sessão não seja reutilizada para autenticar o mesmo ou outro usuário.
        /// </summary>
        public const string Consumed = "Consumed";
    }
}
