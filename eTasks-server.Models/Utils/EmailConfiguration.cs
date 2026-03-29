namespace eTasks_server.Models.Utils
{
    /// <summary>
    /// Classe para abrigar as configurações de e-mail, facilitando o acesso às propriedades de configuração do SMTP a partir do appsettings.json.
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Endereço do servidor SMTP, por exemplo: "smtp.gmail.com". Deve ser configurado no appsettings.json com a chave "SmtpHost".
        /// </summary>
        public string host { get; set; } = string.Empty;
        /// <summary>
        /// Porta do servidor SMTP, geralmente 587 para TLS ou 465 para SSL. Deve ser configurado no appsettings.json com a chave "SmtpPort". O valor padrão é 587 se não for especificado.
        /// </summary>
        public int port { get; set; }
        /// <summary>
        /// Se deve usar SSL para a conexão SMTP. Deve ser configurado no appsettings.json com a chave "SmtpEnableSsl". O valor padrão é true se não for especificado.
        /// </summary>
        public bool enableSsl { get; set; }
        /// <summary>
        /// Nome do usuário para autenticação SMTP, geralmente o endereço de e-mail completo. Deve ser configurado no appsettings.json com a chave "SmtpUsername".
        /// </summary>
        public string username { get; set; } = string.Empty;
        /// <summary>
        /// Senha para autenticação SMTP. Deve ser configurado no appsettings.json com a chave "SmtpPassword". É recomendado usar variáveis de ambiente ou serviços de gerenciamento de segredos para armazenar essa informação sensível, em vez de colocá-la diretamente no appsettings.json.
        /// </summary>
        public string password { get; set; } = string.Empty;
        /// <summary>
        /// Endereço de e-mail do remetente que aparecerá nos e-mails enviados. Deve ser configurado no appsettings.json com a chave "SmtpFromEmail". O valor padrão é string.Empty se não for especificado. É recomendado usar um endereço de e-mail válido para garantir a entrega dos e-mails. O nome do remetente pode ser configurado usando a propriedade "fromName". Se "fromName" estiver vazio, o endereço de e-mail será usado como nome do remetente.
        /// </summary>
        public string fromEmail { get; set; } = string.Empty;
        /// <summary>
        /// Nome do remetente que aparecerá nos e-mails enviados. Deve ser configurado no appsettings.json com a chave "SmtpFromName". O valor padrão é string.Empty se não for especificado. Se "fromName" estiver vazio, o endereço de e-mail será usado como nome do remetente.
        /// </summary>
        public string fromName { get; set; } = string.Empty;
    }
}
