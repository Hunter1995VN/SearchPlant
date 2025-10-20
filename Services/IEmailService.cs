using System.Threading.Tasks;

namespace SearchPlant.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}