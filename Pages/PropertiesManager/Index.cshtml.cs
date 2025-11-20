using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.PropertiesManager
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private const int PageSize = 10;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)] public string? SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalProperties { get; set; }
        public List<Plantproperty> Properties { get; set; } = new();

        [BindProperty] public Plantproperty Property { get; set; } = new();

        public async Task OnGetAsync()
        {
            IQueryable<Plantproperty> query = _context.Plantproperties.AsNoTracking();

            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchLower = SearchString.ToLower().Trim();
                query = query.Where(p => p.Propertyname.ToLower().Contains(searchLower) ||
                                        (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            TotalProperties = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalProperties / (double)PageSize);

            Properties = await query
                .OrderBy(p => p.Propertyname)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var trimmedName = Property.Propertyname.Trim();
            if (await _context.Plantproperties.AnyAsync(p => p.Propertyname.ToUpper() == trimmedName.ToUpper()))
            {
                ModelState.AddModelError("Property.Propertyname", "Đặc tính này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            Property.Propertyname = trimmedName;
            Property.Description = Property.Description?.Trim();

            _context.Plantproperties.Add(Property);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm đặc tính thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var property = await _context.Plantproperties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var trimmedName = Property.Propertyname.Trim();
            if (await _context.Plantproperties.AnyAsync(p => p.Propertyname.ToUpper() == trimmedName.ToUpper() && p.Propertyid != id))
            {
                ModelState.AddModelError("Property.Propertyname", "Đặc tính này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            property.Propertyname = trimmedName;
            property.Description = Property.Description?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật đặc tính thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var property = await _context.Plantproperties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            // Check if property is being used
            if (await _context.Plants.AnyAsync(p => p.Properties.Any(prop => prop.Propertyid == id)))
            {
                TempData["ErrorMessage"] = "Không thể xóa đặc tính này vì đang được sử dụng bởi cây trồng.";
                return RedirectToPage();
            }

            _context.Plantproperties.Remove(property);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa đặc tính thành công!";
            return RedirectToPage();
        }
    }
}