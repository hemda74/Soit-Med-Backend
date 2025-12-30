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
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPortStr = _configuration["EmailSettings:SmtpPort"] ?? "587";
            
            if (!int.TryParse(smtpPortStr, out int smtpPort))
            {
                _logger.LogError($"Invalid SMTP port configuration: {smtpPortStr}. Using default port 587.");
                smtpPort = 587;
            }
            
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["EmailSettings:FromName"] ?? "SoitMed System";

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("Email configuration is missing. Please configure EmailSettings:SmtpUsername and EmailSettings:SmtpPassword in appsettings.json");
                return false;
            }

            if (string.IsNullOrEmpty(smtpHost))
            {
                _logger.LogError("SMTP host is not configured. Please set EmailSettings:SmtpHost in appsettings.json");
                return false;
            }

            // Try the configured port first
            var result = await TrySendEmail(smtpHost, smtpPort, smtpUsername, smtpPassword, fromEmail ?? smtpUsername, fromName, to, subject, body, isHtml);
            
            if (result)
            {
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }

            // If port 465 fails, try port 587 as fallback
            if (smtpPort == 465)
            {
                _logger.LogWarning($"Failed to send email via port {smtpPort}, trying fallback port 587");
                var fallbackResult = await TrySendEmail(smtpHost, 587, smtpUsername, smtpPassword, fromEmail ?? smtpUsername, fromName, to, subject, body, isHtml);
                if (fallbackResult)
                {
                    _logger.LogInformation($"Email sent successfully via fallback port 587 to {to}");
                    return true;
                }
            }

            _logger.LogError($"Failed to send email to {to} after all attempts");
            return false;
        }

        private async Task<bool> TrySendEmail(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, 
            string fromEmail, string fromName, string to, string subject, string body, bool isHtml)
        {
            try
            {
                // Validate email addresses
                if (string.IsNullOrWhiteSpace(to) || !to.Contains("@"))
                {
                    _logger.LogError($"Invalid recipient email address: '{to}'");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(fromEmail) || !fromEmail.Contains("@"))
                {
                    _logger.LogError($"Invalid sender email address: '{fromEmail}'");
                    return false;
                }

                using var client = new SmtpClient();
                
                // Configure SMTP client based on port
                if (smtpPort == 465)
                {
                    // Port 465 uses implicit SSL (SMTPS)
                    client.Host = smtpHost;
                    client.Port = smtpPort;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                }
                else
                {
                    // Port 587 uses STARTTLS
                    client.Host = smtpHost;
                    client.Port = smtpPort;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                }

                // Set timeout (30 seconds)
                client.Timeout = 30000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail ?? smtpUsername, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;
                message.Priority = MailPriority.Normal;
                
                // Add headers to improve deliverability
                message.Headers.Add("X-Mailer", "SoitMed-System");
                message.Headers.Add("X-Priority", "3");
                message.Headers.Add("Message-ID", $"<{Guid.NewGuid()}@soitmed.com>");
                
                // Add Reply-To header
                if (!string.IsNullOrEmpty(fromEmail))
                {
                    message.ReplyToList.Add(new MailAddress(fromEmail, fromName));
                }

                await client.SendMailAsync(message);
                return true;
            }
            catch (SmtpFailedRecipientsException recipientsEx)
            {
                _logger.LogError(recipientsEx, $"Failed to send email to multiple recipients via {smtpHost}:{smtpPort}");
                for (int i = 0; i < recipientsEx.InnerExceptions.Length; i++)
                {
                    var innerEx = recipientsEx.InnerExceptions[i];
                    _logger.LogError($"Recipient {i + 1}: {innerEx.FailedRecipient} - Status: {innerEx.StatusCode}");
                }
                return false;
            }
            catch (SmtpFailedRecipientException recipientEx)
            {
                _logger.LogError(recipientEx, $"Failed to send email to recipient {to} via {smtpHost}:{smtpPort}");
                _logger.LogError($"Failed Recipient: {recipientEx.FailedRecipient}, Status Code: {recipientEx.StatusCode}");
                return false;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error sending email to {to} via {smtpHost}:{smtpPort}");
                _logger.LogError($"SMTP Status Code: {smtpEx.StatusCode}, Error: {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error sending email to {to} via {smtpHost}:{smtpPort}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string resetCode, string firstName, string lastName)
        {
            var subject = "Password Reset Request - SoitMed";
            
            // Create both HTML and plain text versions for better deliverability
            var htmlBody = $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
                    <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                        <h2 style='color: #2c5aa0; text-align: center; margin-bottom: 20px;'>Password Reset Request</h2>
                        <p style='font-size: 16px; margin-bottom: 15px;'>Hello <strong>{firstName} {lastName}</strong>,</p>
                        <p style='font-size: 16px; margin-bottom: 15px;'>You have requested to reset your password for your SoitMed account.</p>
                        <p style='font-size: 16px; margin-bottom: 20px;'>Please use the following verification code to reset your password:</p>
                        
                        <div style='background-color: #f4f4f4; padding: 25px; text-align: center; margin: 25px 0; border-radius: 8px; border: 2px solid #2c5aa0;'>
                            <h1 style='color: #2c5aa0; font-size: 36px; letter-spacing: 8px; margin: 0; font-weight: bold;'>{resetCode}</h1>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <p style='margin: 0; font-weight: bold; color: #856404;'>Important Security Information:</p>
                            <ul style='margin: 10px 0; padding-left: 20px; color: #856404;'>
                                <li>This code will expire in 15 minutes</li>
                                <li>Do not share this code with anyone</li>
                                <li>If you didn't request this reset, please ignore this email</li>
                            </ul>
                        </div>
                        
                        <p style='font-size: 16px; margin: 25px 0;'>If you have any questions or need assistance, please contact our support team.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <div style='text-align: center; padding: 20px 0; background-color: #f8f9fa; border-radius: 5px;'>
                            <p style='font-size: 12px; color: #666; margin: 0;'>
                                This is an automated message from <strong>SoitMed System</strong><br>
                                Please do not reply to this email
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            // Plain text version for better compatibility
            var plainTextBody = $@"
Password Reset Request - SoitMed

Hello {firstName} {lastName},

You have requested to reset your password for your SoitMed account.

Your verification code is: {resetCode}

Important Security Information:
- This code will expire in 15 minutes
- Do not share this code with anyone
- If you didn't request this reset, please ignore this email

If you have any questions or need assistance, please contact our support team.

---
This is an automated message from SoitMed System
Please do not reply to this email
";

            // Try HTML first, then fallback to plain text if needed
            var result = await SendEmailAsync(to, subject, htmlBody, true);
            
            if (!result)
            {
                _logger.LogWarning("HTML email failed, trying plain text version...");
                result = await SendEmailAsync(to, subject, plainTextBody, false);
            }
            
            return result;
        }

        public async Task<bool> SendEmailVerificationCodeAsync(string to, string verificationCode, string firstName, string lastName)
        {
            var subject = "Email Verification Code - SoitMed";
            var logoUrl = GetLogoUrl();
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff;'>
                        <!-- Header with Logo -->
                        <div style='text-align: center; margin-bottom: 30px; padding: 20px 0; border-bottom: 2px solid #2c5aa0;'>
                            <img src='{logoUrl}' alt='SoitMed Logo' style='max-height: 80px; max-width: 200px; height: auto; width: auto;' />
                        </div>
                        
                        <h2 style='color: #2c5aa0; text-align: center; margin-bottom: 20px;'>Email Verification</h2>
                        <p style='font-size: 16px; margin-bottom: 15px;'>Hello <strong>{firstName} {lastName}</strong>,</p>
                        <p style='font-size: 16px; margin-bottom: 15px;'>Welcome to SoitMed! Please verify your email address using the following code:</p>
                        
                        <div style='background-color: #f4f4f4; padding: 25px; text-align: center; margin: 25px 0; border-radius: 8px; border: 2px solid #2c5aa0;'>
                            <h1 style='color: #2c5aa0; font-size: 36px; letter-spacing: 8px; margin: 0; font-weight: bold;'>{verificationCode}</h1>
                        </div>
                        
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; border-left: 4px solid #17a2b8; margin: 20px 0;'>
                            <p style='margin: 0; font-weight: bold; color: #0c5460;'>Verification Instructions:</p>
                            <ul style='margin: 10px 0; padding-left: 20px; color: #0c5460;'>
                                <li>This code will expire in 15 minutes</li>
                                <li>Do not share this code with anyone</li>
                                <li>If you didn't request this verification, please ignore this email</li>
                            </ul>
                        </div>
                        
                        <p style='font-size: 16px; margin: 25px 0;'>If you have any questions or need assistance, please contact our support team.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        
                        <div style='text-align: center; padding: 20px 0; background-color: #f8f9fa; border-radius: 5px;'>
                            <p style='font-size: 12px; color: #666; margin: 0;'>
                                This is an automated message from <strong>SoitMed System</strong><br>
                                Please do not reply to this email
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(to, subject, body);
        }

        private string GetLogoUrl()
        {
            // Get base URL from configuration
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:58868";
            var logoUrl = $"{baseUrl}/images/soitmed-logo.png";
            
            return logoUrl;
        }

        private string GetEmbeddedLogoBase64()
        {
            // Replace this with your actual logo's base64 string
            // You can convert your logo to base64 using online tools or:
            // Convert.ToBase64String(File.ReadAllBytes("path/to/your/logo.png"))
            return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="; // 1x1 transparent pixel placeholder
        }

        public async Task<bool> TestSmtpConnectionAsync()
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPortStr = _configuration["EmailSettings:SmtpPort"] ?? "587";
            
            if (!int.TryParse(smtpPortStr, out int smtpPort))
            {
                _logger.LogError($"Invalid SMTP port configuration: {smtpPortStr}");
                return false;
            }
            
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogError("Email configuration is missing. Please configure EmailSettings in appsettings.json");
                return false;
            }

            try
            {
                _logger.LogInformation($"Testing SMTP connection to {smtpHost}:{smtpPort}...");

                using var client = new SmtpClient();
                client.Host = smtpHost;
                client.Port = smtpPort;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.Timeout = 10000; // 10 seconds for test
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                // Try to send a test email to ourselves
                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail ?? smtpUsername, "SoitMed Test");
                message.To.Add(fromEmail ?? smtpUsername); // Send to ourselves
                message.Subject = "SMTP Connection Test";
                message.Body = "This is a test email to verify SMTP configuration.";
                message.IsBodyHtml = false;

                await client.SendMailAsync(message);
                _logger.LogInformation($"SMTP connection test successful! Email sent to {fromEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP connection test failed");
                _logger.LogError($"SMTP Status Code: {smtpEx.StatusCode}");
                _logger.LogError($"Error: {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMTP connection test failed with unexpected error");
                _logger.LogError($"Error Type: {ex.GetType().Name}");
                _logger.LogError($"Error Message: {ex.Message}");
                return false;
            }
        }
    }
}
