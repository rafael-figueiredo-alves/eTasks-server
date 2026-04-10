using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities
{
    /// <summary>
    /// Interface padrão das entidades do sistema, que define um método de configuração para o Entity Framework Core. As classes que implementam essa interface devem fornecer uma implementação do método Configure, que é usado para configurar a entidade no contexto do banco de dados. Isso inclui definir a tabela, chaves primárias, relacionamentos e outras configurações específicas da entidade. Ao implementar essa interface, as entidades podem ser facilmente configuradas e integradas ao modelo de dados do aplicativo usando o Entity Framework Core.
    /// </summary>
    /// <typeparam name="TSelf">Tipo da entidade que implementa a interface.</typeparam>
    public interface IEntityModelConfiguration<TSelf>
        where TSelf : IEntityModelConfiguration<TSelf>
    {
        /// <summary>
        /// Metodo de configuração do modelo para o Entity Framework Core. As classes que implementam essa interface devem fornecer uma implementação deste método para configurar a entidade no contexto do banco de dados, definindo a tabela, chaves primárias, relacionamentos e outras configurações específicas da entidade.
        /// </summary>
        /// <param name="modelBuilder">O construtor de modelo do Entity Framework Core usado para configurar a entidade.</param>
        static abstract void Configure(ModelBuilder modelBuilder);
    }
}
