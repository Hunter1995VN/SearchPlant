using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SearchPlant.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using BCrypt.Net;

namespace SearchPlant.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public LoginModel(SearchPlantContext context)
        {
            _context = context;
        }
        [BindProperty]
        public bool Remember { get; set; }


        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;


        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _context.Appusers
                .FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                if (user.Status != "Active")
                {
                    Message = "Tài khoản của bạn chưa xác thực email. Vui lòng kiểm tra email và xác thực trước khi đăng nhập.";
                    return Page();
                }
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
             new Claim("AvatarUrl", string.IsNullOrEmpty(user.Avatarurl) ? "https://www.svgrepo.com/show/382106/male-avatar-boy-face-man-user-9.svg" : user.Avatarurl),
                                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString())
        };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = Remember,
                    ExpiresUtc = Remember ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
                string? returnUrl = HttpContext.Request.Query["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return user.Role == "Admin"
                    ? RedirectToPage("/Admin/Index")
                    : RedirectToPage("/Home/Index");
            }
            Message = "Email hoặc mật khẩu không đúng.";
            return Page();
        }


    }
}
