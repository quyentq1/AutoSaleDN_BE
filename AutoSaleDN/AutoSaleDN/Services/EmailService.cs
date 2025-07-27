using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace AutoSaleDN.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body }; // Changed to "html" for richer content

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls); // Use StartTls
                await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error sending email: {ex.Message}");
                // In a real application, you might re-throw or handle more gracefully
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}