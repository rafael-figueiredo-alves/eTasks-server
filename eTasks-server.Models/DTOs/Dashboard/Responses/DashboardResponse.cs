namespace eTasks_server.Models.DTOs.Dashboard.Responses
{
    /// <summary>
    /// Dados para alimentar o dashboard do administrador, incluindo estatísticas de usuários e tendências de login.
    /// </summary>
    public class DashboardResponse
    {
        /// <summary>
        /// Total de usuários cadastrados no sistema, incluindo ativos e inativos.
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// Usuários que se registraram no sistema nos últimos 7 dias, indicando o crescimento recente da base de usuários.
        /// </summary>
        public int NewUsersLast7Days { get; set; }
        
        /// <summary>
        /// Total de logins falhos registrados hoje, o que pode indicar problemas de segurança ou dificuldades de acesso para os usuários.
        /// </summary>
        public int FailedLoginsToday { get; set; }
        
        /// <summary>
        /// Tendências de login ao longo do tempo, incluindo contagens de logins bem-sucedidos e falhos.
        /// </summary>
        public List<LoginTrendItem> LoginTrends { get; set; } = new();
    }

    /// <summary>
    /// Tendências de login para um período específico, incluindo a data, o número de logins bem-sucedidos e o número de logins falhos.
    /// </summary>
    public class LoginTrendItem
    {
        /// <summary>
        /// Data para a qual as tendências de login estão sendo registradas, permitindo a análise de padrões ao longo do tempo.
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Total de logins bem-sucedidos registrados para a data específica, indicando o nível de atividade dos usuários e a eficácia do sistema de autenticação.
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// Total de logins falhos registrados para a data específica, o que pode indicar problemas de segurança ou dificuldades de acesso para os usuários.
        /// </summary>
        public int FailureCount { get; set; }
    }
}
