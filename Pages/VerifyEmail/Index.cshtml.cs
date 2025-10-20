using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SearchPlant.Models;
using Microsoft.EntityFrameworkCore;

namespace SearchPlant.Pages.VerifyEmail
{
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Message = "Token xác thực không hợp lệ.";
                IsSuccess = false;
                return Page();
            }
            var user = await _context.Appusers.FirstOrDefaultAsync(u => u.EmailVerificationToken == token && u.Status == "PendingVerification");
            if (user == null)
            {
                Message = "Token không hợp lệ hoặc đã hết hạn.";
                IsSuccess = false;
                return Page();
            }
            user.Status = "Active";
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            await _context.SaveChangesAsync();
            Message = "Xác thực email thành công! Bạn có thể đăng nhập.";
            IsSuccess = true;
            return Page();
        }
    }
}
