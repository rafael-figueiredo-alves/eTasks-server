namespace eTasks_server.Models.Enums.Ai
{
    /// <summary>
    /// Enumerado que representa os diferentes tipos de recursos que podem ser processados pela IA.
    /// </summary>
    public enum AiResourceType
    {
        /// <summary>
        /// Geral ou não especificado.
        /// </summary>
        General = 0,

        /// <summary>
        /// Tarefas ou listas de afazeres.
        /// </summary>
        Tasks = 1,

        /// <summary>
        /// Metas ou objetivos pessoais.
        /// </summary>
        Goals = 2,

        /// <summary>
        /// Anotações ou registros de informações.
        /// </summary>
        Notes = 3,

        /// <summary>
        /// Leituras ou materiais de estudo, como artigos, livros ou pesquisas.
        /// </summary>
        Readings = 4,
        
        /// <summary>
        /// Compras ou listas de compras, incluindo produtos e serviços.
        /// </summary>
        Shopping = 5,
        
        /// <summary>
        /// Informações financeiras, como orçamentos, despesas e investimentos.
        /// </summary>
        Finances = 6,
        
        /// <summary>
        /// Informações do perfil do usuário, incluindo preferências, histórico e configurações.
        /// </summary>
        UserProfile = 7
    }
}
