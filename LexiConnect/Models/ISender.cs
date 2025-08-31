using Microsoft.AspNetCore.Identity.UI.Services;

namespace LexiConnect.Models
{
    public interface ISender : IEmailSender
    {
        Task SendWelcomeEmailAsync(string toEmail, string userName, string confirmationUrl);
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl);
    }

}