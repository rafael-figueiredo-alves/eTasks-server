using System;
using System.Collections.Generic;

namespace eTasks_server.Models.DTOs.Dashboard.Responses
{
    public class DashboardResponse
    {
        public int TotalUsers { get; set; }
        public int NewUsersLast7Days { get; set; }
        public int FailedLoginsToday { get; set; }
        public List<LoginTrendItem> LoginTrends { get; set; } = new();
    }

    public class LoginTrendItem
    {
        public DateTime Date { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}
