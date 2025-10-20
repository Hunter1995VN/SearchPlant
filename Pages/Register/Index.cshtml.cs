using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SearchPlant.Models;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Services;

namespace SearchPlant.Pages.Register
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

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
    public string? Message { get; set; }

   public async Task<IActionResult> OnPostAsync()
{
    if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
    {
        Message = "Tên người dùng phải có ít nhất 3 ký tự.";
        return Page();
    }
    if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@") || !Email.Contains("."))
    {
        Message = "Email không hợp lệ.";
        return Page();
    }
    if (Password.Length < 6)
    {
        Message = "Mật khẩu phải có ít nhất 6 ký tự.";
        return Page();
    }
    if (Password != ConfirmPassword)
    {
        Message = "Mật khẩu xác nhận không khớp.";
        return Page();
    }
    var exists = await _context.Appusers.AnyAsync(u => u.Email == Email);
    if (exists)
    {
        Message = "Email đã được sử dụng.";
        return Page();
    }
    // Sinh token xác thực
    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("+", "-").Replace("/", "_").Replace("=", "");
    var newUser = new Appuser
    {
        Username = Username,
        Email = Email,
        Password = BCrypt.Net.BCrypt.HashPassword(Password),
        Role = "User",
        CreatedAt = DateTime.UtcNow,
        Status = "PendingVerification",
        EmailVerificationToken = token
    };
    _context.Appusers.Add(newUser);
    await _context.SaveChangesAsync();
    // Tạo link xác thực
    var verificationLink = Url.Page(
        "/VerifyEmail/Index",
        pageHandler: null,
        values: new { token = token },
        protocol: Request.Scheme);
    var emailBody = $@"
<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
        <h2 style='color: #2E7D32;'>Chào mừng bạn đến với SearchPlant!</h2>
        <p>Xin chào <strong>{Username}</strong>,</p>
        <p>Cảm ơn bạn đã đăng ký tài khoản SearchPlant. Vui lòng nhấp vào nút bên dưới để xác thực tài khoản của bạn:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{verificationLink}' style='display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Xác thực tài khoản</a>
        </div>
        <p>Hoặc copy link sau vào trình duyệt:</p>
        <p style='background-color: #f0f0f0; padding: 10px; border-radius: 5px; word-break: break-all;'>{verificationLink}</p>
        <p>Link này sẽ có hiệu lực trong 24 giờ.</p>
        <p>Nếu bạn không yêu cầu đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
        <p style='font-size: 0.9em; color: #777;'>Trân trọng,<br/>Đội ngũ SearchPlant</p>
    </div>
</div>";
    // Gửi email xác thực
    try
    {
        await _emailService.SendEmailAsync(newUser.Email, "Xác thực tài khoản SearchPlant", emailBody);
    }
    catch (Exception ex)
    {
        Message = $"Đăng ký thành công nhưng gửi email xác thực thất bại: {ex.Message}";
        return Page();
    }
    Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
    return Page();
}

}

}
