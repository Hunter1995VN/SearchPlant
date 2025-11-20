using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using System;
using GrapeCity.ActiveReports.Rdl.Themes; // Thêm using này cho Guid

namespace SearchPlant.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SearchPlant.Models.SearchPlantContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Các thuộc tính để hiển thị trên trang
        public string Username { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public bool HasPassword { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public ProfileInputModel ProfileInput { get; set; }

        [BindProperty]
        public ChangePasswordInputModel ChangePasswordInput { get; set; }

        // <<< ĐÃ XÓA >>> ChangePasswordInputModel không còn cần thiết

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "Tên người dùng không được để trống.")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên người dùng phải từ 3 đến 50 ký tự.")]
            public string Username { get; set; }

            public string AvatarUrl { get; set; }
        }

        public class ChangePasswordInputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string? CurrentPassword { get; set; }

            [Required(ErrorMessage = "Please enter a new password.")]
            [StringLength(100, ErrorMessage = "The password must be at least 6 characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        // <<< ĐÃ XÓA >>> class ChangePasswordInputModel không còn cần thiết

        private async Task<Appuser> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;
            return await _context.Appusers.FindAsync(int.Parse(userId));
        }

        private async Task LoadUserInfoAsync(Appuser user)
        {
            Username = user.Username;
            Email = user.Email;
            AvatarUrl = user.Avatarurl ?? "/images/default-avatar.png";
            ProfileInput = new ProfileInputModel { Username = user.Username, AvatarUrl = user.Avatarurl ?? "" };
            HasPassword = !string.IsNullOrEmpty(user.Password);
        }

        private async Task RefreshSignInSign(Appuser user)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("AvatarUrl", user.Avatarurl?? "/images/avatars/default.png")
            };
            var claimidentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // Dòng này sẽ tạo ra một cookie mới và ghi đè lên cookie cũ trong trình duyệt của người dùng

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimidentity));
            _logger.LogInformation($"User {user.Userid} signed in again with updated claims (new avatar: {user.Avatarurl}).");

        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Forbid();

            await LoadUserInfoAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            _logger.LogInformation("OnPostUpdateProfileAsync called.");
            var user = await GetCurrentUserAsync();
            if (user == null) return Forbid();

            if (ProfileInput == null)
            {
                _logger.LogWarning("ProfileInput bound as null while calling OnPostUpdateProfileAsync for user {UserId}", user.Userid);
            }
            // Clear non-related validation and validate only ProfileInput
            ModelState.Clear();
            if (ProfileInput == null || !TryValidateModel(ProfileInput, nameof(ProfileInput)))
            {
                // Log ModelState errors to help debugging
                foreach (var kvp in ModelState.Where(m => (m.Value?.Errors?.Count ?? 0) > 0))
                {
                    foreach (var error in kvp.Value?.Errors ?? new Microsoft.AspNetCore.Mvc.ModelBinding.ModelErrorCollection())
                    {
                        _logger.LogWarning("Validation error in {Field}: {Error}", kvp.Key, error.ErrorMessage);
                    }
                }
                await LoadUserInfoAsync(user);
                return Page();
            }

            _logger.LogInformation($"Updating user {user.Userid}: Username to {ProfileInput.Username}, AvatarUrl to {ProfileInput.AvatarUrl ?? "null"}");

            user.Username = (ProfileInput.Username ?? "").Trim();
            // Chỉ cập nhật AvatarUrl nếu người dùng nhập gì đó vào ô URL
            var profileAvatar = (ProfileInput.AvatarUrl ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(profileAvatar))
            {
                user.Avatarurl = profileAvatar;
            }

            _logger.LogInformation("Saving profile changes for {UserId}: Username={Username}, AvatarUrl={Avatar}", user.Userid, user.Username, user.Avatarurl);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save profile changes for user {UserId}", user.Userid);
                ModelState.AddModelError(string.Empty, "Không thể lưu thay đổi. Vui lòng thử lại hoặc liên hệ quản trị.");
                await LoadUserInfoAsync(user);
                return Page();
            }
            await RefreshSignInSign(user);

            // Cập nhật lại thông tin đăng nhập (cookie) để tên mới hiển thị ngay lập tức
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
            new Claim("AvatarUrl", user.Avatarurl?? "/images/avatars/default.png")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            StatusMessage = "Hồ sơ đã được cập nhật thành công.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAvatarAsync(IFormFile AvatarFile)
        {
            _logger.LogInformation("OnPostUpdateAvatarAsync called.");
            var user = await GetCurrentUserAsync();
            if (user == null) return Forbid();

            // <<< ĐÃ XÓA >>> Các dòng ModelState.Remove không còn cần thiết vì chỉ có 1 form được submit

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                user.Avatarurl = $"/images/avatars/{fileName}";
                await _context.SaveChangesAsync();
                await RefreshSignInSign(user);
                StatusMessage = "Avatar đã được cập nhật thành công.";
            }
            else
            {
                StatusMessage = "Vui lòng chọn một file ảnh.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Forbid();

            _logger.LogInformation("User {UserId} attempting to change password", user.Userid);

            // Clear page-level validation state and validate only the ChangePasswordInput model
            ModelState.Clear();
            if (!TryValidateModel(ChangePasswordInput, nameof(ChangePasswordInput)))
            {
                await LoadUserInfoAsync(user);
                return Page();
            }

            // If user has a password set, require the current password to match
            if (!string.IsNullOrEmpty(user.Password))
            {
                if (string.IsNullOrEmpty(ChangePasswordInput.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(ChangePasswordInput.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("ChangePasswordInput.CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    _logger.LogWarning("User {UserId} provided wrong current password.", user.Userid);
                    await LoadUserInfoAsync(user);
                    return Page();
                }
            }

            // Hash and set the new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(ChangePasswordInput.NewPassword ?? string.Empty);
            await _context.SaveChangesAsync();

            // Refresh auth cookie so new credentials are effective immediately
            await RefreshSignInSign(user);
            _logger.LogInformation("User {UserId} password changed successfully.", user.Userid);

            StatusMessage = "Mật khẩu đã được cập nhật thành công.";
            return RedirectToPage();
        }
    }
}