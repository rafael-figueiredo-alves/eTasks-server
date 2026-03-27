using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Version
{
    /// <summary>
    /// Entidade que representa dados da versão do Aplicativo eTasks, incluindo número da versão, URL para download do APK e do executável para Windows.
    /// </summary>
    public class eTasksVersion
    {
        /// <summary>
        /// Identificação do registro
        /// </summary>
        /// <remarks>
        /// Valor sempre deve ser 1, pois é utilizado apenas um registro para armazenar as informações da versão do aplicativo. O campo Id é necessário para a estruturação do banco de dados, mas não tem função de identificação de múltiplos registros, já que apenas um registro é utilizado para armazenar as informações da versão do aplicativo.
        /// </remarks>
        [AllowedValues([1])]
        public int Id { get; set; } = 1;
        /// <summary>
        /// Versão do aplicativo, utilizada para controle de atualizações. Deve ser incrementada a cada nova versão lançada.
        /// </summary>
        [Required]       
        public int AppVersion { get; set; } = 1;
        /// <summary>
        /// É a versão exibida para os usuários, que pode incluir informações adicionais como nome da versão ou data de lançamento. Deve ser atualizada a cada nova versão lançada para refletir as mudanças e melhorias feitas no aplicativo.
        /// </summary>
        [Required]       
        public string DisplayVersion { get; set; } = "2.0.0";
        /// <summary>
        /// URL para download do arquivo APK do aplicativo eTasks, utilizado para distribuição em plataformas Android. Deve ser atualizado a cada nova versão lançada para garantir que os usuários tenham acesso à versão mais recente do aplicativo.
        /// </summary>        
        public string URL_APK { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.apk";
        /// <summary>
        /// Url para download do arquivo executável do aplicativo eTasks, utilizado para distribuição em plataformas Windows. Deve ser atualizado a cada nova versão lançada para garantir que os usuários tenham acesso à versão mais recente do aplicativo.
        /// </summary>
        public string URL_Win { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.exe";
    }
}
