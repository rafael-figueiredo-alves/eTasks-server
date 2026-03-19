using System.Threading.Tasks;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetCode);
        Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink);
    }
}
