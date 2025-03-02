using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Web.Services
{
    public class PapercutEmailSenderService : IEmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _fromEmail;

        public PapercutEmailSenderService(IConfiguration configuration)
        {
            _smtpHost = configuration["Papercut:SmtpHost"] ?? "localhost"; // Default: Papercut SMTP
            _smtpPort = int.Parse(configuration["Papercut:SmtpPort"] ?? "25"); // Default: Port 25
            _fromEmail = configuration["Papercut:FromEmail"] ?? "noreply@yourapp.com";
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("", ""); // No credentials needed for local SMTP

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "YourApp"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}