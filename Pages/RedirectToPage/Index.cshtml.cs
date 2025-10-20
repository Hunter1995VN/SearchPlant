using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SearchPlant.Pages.RedirectToPage
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // if (!User.Identity.IsAuthenticated)
            // {
            //     return RedirectToPage("/Login");
            // }

            if (User.IsInRole("Admin"))
            {
                return RedirectToPage("/Admin/Index");
            }

            return RedirectToPage("/Home/Index");
        }
    }
}
