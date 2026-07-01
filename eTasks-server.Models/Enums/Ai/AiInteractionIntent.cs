namespace eTasks_server.Models.Enums.Ai
{
    /// <summary>
    /// Enumerado que representa as diferentes intenções de interação com a IA.
    /// </summary>
    public enum AiInteractionIntent
    {
        /// <summary>
        /// Ajuda geral ou informações sobre como interagir com a IA.
        /// </summary>
        GeneralHelp = 0,

        /// <summary>
        /// Resumir o conteúdo fornecido.
        /// </summary>
        Summarize = 1,

        /// <summary>
        /// Reescrever o conteúdo fornecido para melhorar a clareza, gramática ou estilo.
        /// </summary>
        Rewrite = 2,

        /// <summary>
        /// Sugerir próximos passos ou ações com base no conteúdo fornecido.
        /// </summary>
        SuggestNextSteps = 3,

        /// <summary>
        /// Analisar o conteúdo fornecido para identificar padrões, insights ou informações relevantes.
        /// </summary>
        Analyze = 4,

        /// <summary>
        /// Planejar ou organizar tarefas, eventos ou projetos com base no conteúdo fornecido.
        /// </summary>
        Plan = 5
    }
}
