using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SearchPlant.Models;
using SearchPlant.Services;
using Microsoft.EntityFrameworkCore;
namespace SearchPlant.Pages.Forgot
{
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(SearchPlantContext context, IEmailService emailService, ILogger<IndexModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty] public string Email { get; set; } = string.Empty;
        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Validation (Giữ nguyên code cũ của bạn)
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Enter your email.";
                return Page();
            }

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailPattern))
            {
                Message = "Email type is not valid.";
                return Page();
            }

            // 2. Tìm người dùng
            var user = await _context.Appusers.FirstOrDefaultAsync(u => u.Email == Email);

            // Mẹo bảo mật: Kể cả khi không tìm thấy email, vẫn nên hiển thị thông báo chung chung
            // để kẻ xấu không thể dò ra email nào đã tồn tại trong hệ thống.
            if (user == null)
            {
                Message = "If an account with that email exists, a recovery code has been sent.";
                return Page();
            }

            // 3. Tạo mã OTP (6 chữ số)
            var otpCode = new Random().Next(100000, 999999).ToString();

            // 4. Lưu mã OTP, thời gian hết hạn (10 phút) và LastOtpSentAt vào DB
            var now = DateTime.UtcNow;
            user.PasswordResetToken = otpCode;
            user.ResetTokenExpiresAt = now.AddMinutes(10);
            user.LastOtpSentAt = now;
            await _context.SaveChangesAsync();

            // 5. Soạn nội dung email (HTML cho đẹp)
            var emailBody = $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
            <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #2E7D32;'>Password Reset Request</h2>
                <p>Hello {user.Username},</p>
                <p>We received a request to reset your password for your <strong>SearchPlant</strong> account. Please use the code below to complete the process.</p>
                <p style='background-color: #f0f0f0; padding: 10px 15px; border-radius: 5px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 2px; color: #2E7D32;'>
                    {otpCode}
                </p>
                <p>This code is valid for <strong>10 minutes</strong>.</p>
                <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                <hr style='border: none; border-top: 1px solid #eee;'>
                <p style='font-size: 0.9em; color: #777;'>Thank you,<br/>The SearchPlant Team</p>
            </div>
        </div>";

            // 6. Gửi email
            try
            {
                await _emailService.SendEmailAsync(Email, "Your SearchPlant Password Reset Code", emailBody);
            }
            catch (Exception ex)
            {
                    _logger.LogError(ex, "Lỗi nghiêm trọng khi gửi email khôi phục mật khẩu cho {UserEmail}", Email);

                // Nếu có lỗi khi gửi mail (ví dụ sai mật khẩu SMTP), chúng ta cần xử lý
                // Trong thực tế bạn nên ghi log lỗi này lại.
                // Console.WriteLine(ex.Message); 
                Message = "There was an error sending the recovery email. Please try again later.";
                return Page();
            }

            // 7. Chuyển hướng đến trang nhập mã OTP, truyền thời gian chờ còn lại
            var waitSeconds = 60;
            TempData["StatusMessage"] = "A recovery code has been sent to your email. It will expire in 10 minutes.";
            return RedirectToPage("/ResetPassword/Index", new { email = Email, wait = waitSeconds });
        }
    }
}
