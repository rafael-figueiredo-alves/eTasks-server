using System;
using System.Collections.Generic;

namespace eTasks_server.Models.Users
{
    public class AdminUserDTO
    {
        public Guid Uid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserLoginLogDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminSetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}

