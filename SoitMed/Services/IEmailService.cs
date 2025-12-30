using System.Threading.Tasks;

namespace SoitMed.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendPasswordResetEmailAsync(string to, string resetCode, string firstName, string lastName);
        Task<bool> SendEmailVerificationCodeAsync(string to, string verificationCode, string firstName, string lastName);
        Task<bool> TestSmtpConnectionAsync();
    }
}
