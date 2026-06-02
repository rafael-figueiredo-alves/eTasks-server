namespace eTasks_server.Models.Enums.Notifications
{
    /// <summary>
    /// Identifica o tipo de destinatário para uma notificação.
    /// </summary>
    public enum NotificationTargetType
    {
        /// <summary>
        /// Todos
        /// </summary>
        All = 1,

        /// <summary>
        /// Usuários regulares (exclui administradores)
        /// </summary>
        RegularUsers = 2,

        /// <summary>
        /// Administradores (exclui usuários regulares)
        /// </summary>
        Admins = 3,

        /// <summary>
        /// Usuários selecionados (destinatários específicos definidos para a notificação)
        /// </summary>
        SelectedUsers = 4
    }
}
