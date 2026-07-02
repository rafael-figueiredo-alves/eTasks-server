namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    /// <summary>
    /// Representa um elemento da caixa de entrada de notificações para um usuário específico.
    /// </summary>
    public class NotificationInboxItemResponse
    {
        /// <summary>
        /// Obtém ou define o identificador único do destinatário da notificação.
        /// </summary>
        public Guid RecipientId { get; set; }

        /// <summary>
        /// Obtém ou define o identificador único da notificação.
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// Obtém ou define o título da notificação.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o corpo da notificação.
        /// </summary>
        public string Body { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define a URL da ação da notificação.
        /// </summary>
        public string? ActionUrl { get; set; }
        
        /// <summary>
        /// Obtém ou define os dados JSON da notificação.
        /// </summary>
        public string? DataJson { get; set; }

        /// <summary>
        /// Obtém ou define a data e hora em que a notificação foi criada.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Obtém ou define a data e hora em que a notificação foi lida pelo destinatário. Se for nulo, significa que a notificação ainda não foi lida.
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Obtém um valor booleano que indica se a notificação foi lida pelo destinatário.
        /// </summary>
        public bool IsRead => ReadAt.HasValue;
    }
}
