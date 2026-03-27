using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Users
{
    /// <summary>
    /// Entidade que representa um usuário do sistema, contendo informações essenciais para autenticação, autorização e gerenciamento de conta.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único do usuário, gerado usando o método CreateVersion7 para garantir unicidade e ordenação temporal. Este campo é fundamental para a identificação do usuário em todo o sistema, permitindo operações como autenticação, autorização e associação com outras entidades (como tokens de atualização e códigos de redefinição de senha).
        /// </summary>
        public Guid Uid { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Nome do usuário, utilizado para exibição e identificação amigável. Este campo é importante para personalizar a experiência do usuário e facilitar a comunicação dentro do sistema. Ele deve ser único ou acompanhado de outras informações (como email) para evitar confusões entre usuários com nomes semelhantes.
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
        /// Senha encriptografada do usuário, armazenada como um hash seguro para garantir a proteção dos dados de autenticação. Este campo é essencial para a segurança do sistema, pois armazena a senha de forma que não possa ser facilmente recuperada ou comprometida em caso de acesso não autorizado ao banco de dados. O uso de hashing e salting é recomendado para proteger as senhas contra ataques de força bruta e outras formas de comprometimento de segurança.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
        /// <summary>
        /// Caminho para a foto de perfil do usuário, permitindo que ele personalize sua conta com uma imagem. Este campo é opcional e pode ser utilizado para melhorar a experiência do usuário, tornando a interface mais amigável e personalizada. O caminho pode ser um URL para uma imagem hospedada ou um caminho local no servidor, dependendo da implementação do sistema de armazenamento de arquivos.
        /// </summary>
        public string? PhotoPath { get; set; }
        /// <summary>
        /// Identifica se o usuário confirmou seu endereço de e-mail, o que é importante para garantir a autenticidade da conta e permitir o acesso a funcionalidades restritas. Este campo é fundamental para a segurança do sistema, pois ajuda a prevenir o uso de contas falsas ou não verificadas, garantindo que apenas usuários legítimos possam acessar certas áreas ou recursos do sistema. A confirmação de e-mail geralmente envolve o envio de um link de verificação para o endereço de e-mail fornecido pelo usuário durante o processo de registro.
        /// </summary>
        public bool IsConfirmed { get; set; } = false;
        /// <summary>
        /// Identifica se o usuário possui privilégios administrativos, permitindo que ele acesse funcionalidades e áreas restritas do sistema. Este campo é crucial para a implementação de controle de acesso e autorização, garantindo que apenas usuários com as permissões adequadas possam realizar ações sensíveis ou acessar informações confidenciais. Usuários com privilégios administrativos geralmente têm acesso a painéis de controle, gerenciamento de usuários e outras funcionalidades avançadas do sistema.
        /// </summary>
        public bool IsAdmin { get; set; } = false;
        /// <summary>
        /// Identifica se o usuário está bloqueado, o que impede que ele acesse o sistema ou utilize suas funcionalidades. Este campo é importante para a segurança e gerenciamento de contas, permitindo que administradores bloqueiem usuários que apresentem comportamento inadequado, tentativas de acesso não autorizadas ou outras atividades suspeitas. Um usuário bloqueado geralmente não pode fazer login ou interagir com o sistema até que seja desbloqueado por um administrador.
        /// </summary>
        public bool IsBlocked { get; set; } = false;
        /// <summary>
        /// Data de criação da conta do usuário, armazenada em formato UTC para garantir consistência em diferentes fusos horários. Este campo é útil para rastrear a antiguidade da conta, monitorar o crescimento do sistema e implementar funcionalidades relacionadas à data de registro, como promoções ou restrições baseadas no tempo. A data de criação é definida automaticamente quando um novo usuário é registrado no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Relação de Tokens de atualização associados ao usuário, permitindo a implementação de autenticação baseada em tokens e a renovação de sessões sem a necessidade de reautenticação constante. Esta coleção é fundamental para a segurança e usabilidade do sistema, pois permite que os usuários mantenham suas sessões ativas por períodos prolongados sem comprometer a segurança. Cada token de atualização é associado a um dispositivo ou sessão específica, permitindo o controle granular sobre o acesso do usuário.
        /// </summary>
        // Navigation Properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>
        /// Relação de códigos de redefinição de senha associados ao usuário, permitindo a implementação de funcionalidades de recuperação de senha. Esta coleção é importante para a segurança e usabilidade do sistema, pois permite que os usuários recuperem o acesso às suas contas em caso de esquecimento da senha. Cada código de redefinição de senha é geralmente associado a um token único e tem um tempo de validade limitado para garantir a segurança do processo de recuperação.
        /// </summary>
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = new List<PasswordResetCode>();
    }
}
