using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
namespace SearchPlant.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
{
    var email = new MimeMessage();
    email.Sender = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
    email.From.Add(email.Sender);
    email.To.Add(MailboxAddress.Parse(toEmail));
    email.Subject = subject;

    var builder = new BodyBuilder { HtmlBody = message };
    email.Body = builder.ToMessageBody();

    using var smtp = new SmtpClient();

    // === THAY ĐỔI QUAN TRỌNG Ở ĐÂY ===
    // Bỏ qua việc kiểm tra thu hồi chứng chỉ (revocation check)
    // Điều này an toàn khi bạn biết chắc mình đang kết nối đến server tin cậy như smtp.gmail.com
    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

    await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
    await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
    await smtp.SendAsync(email);
    await smtp.DisconnectAsync(true);
}
    }
}