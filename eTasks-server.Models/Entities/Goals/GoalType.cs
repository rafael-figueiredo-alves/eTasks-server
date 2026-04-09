namespace eTasks_server.Models.Entities.Goals
{
    /// <summary>
    /// Define a categoria principal da meta.
    /// </summary>
    public enum GoalType
    {
        /// <summary>
        /// Meta pessoal.
        /// </summary>
        Personal = 0,
        /// <summary>
        /// Meta profissional.
        /// </summary>
        Professional = 1,
        /// <summary>
        /// Meta de estudos.
        /// </summary>
        Education = 2,
        /// <summary>
        /// Meta de saude.
        /// </summary>
        Health = 3,
        /// <summary>
        /// Meta financeira.
        /// </summary>
        Financial = 4
    }
}
