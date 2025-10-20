using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using SearchPlant.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BCrypt.Net; // Thư viện để hash mật khẩu

namespace SearchPlant.Pages.ResetPassword
{
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private readonly IEmailService _emailService;
        public IndexModel(SearchPlantContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

    [BindProperty]
    public InputModel? Input { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

        public class InputModel
        {
            // Trường này sẽ được điền tự động và ẩn đi
            [Required]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Please enter the OTP code.")]
            [Display(Name = "OTP Code")]
            public string? OtpCode { get; set; }

            [Required(ErrorMessage = "Please enter a new password.")]
            [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        public int WaitSeconds { get; set; } = 0;

        public IActionResult OnGet(string email, int? wait)
        {
            if (string.IsNullOrEmpty(email))
            {
                // Nếu không có email, không thể reset, quay về trang chủ
                return RedirectToPage("/Index");
            }
            Input = new InputModel { Email = email };
            WaitSeconds = wait ?? 0;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || Input == null)
            {
                return Page();
            }

            var user = await _context.Appusers.FirstOrDefaultAsync(u => u.Email == Input.Email);

            if (user == null || Input.OtpCode == null || user.PasswordResetToken != Input.OtpCode || user.ResetTokenExpiresAt <= DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP code or the code has expired. Please try again.");
                return Page();
            }

            // Mọi thứ hợp lệ, tiến hành cập nhật mật khẩu
            user.Password = BCrypt.Net.BCrypt.HashPassword(Input.Password ?? ""); // Hash mật khẩu mới

            // Xóa token sau khi đã sử dụng để bảo mật
            user.PasswordResetToken = null;
            user.ResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();

            // Dùng TempData để gửi thông báo thành công về trang Login
            TempData["LoginMessage"] = "Your password has been reset successfully. You can now log in with your new password.";
            return RedirectToPage("/Login/Index");
        }

        public async Task<IActionResult> OnPostResendOtpAsync()
        {
            if (Input == null || string.IsNullOrWhiteSpace(Input.Email))
            {
                StatusMessage = "Vui lòng nhập email.";
                return Page();
            }
            var user = await _context.Appusers.AsNoTracking().FirstOrDefaultAsync(u => u.Email == Input.Email);
            if (user == null)
            {
                StatusMessage = "Nếu email tồn tại, mã OTP đã được gửi lại.";
                return Page();
            }
            var now = DateTime.UtcNow;
            if (user.LastOtpSentAt != null && user.LastOtpSentAt.Value.AddSeconds(60) > now)
            {
                var wait = (user.LastOtpSentAt.Value.AddSeconds(60) - now).Seconds;
                StatusMessage = $"Bạn phải chờ {wait} giây nữa mới được gửi lại mã OTP.";
                return Page();
            }
            // Cho phép gửi lại
            user.LastOtpSentAt = now;
            var otpCode = new Random().Next(100000, 999999).ToString();
            user.PasswordResetToken = otpCode;
            user.ResetTokenExpiresAt = now.AddMinutes(10);
            _context.Appusers.Update(user);
            await _context.SaveChangesAsync();
            // Gửi lại email
            var emailBody = $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
            <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #2E7D32;'>Yêu cầu cấp lại mã OTP</h2>
                <p>Xin chào {user.Username},</p>
                <p>Bạn vừa yêu cầu cấp lại mã OTP để đặt lại mật khẩu cho tài khoản <strong>SearchPlant</strong>.</p>
                <p style='background-color: #f0f0f0; padding: 10px 15px; border-radius: 5px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 2px; color: #2E7D32;'>
                    {otpCode}
                </p>
                <p>Mã này có hiệu lực trong <strong>10 phút</strong>.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này hoặc liên hệ hỗ trợ.</p>
                <hr style='border: none; border-top: 1px solid #eee;'>
                <p style='font-size: 0.9em; color: #777;'>Trân trọng,<br/>Đội ngũ SearchPlant</p>
            </div>
        </div>";
            await _emailService.SendEmailAsync(user.Email, "Mã OTP khôi phục mật khẩu mới", emailBody);
            // Chuyển hướng về lại trang nhập OTP với wait=60 để countdown lại
            return RedirectToPage("/ResetPassword/Index", new { email = Input.Email, wait = 60 });
        }
    }
}