using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DataAnnotations
{
    /// <summary>
    /// Data annotation personalizada para validar o campo UserAgent.
    /// </summary>
    public class AllowedUserAgentAttribute : AllowedValuesAttribute
    {
        /// <summary>
        /// Construtor da classe AllowedUserAgentAttribute com os valores permitidos para o campo UserAgent.
        /// </summary>
        public AllowedUserAgentAttribute()
            : base(null, Constants.WebUserAgent, Constants.WebAdminUserAgent, Constants.AndroidUserAgent, Constants.WindowsUserAgent)
        {
            ErrorMessage = $"O campo UserAgent deve conter um dos seguintes valores: '{Constants.WebUserAgent}', '{Constants.WebAdminUserAgent}', '{Constants.AndroidUserAgent}' ou '{Constants.WindowsUserAgent}'.";
        }
    }
}
