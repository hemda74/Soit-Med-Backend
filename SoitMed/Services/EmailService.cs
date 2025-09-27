using System.Net;
using System.Net.Mail;
using System.Text;

namespace SoitMed.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
                var fromName = _configuration["EmailSettings:FromName"] ?? "SoitMed System";

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Email configuration is missing. Please configure EmailSettings in appsettings.json");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string resetCode, string userName)
        {
            var subject = "Password Reset Request - SoitMed";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5aa0; text-align: center;'>SoitMed Password Reset</h2>
                        <p>Hello {userName},</p>
                        <p>You have requested to reset your password for your SoitMed account.</p>
                        <p>Please use the following verification code to reset your password:</p>
                        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                            <h1 style='color: #2c5aa0; font-size: 32px; letter-spacing: 5px; margin: 0;'>{resetCode}</h1>
                        </div>
                        <p><strong>Important:</strong></p>
                        <ul>
                            <li>This code will expire in 15 minutes</li>
                            <li>Do not share this code with anyone</li>
                            <li>If you didn't request this reset, please ignore this email</li>
                        </ul>
                        <p>If you have any questions, please contact our support team.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from SoitMed System. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendEmailVerificationCodeAsync(string to, string verificationCode, string userName)
        {
            var subject = "Email Verification Code - SoitMed";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5aa0; text-align: center;'>SoitMed Email Verification</h2>
                        <p>Hello {userName},</p>
                        <p>Please verify your email address using the following code:</p>
                        <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                            <h1 style='color: #2c5aa0; font-size: 32px; letter-spacing: 5px; margin: 0;'>{verificationCode}</h1>
                        </div>
                        <p><strong>Important:</strong></p>
                        <ul>
                            <li>This code will expire in 15 minutes</li>
                            <li>Do not share this code with anyone</li>
                            <li>If you didn't request this verification, please ignore this email</li>
                        </ul>
                        <p>If you have any questions, please contact our support team.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>
                            This is an automated message from SoitMed System. Please do not reply to this email.
                        </p>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body);
        }
    }
}
