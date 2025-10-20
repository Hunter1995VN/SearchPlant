using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Drawing.Charts;
namespace SearchPlant.Pages.EditUser
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlant.Models.SearchPlantContext _context;

        public IndexModel(SearchPlant.Models.SearchPlantContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Appuser AppUser { get; set; }
        [BindProperty]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Display(Name = "Conform New Password")]
        [Compare("NewPassword", ErrorMessage = " New password not match with conform password.")]
        public string? ConfirmPassword { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AppUser = await _context.Appusers.FirstOrDefaultAsync(m => m.Userid == id);

            if (AppUser == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("AppUser.Password");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Lấy entity gốc từ database để tránh cập nhật các trường không mong muốn
            var userToUpdate = await _context.Appusers.FindAsync(AppUser.Userid);

            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Chỉ cập nhật các trường được phép thay đổi từ form
            userToUpdate.Username = AppUser.Username;
            userToUpdate.Email = AppUser.Email;
            userToUpdate.Role = AppUser.Role;
            userToUpdate.Status = AppUser.Status;
            userToUpdate.Avatarurl = AppUser.Avatarurl;
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (string.IsNullOrEmpty(ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", " Conform password is empty please input it.");
                    return Page();
                }
                else
                {
                    userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Appusers.Any(e => e.Userid == AppUser.Userid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/UserManager/Index");
        }
    }
}