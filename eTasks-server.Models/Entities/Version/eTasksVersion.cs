using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Version
{
    /// <summary>
    /// Dados da versão do aplicativo cliente, para controle de atualizações e compatibilidade.
    /// </summary>
    public class eTasksVersion : IEntityModelConfiguration<eTasksVersion>
    {
        /// <summary>
        /// Id obrigatório para o Entity Framework, mas fixado em 1, pois só haverá uma versão ativa do aplicativo cliente.
        /// </summary>
        [AllowedValues([1])]
        public int Id { get; set; } = 1;

        /// <summary>
        /// Versão numérica do aplicativo cliente, usada para controle de compatibilidade e atualizações. Deve ser incrementada a cada nova versão lançada.
        /// </summary>
        [Required]
        public int AppVersion { get; set; } = 1;

        /// <summary>
        /// Versão de exibição do aplicativo cliente, usada para mostrar aos usuários a versão atual do aplicativo. Pode ser incrementada a cada nova versão lançada, mas não é obrigatória para o controle de compatibilidade. Deve seguir o formato "X.Y.Z" (ex: "2.0.0").
        /// </summary>
        [Required]
        public string DisplayVersion { get; set; } = "2.0.0";

        /// <summary>
        /// URL para download da versão mais recente do aplicativo cliente, usada para permitir que os usuários atualizem facilmente para a versão mais recente. Deve ser atualizado a cada nova versão lançada. Deve ser um link direto para o arquivo de instalação (ex: APK para Android, EXE para Windows).
        /// </summary>
        public string URL_APK { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.apk";

        /// <summary>
        /// URL para download da versão mais recente do aplicativo cliente para Windows, usada para permitir que os usuários atualizem facilmente para a versão mais recente. Deve ser atualizado a cada nova versão lançada. Deve ser um link direto para o arquivo de instalação (ex: EXE para Windows).
        /// </summary>
        public string URL_Win { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.exe";

        /// <summary>
        /// Método para configurar a entidade eTasksVersion no Entity Framework, definindo a tabela "version" e a chave primária como "Id". Isso garante que o Entity Framework saiba como mapear a classe eTasksVersion para a tabela correspondente no banco de dados.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eTasksVersion>()
                                  .ToTable("version")
                                  .HasKey(x => x.Id);
        }
    }
}
