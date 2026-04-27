namespace eTasks_server.Models.Utils
{
    /// <summary>
    /// Classe de constantes para configurações e valores fixos usados no projeto eTasks Server.
    /// </summary>
    public static class Constants
    {
        #region Constantes referentes a configurações de e-mail e SMTP
        /// <summary>
        /// Se SMTP está habilitado para envio de e-mails, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para determinar se a funcionalidade de envio de e-mails está ativa no sistema, permitindo que os administradores ativem ou desativem essa funcionalidade conforme necessário para notificações, recuperação de senha e outras comunicações por e-mail.
        /// </summary>
        public const string SmtpEnabled = "Smtp:Enabled";

        /// <summary>
        /// Host do servidor SMTP usado para envio de e-mails, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para especificar o endereço do servidor de e-mail que será utilizado para enviar mensagens, garantindo que as comunicações por e-mail sejam roteadas corretamente através do servidor configurado.
        /// </summary>
        public const string SmtpHost = "Smtp:Host";

        /// <summary>
        /// Porta do servidor SMTP usada para envio de e-mails, que deve ser configurada no arquivo appsettings.json ou em variáveis de ambiente, e é usada para especificar a porta de comunicação com o servidor de e-mail, garantindo que as mensagens sejam enviadas através da porta correta para uma comunicação eficiente e segura com o servidor SMTP.
        /// </summary>
        public const string SmtpPort = "Smtp:Port";

        /// <summary>
        /// Se está habilitado o uso de SSL para a conexão SMTP, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para determinar se a comunicação com o servidor de e-mail deve ser criptografada usando SSL, garantindo a segurança das mensagens enviadas e protegendo as informações sensíveis durante a transmissão dos e-mails.
        /// </summary>
        public const string SmtpEnableSsl = "Smtp:EnableSsl";

        /// <summary>
        /// Nome do usuário para autenticação no servidor SMTP, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para fornecer as credenciais necessárias para autenticar a conexão com o servidor de e-mail, garantindo que apenas usuários autorizados possam enviar mensagens através do servidor SMTP configurado.
        /// </summary>
        public const string SmtpUsername = "Smtp:Username";

        /// <summary>
        /// Senha para autenticação no servidor SMTP, que deve ser configurada no arquivo appsettings.json ou em variáveis de ambiente, e é usada para fornecer as credenciais necessárias para autenticar a conexão com o servidor de e-mail, garantindo que apenas usuários autorizados possam enviar mensagens através do servidor SMTP configurado. É importante garantir que essa senha seja armazenada de forma segura e não exposta em código-fonte ou repositórios públicos para evitar comprometer a segurança do sistema.
        /// </summary>
        public const string SmtpPassword = "Smtp:Password";

        /// <summary>
        /// Endereço de e-mail do remetente usado para envio de e-mails, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para especificar o endereço de e-mail que aparecerá como remetente nas mensagens enviadas pelo sistema, garantindo que as comunicações por e-mail sejam identificáveis e consistentes com a identidade do aplicativo ou organização responsável pelo envio das mensagens.
        /// </summary>
        public const string SmtpFromEmail = "Smtp:FromEmail";

        /// <summary>
        /// Nome do remetente usado para envio de e-mails, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para especificar o nome que aparecerá como remetente nas mensagens enviadas pelo sistema, proporcionando uma identificação mais amigável e profissional nas comunicações por e-mail, além de reforçar a identidade do aplicativo ou organização responsável pelo envio das mensagens.
        /// </summary>
        public const string SmtpFromName = "Smtp:FromName";
        #endregion

        #region Constantes referentes a configurações de CORS e API
        /// <summary>
        /// Nome da política CORS usada para permitir requisições do aplicativo WASM hospedado no GitHub Pages.
        /// </summary>
        public const string CorsPolicyName = "WASMAppPolicy";

        /// <summary>
        /// Origem permitida para requisições CORS, apontando para o aplicativo WASM hospedado no GitHub Pages.
        /// </summary>
        public const string AllowedOrigin = "https://rafael-figueiredo-alves.github.io";

        /// <summary>
        /// Nome da string de conexão do banco de dados, que deve ser configurada no arquivo appsettings.json ou em variáveis de ambiente.
        /// </summary>
        public const string DatabaseConnection = "DefaultConnection";

        /// <summary>
        /// Nome da configuração para a URL base da API, que deve ser configurada no arquivo appsettings.json ou em variáveis de ambiente.
        /// </summary>
        public const string ApiBaseUrl = "ApiSettings:BaseUrl";

        /// <summary>
        /// Chave de configuracao usada para autorizar o cadastro administrativo no painel web.
        /// </summary>
        public const string AdminApiKeyConfig = "APIKEY_ADMIN";

        /// <summary>
        /// Endpoint para a verificação de saúde da aplicação, usado para monitoramento e verificação de status.
        /// </summary>
        public const string HealthCheckEndpoint = "/health";

        /// <summary>
        /// Segmento da URL para os endpoints da API, indicando a versão da API (v2) e organizando as rotas de forma consistente.
        /// </summary>
        public const string URLClientServicesAPISegment = "api/v2/";

        /// <summary>
        /// Endereço do endpoint para a documentação OpenAPI (Swagger) da API, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para acessar a documentação interativa da API, permitindo que os desenvolvedores explorem e testem os endpoints disponíveis de forma fácil e eficiente.
        /// </summary>
        public const string OpenApiEndpoint = "openapi/v2.json";

        /// <summary>
        /// endpoint para a interface de documentação interativa da API (Swagger UI), que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para acessar a interface gráfica onde os desenvolvedores podem visualizar a documentação da API, testar os endpoints e entender melhor as funcionalidades oferecidas pelo eTasks Server.
        /// </summary>
        public const string ScalarDocEndpoint = "docs";
        #endregion

        #region JWT Configurations
        /// <summary>
        /// Chave criptográfica para a geração e validação de tokens JWT, que deve ser configurada no arquivo appsettings.json ou em variáveis de ambiente, e é usada para garantir a segurança e integridade dos tokens de autenticação usados para proteger os endpoints da API e gerenciar o acesso dos usuários ao sistema.
        /// </summary>
        public const string JwtKeyConfig = "Jwt:Key";

        /// <summary>
        /// Consumidor (issuer) dos tokens JWT, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para identificar a entidade que emite os tokens de autenticação, permitindo que os sistemas consumidores verifiquem a autenticidade e a origem dos tokens recebidos.
        /// </summary>
        public const string JwtIssuerConfig = "Jwt:Issuer";

        /// <summary>
        /// Público-alvo (audience) dos tokens JWT, que deve ser configurado no arquivo appsettings.json ou em variáveis de ambiente, e é usado para identificar os destinatários pretendidos dos tokens de autenticação, permitindo que os sistemas consumidores verifiquem se los tokens recebidos são destinados a eles e, assim, garantir um controle de acesso mais seguro e eficiente.
        /// </summary>
        public const string JwtAudienceConfig = "Jwt:Audience";

        /// <summary>
        /// Chave usada para criptografia simetrica de segredos persistidos em banco.
        /// </summary>
        public const string DataEncryptionKeyConfig = "Security:DataEncryptionKey";

        /// <summary>
        /// Claim customizado que identifica o tipo de cliente para o qual o token JWT foi emitido.
        /// </summary>
        public const string UserAgentClaimType = "user_agent";

        /// <summary>
        /// Claim customizado para armazenar o caminho da foto de perfil do usuário.
        /// </summary>
        public const string PhotoPathClaimType = "photo_path";

        /// <summary>
        /// Nome do cookie HttpOnly usado para transportar o refresh token nas chamadas da API.
        /// </summary>
        public const string RefreshTokenCookieName = "refresh_token";
        #endregion

        #region UserAgents da aplicação
        /// <summary>
        /// UserAgent aceito pela aplicação WebAssembly/PWA consumindo a API.
        /// </summary>
        public const string WebUserAgent = "web";

        /// <summary>
        /// UserAgent aceito pelo aplicativo nativo Delphi no Windows.
        /// </summary>
        public const string WindowsUserAgent = "windows";

        /// <summary>
        /// UserAgent aceito pelo aplicativo nativo Delphi no Android.
        /// </summary>
        public const string AndroidUserAgent = "android";

        /// <summary>
        /// UserAgent aceito pelo painel administrativo web.
        /// </summary>
        public const string WebAdminUserAgent = "web-adm";


        /// <summary>
        /// Lista de UserAgents aceitos nos consumidores da API.
        /// </summary>
        public static readonly string[] ApiClientUserAgents =
        [
            WebUserAgent,
            WindowsUserAgent,
            AndroidUserAgent
        ];
        #endregion

        #region Informações de versão e branding
        /// <summary>
        /// Versão da API do eTasks Server, usada para controle de versão e comunicação com os clientes sobre a versão atual da API e funcionalidades disponíveis.
        /// </summary>
        public const string ApiVersion = "v2";

        /// <summary>
        /// Título da aplicação, usado em mensagens de erro, documentação e outros contextos onde o nome do aplicativo é necessário para identificação e branding.       
        /// </summary>
        public const string AppTitle = "eTasks Server";

        /// <summary>
        /// Descrição da API, usada na documentação interativa da API (Swagger) para fornecer uma visão geral do propósito e funcionalidades da API do eTasks Server, destacando seu foco em gerenciamento de tarefas eficientes.
        /// </summary>
        public const string ApiDescription = "Esta é a documentação oficial da API do eTasks. Você encontrará informações sobre todas as possibilidades que a API oferece para gestão de todas as funcionalidades do sistema.";

        /// <summary>
        /// Nome do desenvolvedor, usado em mensagens de erro, documentação e outros contextos onde a identificação do responsável pelo desenvolvimento é relevante para contato, suporte ou reconhecimento.
        /// </summary>
        public const string DeveloperName = "Rafael Figueiredo Alves";

        /// <summary>
        /// Versão do servidor eTasks, usada para controle de versão e comunicação com os clientes sobre a versão atual da API e funcionalidades disponíveis.
        /// </summary>
        public const string ServerVersion = "1.1.0";
        #endregion
    }
}
