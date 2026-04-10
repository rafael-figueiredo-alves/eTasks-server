namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Tipo de exibição de conquista, indicando se a conquista é representada por um troféu ou uma medalha.
    /// </summary>
    public enum AchievementDisplayType
    {
        /// <summary>
        /// Trofeu: Indica que a conquista é representada por um troféu, simbolizando uma realização significativa ou um marco importante alcançado pelo usuário.
        /// </summary>
        Trophy = 1,

        /// <summary>
        /// Medalha: Indica que a conquista é representada por uma medalha, simbolizando uma realização ou um marco alcançado pelo usuário, mas com um significado ou importância diferente do troféu.
        /// </summary>
        Medal = 2
    }
}
