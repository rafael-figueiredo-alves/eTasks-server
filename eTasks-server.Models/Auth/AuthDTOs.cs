using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

//Este namespace contém os DTOs (Data Transfer Objects) relacionados à autenticação e gerenciamento de usuários, incluindo as classes LoginRequest, LoginResponse, RegisterRequest, RefreshTokenRequest, ForgotPasswordRequest, ResetPasswordRequest e ChangePasswordRequest. Esses DTOs são utilizados para transportar os dados necessários para as operações de login, registro, renovação de token, recuperação de senha e alteração de senha, garantindo uma comunicação eficiente e segura entre o cliente e o servidor durante esses processos. Cada classe é projetada para conter os campos relevantes para a operação específica, com validações apropriadas para garantir a integridade dos dados e a segurança do sistema.
namespace eTasks_server.Models.Auth
{
    /// <summary>
    /// DTO responsável por transportar os dados de login do usuário, incluindo email, senha e opcionalmente o user agent para identificar a origem do login (Web ou Delphi).
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// E-mail do usuário, utilizado para autenticação. Deve ser um endereço de e-mail válido e registrado no sistema.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Senha do usuário, utilizada para autenticação. Deve ser uma string segura e seguir as políticas de senha definidas pelo sistema (ex: mínimo de caracteres, uso de caracteres especiais, etc.).
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha não deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Este campo é opcional e pode ser utilizado para identificar a origem do login, como "Web" para logins realizados através da interface web ou "Delphi" para logins realizados através de um cliente Delphi. Isso pode ser útil para fins de análise, monitoramento ou aplicação de políticas específicas de segurança com base na origem do login.
        /// </summary>
        /// <remarks>
        /// Utilizar o campo UserAgent pode ajudar a diferenciar os tipos de clientes que estão acessando o sistema, permitindo uma melhor análise de uso e a aplicação de medidas de segurança específicas para cada tipo de cliente. Por exemplo, se um login for identificado como vindo de um cliente Delphi, o sistema pode aplicar regras de segurança mais rigorosas ou monitorar atividades suspeitas com mais atenção. No entanto, é importante lembrar que o campo UserAgent é opcional e pode ser facilmente manipulado, portanto, não deve ser a única medida de segurança utilizada para proteger o sistema.
        /// </remarks>
        /// <example>
        /// Web: "Web", Delphi: "Delphi", ou pode ser deixado em branco se a origem do login não for relevante ou conhecida.
        /// </example>
        [AllowedUserAgent]
        public string? UserAgent { get; set; } // Opcional, o front pode enviar "Web" ou "Delphi"
    }

    /// <summary>
    /// Representa a resposta do servidor após uma tentativa de login bem-sucedida, contendo o token de autenticação (JWT), o token de atualização (refresh token) e as datas de expiração de ambos os tokens. Esses dados são essenciais para que o cliente possa manter a sessão do usuário ativa e solicitar novos tokens quando necessário, garantindo uma experiência de usuário contínua e segura.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Token de autenticação (JWT) gerado pelo servidor após uma tentativa de login bem-sucedida. Este token é utilizado para autenticar as requisições subsequentes do cliente, permitindo o acesso aos recursos protegidos do sistema. O token deve ser incluído no cabeçalho Authorization das requisições, geralmente no formato "Bearer {token}", para que o servidor possa validar a autenticidade e a autorização do usuário.
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// Este é o token de atualização (refresh token) gerado pelo servidor, utilizado para obter um novo token de autenticação (JWT) quando o token atual expirar. O refresh token é uma string segura e deve ser armazenada de forma segura pelo cliente, pois pode ser utilizado para manter a sessão do usuário ativa sem a necessidade de solicitar as credenciais novamente. O cliente pode enviar o refresh token em uma requisição específica para renovar o token de autenticação, garantindo uma experiência de usuário contínua e sem interrupções.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// Representa a data e hora de expiração do token de autenticação (JWT). O cliente deve monitorar essa data para saber quando o token irá expirar e, assim, solicitar um novo token utilizando o refresh token antes que o token atual se torne inválido. Isso é crucial para garantir que o usuário mantenha acesso contínuo aos recursos protegidos do sistema sem interrupções, evitando a necessidade de realizar login novamente.
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; }
        /// <summary>
        /// Representa a data e hora de expiração do token de atualização (refresh token). O cliente deve monitorar essa data para saber quando o refresh token irá expirar, pois após essa data, o cliente não poderá mais utilizar o refresh token para obter novos tokens de autenticação. É importante que o cliente solicite um novo refresh token antes que o atual expire para garantir que a sessão do usuário possa ser mantida ativa sem a necessidade de realizar login novamente.
        /// </summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }
    }

    /// <summary>
    /// Entidade que representa os dados necessários para registrar um novo usuário no sistema, incluindo nome, email, senha e uma foto opcional em formato Base64. Esses dados são essenciais para criar uma conta de usuário e permitir que o novo usuário acesse os recursos protegidos do sistema após o registro bem-sucedido. O campo de foto é opcional, permitindo que os usuários escolham se desejam ou não fornecer uma imagem de perfil durante o processo de registro.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome de exibição do usuário, utilizado para identificar o usuário no sistema. Este campo é obrigatório e deve conter um nome válido que possa ser exibido em perfis, mensagens ou outras áreas do sistema onde o nome do usuário seja necessário. O nome de exibição pode ser diferente do email e deve ser escolhido pelo usuário durante o processo de registro.
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
        /// Abriga a senha do usuário, que é um elemento crucial para a segurança da conta. Este campo é obrigatório e deve conter uma senha forte, seguindo as políticas de segurança definidas pelo sistema, como um mínimo de caracteres, uso de letras maiúsculas e minúsculas, números e caracteres especiais. A senha deve ser armazenada de forma segura no servidor, utilizando técnicas de hashing e salting para proteger os dados do usuário contra acessos não autorizados.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha não deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Campo opcional que pode conter a foto do usuário em formato Base64. Este campo é utilizado para permitir que os usuários forneçam uma imagem de perfil durante o processo de registro. A foto em Base64 é uma representação textual de uma imagem binária, permitindo que a imagem seja transmitida como parte dos dados do registro sem a necessidade de um upload separado. Se fornecida, a foto pode ser armazenada e exibida no perfil do usuário ou em outras áreas do sistema onde a imagem do usuário seja relevante. No entanto, como este campo é opcional, os usuários podem optar por não fornecer uma foto durante o registro, e o sistema deve ser capaz de lidar com essa situação adequadamente.
        /// </summary>
        [Base64String(ErrorMessage = "Formato de imagem aceito é apenas Base 64.")]
        public string? PhotoBase64 { get; set; } 
    }

    /// <summary>
    /// Representa entidade que contém os dados necessários para solicitar a renovação do token de autenticação (JWT) utilizando um token de atualização (refresh token). Este DTO é essencial para permitir que o cliente mantenha a sessão do usuário ativa sem a necessidade de solicitar as credenciais novamente, garantindo uma experiência de usuário contínua e segura. O campo UserAgent é opcional e pode ser utilizado para identificar a origem da solicitação de renovação do token, como "Web" ou "Delphi", permitindo uma melhor análise de uso e aplicação de políticas de segurança específicas para cada tipo de cliente.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Representa o token de atualização (refresh token) que foi previamente gerado pelo servidor e fornecido ao cliente durante o processo de login. Este token é utilizado para solicitar um novo token de autenticação (JWT) quando o token atual expirar, permitindo que o cliente mantenha a sessão do usuário ativa sem a necessidade de solicitar as credenciais novamente. O refresh token deve ser armazenado de forma segura pelo cliente, pois pode ser utilizado para obter acesso contínuo aos recursos protegidos do sistema. O cliente deve enviar este token em uma requisição específica para renovar o token de autenticação, garantindo uma experiência de usuário contínua e sem interrupções.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// Representa o user agent da solicitação de renovação do token, que é uma string opcional utilizada para identificar a origem da solicitação, como "Web" para logins realizados através da interface web ou "Delphi" para logins realizados através de um cliente Delphi. O campo UserAgent pode ser útil para fins de análise, monitoramento ou aplicação de políticas específicas de segurança com base na origem da solicitação de renovação do token. No entanto, é importante lembrar que o campo UserAgent é opcional e pode ser facilmente manipulado, portanto, não deve ser a única medida de segurança utilizada para proteger o sistema.
        /// </summary>
        [AllowedUserAgent]
        public string? UserAgent { get; set; } 
    }

    /// <summary>
    /// Representa a entidade que contém os dados necessários para solicitar a recuperação de senha, incluindo o email do usuário. Este DTO é essencial para permitir que os usuários iniciem o processo de recuperação de senha, onde o sistema pode enviar um email com instruções para redefinir a senha ou um link para uma página de redefinição de senha. O campo de email deve ser um endereço de e-mail válido e registrado no sistema, pois será utilizado para identificar o usuário e garantir que as instruções de recuperação sejam enviadas para o destinatário correto.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Representa o email do usuário que está solicitando a recuperação de senha. Este campo é obrigatório e deve ser um endereço de e-mail válido e registrado no sistema, pois será utilizado para identificar o usuário e enviar as instruções de recuperação de senha. O sistema pode enviar um email contendo um link para uma página de redefinição de senha ou um código de verificação que o usuário deve utilizar para confirmar sua identidade antes de permitir a criação de uma nova senha. É importante garantir que o email fornecido seja válido e esteja associado a uma conta existente para que o processo de recuperação de senha funcione corretamente.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta da solicitação de esqueci minha senha, indicando se a operação foi bem-sucedida e fornecendo uma mensagem informativa. Este DTO é utilizado para informar ao usuário sobre o resultado da solicitação de recuperação de senha, garantindo que o usuário saiba se as instruções de recuperação foram enviadas para o email fornecido ou se ocorreu algum problema durante o processo. A mensagem pode ser personalizada para fornecer informações adicionais ou orientações sobre os próximos passos a serem seguidos pelo usuário.
    /// </summary>
    public class PasswordResponse
    {
        /// <summary>
        /// Indicado de sucesso da operação de solicitação de recuperação de senha. Este campo é um booleano que indica se a solicitação foi processada com sucesso, ou seja, se as instruções de recuperação de senha foram enviadas para o email do usuário. Um valor de true indica que a operação foi bem-sucedida, enquanto um valor de false indica que ocorreu um problema durante o processo, como um email inválido ou não registrado no sistema. Essa informação é crucial para que o usuário saiba se deve verificar seu email para obter as instruções de recuperação ou se precisa corrigir algum erro na solicitação.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Mensagem informativa sobre o resultado da solicitação de recuperação de senha. Este campo é uma string que pode conter informações adicionais ou orientações para o usuário, como "Se o e-mail existir, um código foi enviado." ou "O email fornecido não está registrado no sistema." A mensagem deve ser clara e útil para orientar o usuário sobre os próximos passos a serem seguidos, garantindo uma experiência de usuário positiva durante o processo de recuperação de senha.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    };

    /// <summary>
    /// Representa a entidade que contém os dados necessários para solicitar a redefinição de senha, incluindo o email do usuário, o código de verificação enviado para o email do usuário e a nova senha que o usuário deseja definir. Este DTO é essencial para permitir que os usuários concluam o processo de redefinição de senha após receberem as instruções de recuperação, garantindo que apenas o usuário legítimo possa alterar a senha da conta. O campo de email deve ser um endereço de e-mail válido e registrado no sistema, o campo de código deve conter o código de verificação correto enviado para o email do usuário, e o campo de nova senha deve conter uma senha forte seguindo as políticas de segurança definidas pelo sistema.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Representa o email do usuário que está solicitando a redefinição de senha. Este campo é obrigatório e deve ser um endereço de e-mail válido e registrado no sistema, pois será utilizado para identificar o usuário e garantir que a solicitação de redefinição de senha seja associada à conta correta. O sistema pode enviar um email contendo um link para uma página de redefinição de senha ou um código de verificação que o usuário deve utilizar para confirmar sua identidade antes de permitir a criação de uma nova senha. É importante garantir que o email fornecido seja válido e esteja associado a uma conta existente para que o processo de redefinição de senha funcione corretamente.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Código de verificação ou token que foi enviado para o email do usuário durante o processo de recuperação de senha. Este campo é obrigatório e deve conter o código ou token correto para que o sistema possa validar a solicitação de redefinição de senha e garantir que apenas o usuário legítimo possa criar uma nova senha para a conta. O código ou token geralmente tem um tempo de validade limitado, e o usuário deve utilizá-lo dentro desse período para concluir o processo de redefinição de senha com sucesso.
        /// </summary>
        [Required]
        [Length(6, 6, ErrorMessage = "O código de verificação deve ter exatamente 6 dígitos")]        
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Representa a nova senha que o usuário deseja definir para sua conta após a validação do código de verificação. Este campo é obrigatório e deve conter uma senha forte, seguindo as políticas de segurança definidas pelo sistema, como um mínimo de caracteres, uso de letras maiúsculas e minúsculas, números e caracteres especiais. A nova senha deve ser armazenada de forma segura no servidor, utilizando técnicas de hashing e salting para proteger os dados do usuário contra acessos não autorizados. O processo de redefinição de senha deve garantir que apenas o usuário legítimo possa alterar a senha da conta, utilizando o código de verificação como uma medida de segurança adicional.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha não deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representa a entidade que contém os dados necessários para solicitar a alteração de senha, incluindo a senha atual do usuário e a nova senha que o usuário deseja definir. Este DTO é essencial para permitir que os usuários alterem sua senha de forma segura, garantindo que apenas o usuário legítimo possa realizar essa ação. O campo de senha atual deve conter a senha correta do usuário para validar a solicitação de alteração, enquanto o campo de nova senha deve conter uma senha forte seguindo as políticas de segurança definidas pelo sistema. O processo de alteração de senha deve garantir que as credenciais do usuário sejam protegidas e que a nova senha seja armazenada de forma segura no servidor.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Senha atual do usuário, utilizada para validar a solicitação de alteração de senha. Este campo é obrigatório e deve conter a senha correta do usuário para garantir que apenas o usuário legítimo possa alterar a senha da conta. A senha atual é uma medida de segurança adicional para proteger as credenciais do usuário e evitar que terceiros não autorizados possam alterar a senha da conta sem o conhecimento do usuário.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha atual deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha atual não deve exceder 30 caracteres")]
        public string CurrentPassword { get; set; } = string.Empty;
        /// <summary>
        /// Nova senha que o usuário deseja definir para sua conta. Este campo é obrigatório e deve conter uma senha forte, seguindo as políticas de segurança definidas pelo sistema, como um mínimo de caracteres, uso de letras maiúsculas e minúsculas, números e caracteres especiais. A nova senha deve ser armazenada de forma segura no servidor, utilizando técnicas de hashing e salting para proteger os dados do usuário contra acessos não autorizados. O processo de alteração de senha deve garantir que apenas o usuário legítimo possa alterar a senha da conta, utilizando a senha atual como uma medida de segurança adicional para validar a solicitação de alteração.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha não deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
