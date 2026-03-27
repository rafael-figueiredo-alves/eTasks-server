using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DataAnnotations
{
    /// <summary>
    /// Data annotation personalizada para validar o campo UserAgent, permitindo apenas os valores "web", "web-adm", "android", "windows" e "ios". Essa validação é importante para garantir que o campo UserAgent contenha apenas valores válidos e esperados, evitando possíveis erros ou comportamentos indesejados no sistema. A utilização dessa data annotation personalizada ajuda a manter a integridade dos dados e a consistência do sistema, garantindo que apenas valores permitidos sejam aceitos para o campo UserAgent.
    /// </summary>
    public class AllowedUserAgentAttribute : AllowedValuesAttribute
    {
        /// <summary>
        /// Construtor da classe AllowedUserAgentAttribute, que chama o construtor da classe base AllowedValuesAttribute passando os valores permitidos para o campo UserAgent: "web", "web-adm", "android", "windows" e "ios". Esses valores representam os diferentes tipos de agentes de usuário que podem acessar o sistema, e a validação garante que apenas esses valores sejam aceitos para o campo UserAgent, mantendo a integridade dos dados e a consistência do sistema.       
        /// </summary>
        public AllowedUserAgentAttribute()
            : base(null, "web", "web-adm", "android", "windows", "ios")
        {
            ErrorMessage = "O campo UserAgent deve conter um dos seguintes valores: 'web', 'web-adm', 'android', 'windows' ou 'ios'.";
        }
    }
}
