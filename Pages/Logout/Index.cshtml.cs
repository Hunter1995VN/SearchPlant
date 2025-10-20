using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SearchPlant.Pages.Logout
{
    public class LogoutModel : PageModel
    {

//         public async Task<IActionResult> OnGetAsync()
// {
//     await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//     return RedirectToPage("/Home/Index"); // hoặc RedirectToPage("/Index")
// }
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Home/Index"); // hoặc RedirectToPage("/Index") nếu bạn dùng Index.cshtml
        }
    }
}
