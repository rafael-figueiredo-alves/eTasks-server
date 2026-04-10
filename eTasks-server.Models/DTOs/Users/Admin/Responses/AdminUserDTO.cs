using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Admin.Responses
{
    /// <summary>
    /// Entidade de resposta que representa as informações detalhadas de um usuário, incluindo seu nome, email, status de confirmação, bloqueio e exclusão, além de informações sobre a data de criação e último acesso. Esta entidade é utilizada para fornecer uma visão abrangente do perfil do usuário em operações administrativas.
    /// </summary>
    public class AdminUserDTO
    {
        /// <summary>
        /// Identificação única do usuário, representada por um GUID (Globally Unique Identifier). Este campo é utilizado para diferenciar cada usuário de forma única dentro do sistema, garantindo que cada usuário possa ser identificado de maneira precisa e segura.
        /// </summary>
        public Guid Uid { get; set; }

        /// <summary>
        /// Nome de exibição do usuário, que deve conter entre 3 e 30 caracteres. Este campo é obrigatório e é utilizado para identificar o usuário de forma amigável dentro do sistema, permitindo que outros usuários reconheçam facilmente quem é o proprietário da conta. O nome do usuário deve ser único para evitar confusões e garantir uma experiência de usuário consistente.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário, que deve ser um endereço de e-mail válido. Este campo é obrigatório e é utilizado para comunicação com o usuário, bem como para autenticação e recuperação de senha. O formato do e-mail é validado para garantir que seja um endereço de e-mail legítimo, evitando erros de digitação e garantindo a integridade dos dados do usuário.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Caminho para a foto do usuário, que pode ser nulo. Este campo é opcional e pode ser utilizado para armazenar a localização da imagem de perfil do usuário, permitindo que ele personalize sua conta com uma foto. O caminho deve ser válido e acessível para garantir que a imagem possa ser exibida corretamente no sistema.
        /// </summary>
        public string? PhotoPath { get; set; }

        /// <summary>
        /// Indicador se conta se encontra confirmada ou não. Este campo é utilizado para determinar se o usuário completou o processo de confirmação de conta, que pode incluir a verificação do endereço de e-mail ou outras formas de validação. Um usuário confirmado tem acesso total às funcionalidades do sistema, enquanto um usuário não confirmado pode ter acesso limitado ou restrito até que a confirmação seja concluída.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// /Indicador se a conta do usuário está bloqueada ou não. Este campo é utilizado para determinar se o usuário tem acesso ao sistema ou se foi temporariamente impedido de acessar devido a violações de políticas, atividades suspeitas ou outras razões administrativas. Um usuário bloqueado não pode acessar sua conta ou utilizar as funcionalidades do sistema até que o bloqueio seja removido por um administrador.
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Indicador se a conta do usuário foi marcada como excluída ou não. Este campo é utilizado para determinar se o usuário foi removido do sistema, seja por solicitação do próprio usuário ou por ação administrativa. Um usuário marcado como excluído não tem acesso à sua conta e suas informações podem ser ocultadas ou removidas do sistema, dependendo das políticas de retenção de dados da organização.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Indicador da data e hora em que a conta do usuário foi criada. Este campo é utilizado para rastrear quando o usuário se registrou no sistema, permitindo que os administradores tenham uma visão histórica do perfil do usuário e possam monitorar a atividade ao longo do tempo. A data de criação é um dado importante para análises de uso e para entender o ciclo de vida do usuário dentro do sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indicador da data e hora do último acesso do usuário ao sistema. Este campo é utilizado para monitorar a atividade recente do usuário, permitindo que os administradores identifiquem padrões de uso, detectem inatividade ou atividades suspeitas. A data do último acesso é um dado valioso para análises de engajamento e para garantir a segurança da conta do usuário.
        /// </summary>
        public DateTime? LastAccessAt { get; set; }
    }
}
