namespace eTasks_server.Models.Enums.Bonus
{
    /// <summary>
    /// Define as origens possíveis para concessão de pontos ao usuário.
    /// </summary>
    public enum BonusPointSource
    {
        /// <summary>
        /// Ajuste manual de pontos.
        /// </summary>
        ManualAdjustment = 0,
        /// <summary>
        /// Conclusão de tarefa.
        /// </summary>
        TaskCompletion = 1,
        /// <summary>
        /// Conclusão de meta.
        /// </summary>
        GoalCompletion = 2,
        /// <summary>
        /// Conclusão de leitura.
        /// </summary>
        ReadingCompletion = 3,
        /// <summary>
        /// Conclusão de lista de compras.
        /// </summary>
        ShoppingListCompletion = 4,
        /// <summary>
        /// Fechamento mensal com saldo positivo.
        /// </summary>
        PositiveMonthlyBalance = 5,
        /// <summary>
        /// Recompensa associada a conquistas.
        /// </summary>
        AchievementReward = 6
    }
}
