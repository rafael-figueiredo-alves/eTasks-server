using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Classe das configurações do usuário. A classe UserSettings representa as preferências e configurações personalizadas de um usuário específico. Ela inclui propriedades como Theme (tema), Language (idioma), UseCamera (uso da câmera) e EnableBonusSystem (habilitar sistema de bônus). Cada instância de UserSettings está associada a um usuário por meio da propriedade UserUid, que é uma chave estrangeira que aponta para o usuário correspondente na tabela de usuários. A classe também inclui propriedades de data de criação e atualização para rastrear quando as configurações foram criadas ou modificadas.
    /// </summary>
    public class UserSettings : IEntityModelConfiguration<UserSettings>
    {
        /// <summary>
        /// Identificador único para as configurações do usuário. A propriedade Id é uma chave primária que identifica exclusivamente cada instância de UserSettings. Ela é gerada usando o método Guid.CreateVersion7(), que cria um identificador global exclusivo (GUID) com base na versão 7 do algoritmo de geração de GUID. Esse identificador é usado para garantir a unicidade das configurações do usuário no banco de dados e facilitar a referência a essas configurações em outras partes do sistema.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuário associado a essas configurações. A propriedade UserUid é uma chave estrangeira que aponta para o usuário correspondente na tabela de usuários. Ela é usada para estabelecer a relação entre as configurações do usuário e o usuário específico a que elas pertencem. Cada instância de UserSettings está associada a um único usuário por meio dessa propriedade, permitindo que as preferências e configurações personalizadas sejam vinculadas ao usuário correto no sistema.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Tema visual do aplicativo. A propriedade Theme representa o tema visual escolhido pelo usuário para a interface do aplicativo. Ela pode assumir valores como "light" (claro) ou "dark" (escuro), permitindo que o usuário personalize a aparência do aplicativo de acordo com suas preferências. O valor padrão é "light", indicando que o tema claro será usado se o usuário não fizer uma escolha específica.
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma preferido do usuário. A propriedade Language representa o idioma escolhido pelo usuário para a interface do aplicativo. Ela pode assumir valores como "en" (inglês), "pt" (português) ou outros códigos de idioma, permitindo que o usuário personalize a linguagem do aplicativo de acordo com suas preferências. O valor padrão é "pt", indicando que o idioma português será usado se o usuário não fizer uma escolha específica.
        /// </summary>
        public string Language { get; set; } = "pt";

        /// <summary>
        /// Indicador para saber se usará a funlção de câmera do aplicativo. A propriedade UseCamera é um indicador booleano que indica se o usuário optou por usar a função de câmera do aplicativo. Se o valor for true, isso significa que o usuário deseja utilizar a funcionalidade de câmera para capturar fotos ou vídeos dentro do aplicativo. Se o valor for false, isso indica que o usuário não deseja usar a função de câmera. Essa configuração pode ser usada para personalizar a experiência do usuário e controlar o acesso à funcionalidade de câmera com base nas preferências individuais.
        /// </summary>
        public bool UseCamera { get; set; }

        /// <summary>
        /// Habilita e desabilita o sistema de bônus do aplicativo. A propriedade EnableBonusSystem é um indicador booleano que indica se o usuário optou por habilitar ou desabilitar o sistema de bônus do aplicativo. Se o valor for true, isso significa que o usuário deseja participar do sistema de bônus, que pode incluir recompensas, pontos ou benefícios adicionais dentro do aplicativo. Se o valor for false, isso indica que o usuário não deseja participar do sistema de bônus. Essa configuração permite que os usuários personalizem sua experiência no aplicativo e decidam se desejam aproveitar os recursos relacionados ao sistema de bônus.
        /// </summary>
        public bool EnableBonusSystem { get; set; }

        /// <summary>
        /// Data de criação e atualização das configurações do usuário. As propriedades CreatedAt e UpdatedAt são usadas para rastrear quando as configurações do usuário foram criadas e quando foram atualizadas pela última vez. CreatedAt é definida com a data e hora atuais no momento da criação da instância de UserSettings, enquanto UpdatedAt é atualizada sempre que as configurações são modificadas. Essas propriedades são úteis para fins de auditoria e para garantir que as informações sobre as configurações do usuário estejam sempre atualizadas.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// data de atualização das configurações do usuário. A propriedade UpdatedAt é usada para rastrear quando as configurações do usuário foram modificadas pela última vez. Ela é atualizada sempre que as configurações são alteradas, permitindo que o sistema mantenha um registro preciso das mudanças feitas nas preferências do usuário ao longo do tempo. Isso é útil para fins de auditoria e para garantir que as informações sobre as configurações do usuário estejam sempre atualizadas.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Acesso ao usuário associado a essas configurações. A propriedade User é uma referência de navegação que permite acessar os detalhes do usuário relacionado a essas configurações. A relação entre UserSettings e User é estabelecida por meio da propriedade UserUid, que é uma chave estrangeira que aponta para o usuário correspondente na tabela de usuários.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Método de configuração do modelo para o Entity Framework Core. Define a tabela, chave primária e índices para a entidade UserSettings.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSettings>()
                                    .ToTable("user_settings")
                                    .HasKey(x => x.Id);

            modelBuilder.Entity<UserSettings>()
                                    .HasIndex(x => x.UserUid)
                                    .IsUnique();
        }
    }
}
