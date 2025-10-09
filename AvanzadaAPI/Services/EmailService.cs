using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AvanzadaAPI.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Sistema Lubricenter";
        public bool EnableSsl { get; set; } = true;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string nombre, string token)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass),
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
                var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/Account/ResetPassword?token={WebUtility.UrlEncode(token)}";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = "Recuperación de Contraseña - Sistema Vehicular",
                    Body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #333;'>Recuperación de Contraseña</h2>
                            <p>Hola <strong>{nombre}</strong>,</p>
                            <p>Has solicitado restablecer tu contraseña en el Sistema Vehicular.</p>
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p style='margin: 0;'>Haz click en el siguiente enlace para restablecer tu contraseña:</p>
                                <p style='margin: 10px 0;'><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Restablecer Contraseña</a></p>
                            </div>
                            <p><strong>Este enlace expirará en 12 horas.</strong></p>
                            <p>Si no solicitaste este cambio, puedes ignorar este mensaje de forma segura.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                            <p style='color: #666; font-size: 12px;'>Este es un mensaje automático, por favor no respondas a este email.</p>
                        </div>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);

                _logger.LogInformation($"Email de recuperación enviado a {email}");
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error SMTP enviando email a {Email}", email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Email}", email);
                return false;
            }
        }
    }
}