using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Users
{
    /// <summary>
    /// Classe que representa os dados de um usuário administrador para fins de transferência de dados (DTO - Data Transfer Object).
    /// </summary>
    public class AdminUserDTO
    {
        /// <summary>
        /// Identificação única do usuário, representada por um GUID (Globally Unique Identifier). Este campo é utilizado para identificar de forma única cada usuário no sistema, permitindo que os administradores gerenciem e acessem as informações dos usuários de maneira eficiente e segura. O uso de GUIDs garante que cada usuário tenha um identificador exclusivo, mesmo em sistemas distribuídos ou em cenários onde múltiplos administradores possam estar gerenciando usuários simultaneamente.
        /// </summary>
        public Guid Uid { get; set; }
        /// <summary>
        /// Nome do usuário. Este campo é obrigatório e deve conter o nome completo do usuário. O nome é utilizado para identificar o usuário de forma amigável e pode ser exibido em interfaces de administração, relatórios ou outras áreas do sistema onde seja necessário mostrar informações sobre os usuários. A obrigatoriedade deste campo garante que cada usuário tenha um nome associado, facilitando a identificação e a comunicação entre administradores e usuários no sistema.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuário precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuário não pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// E-mail do usuário, utilizado para autenticação e comunicação. Este campo é obrigatório e deve ser um endereço de e-mail válido e único no sistema, pois será utilizado para identificar o usuário durante o processo de login e para enviar notificações ou mensagens relacionadas à conta do usuário. O email também pode ser utilizado para recuperação de senha e outras funcionalidades relacionadas à conta do usuário.
        /// </summary>
        /// <example>
        /// sac@etasks.com; fulano@outlook.com; beltrano.silva@bol.com.br
        /// </example> 
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Caminho do arquivo de imagem do usuário, utilizado para armazenar a foto de perfil do usuário. Este campo é opcional e pode conter o caminho para um arquivo de imagem (como JPEG, PNG, etc.) que representa a foto de perfil do usuário. O uso deste campo permite que os administradores associem uma imagem visual ao perfil do usuário, melhorando a experiência do usuário e facilitando a identificação visual dos usuários no sistema. Se o campo estiver vazio ou nulo, isso indica que o usuário não possui uma foto de perfil associada.
        /// </summary>
        public string? PhotoPath { get; set; }
        /// <summary>
        /// Identificador se o usuário é confirmado ou não. Este campo é utilizado para indicar se o usuário confirmou sua conta, geralmente por meio de um processo de verificação de e-mail ou outro método de confirmação. Se o valor for verdadeiro (true), isso indica que o usuário confirmou sua conta e pode acessar as funcionalidades do sistema. Se o valor for falso (false), isso indica que o usuário ainda não confirmou sua conta, o que pode restringir o acesso a certas funcionalidades ou exigir que o usuário complete o processo de confirmação antes de poder utilizar plenamente o sistema.
        /// </summary>
        public bool IsConfirmed { get; set; }
        /// <summary>
        /// Identificador se o usuário está bloqueado ou não. Este campo é utilizado para indicar se o usuário está bloqueado, o que pode ocorrer por diversos motivos, como violações de políticas de uso, atividades suspeitas ou solicitações de bloqueio por parte do próprio usuário. Se o valor for verdadeiro (true), isso indica que o usuário está bloqueado e não pode acessar as funcionalidades do sistema. Se o valor for falso (false), isso indica que o usuário não está bloqueado e pode acessar as funcionalidades do sistema normalmente.
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Data em que a conta do usuário foi criada. Este campo é utilizado para registrar o momento exato em que a conta do usuário foi criada no sistema, permitindo que os administradores monitorem e analisem a atividade dos usuários ao longo do tempo, identifiquem padrões de criação de contas e gerenciem as contas de maneira eficiente. A data de criação também pode ser útil para fins de auditoria e para determinar a antiguidade da conta do usuário no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Entindade que representa os dados de um log de login de um usuário para fins de transferência de dados (DTO - Data Transfer Object).
    /// </summary>
    public class UserLoginLogDTO
    {
        /// <summary>
        /// Identificação do log de login, representada por um GUID (Globally Unique Identifier). Este campo é utilizado para identificar de forma única cada registro de log de login no sistema.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Status do login, indicando se a tentativa de login foi bem-sucedida ou falhou. Este campo é utilizado para registrar o resultado da tentativa de login, permitindo que os administradores monitorem e analisem as atividades de login dos usuários.
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Endereço IP do usuário no momento da tentativa de login. Este campo é utilizado para registrar a origem da tentativa de login, o que pode ser útil para fins de segurança e auditoria, permitindo que os administradores identifiquem padrões suspeitos ou atividades maliciosas relacionadas a logins.
        /// </summary>
        public string? IpAddress { get; set; }
        /// <summary>
        /// Indicador do dispositivo utilizado pelo usuário para realizar a tentativa de login. Este campo é utilizado para registrar informações sobre o dispositivo, como tipo (desktop, mobile, tablet), sistema operacional e navegador, o que pode ajudar os administradores a entender melhor o contexto das tentativas de login e identificar possíveis atividades suspeitas relacionadas a dispositivos desconhecidos ou não autorizados.
        /// </summary>
        public string? UserAgent { get; set; }
        /// <summary>
        /// Data em que o registro de log de login foi criado. Este campo é utilizado para registrar o momento exato em que a tentativa de login ocorreu, permitindo que os administradores monitorem e analisem as atividades de login dos usuários ao longo do tempo, identificando padrões ou tendências relacionadas a logins bem-sucedidos ou falhos.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Classe que representa os dados necessários para um administrador definir uma nova senha para um usuário.
    /// </summary>
    public class AdminSetPasswordRequest
    {
        /// <summary>
        /// Nova senha a ser definida para o usuário. Deve conter entre 6 e 30 caracteres.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha não deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

