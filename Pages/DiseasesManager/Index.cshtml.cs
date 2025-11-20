using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.DiseasesManager
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
        public int TotalDiseases { get; set; }
        public List<Plantdisease> Diseases { get; set; } = new();

        [BindProperty] public Plantdisease Disease { get; set; } = new();

        public async Task OnGetAsync()
        {
            IQueryable<Plantdisease> query = _context.Plantdiseases.AsNoTracking()
                .Include(d => d.Plants)
                .Include(d => d.Treatments);

            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchLower = SearchString.ToLower().Trim();
                query = query.Where(d => d.Diseasename.ToLower().Contains(searchLower) ||
                                        (d.Symptoms != null && d.Symptoms.ToLower().Contains(searchLower)) ||
                                        (d.Cause != null && d.Cause.ToLower().Contains(searchLower)));
            }

            TotalDiseases = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalDiseases / (double)PageSize);

            Diseases = await query
                .OrderBy(d => d.Diseasename)
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

            var trimmedName = Disease.Diseasename.Trim();
            if (await _context.Plantdiseases.AnyAsync(d => d.Diseasename.ToUpper() == trimmedName.ToUpper()))
            {
                ModelState.AddModelError("Disease.Diseasename", "Bệnh này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            Disease.Diseasename = trimmedName;
            Disease.Symptoms = Disease.Symptoms?.Trim();
            Disease.Cause = Disease.Cause?.Trim();

            _context.Plantdiseases.Add(Disease);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm bệnh thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var disease = await _context.Plantdiseases.FindAsync(id);
            if (disease == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var trimmedName = Disease.Diseasename.Trim();
            if (await _context.Plantdiseases.AnyAsync(d => d.Diseasename.ToUpper() == trimmedName.ToUpper() && d.Diseaseid != id))
            {
                ModelState.AddModelError("Disease.Diseasename", "Bệnh này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            disease.Diseasename = trimmedName;
            disease.Symptoms = Disease.Symptoms?.Trim();
            disease.Cause = Disease.Cause?.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật bệnh thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var disease = await _context.Plantdiseases.FindAsync(id);
            if (disease == null)
            {
                return NotFound();
            }

            // Check if disease is being used
            if (await _context.Plants.AnyAsync(p => p.Diseases.Any(d => d.Diseaseid == id)))
            {
                TempData["ErrorMessage"] = "Không thể xóa bệnh này vì đang được sử dụng bởi cây trồng.";
                return RedirectToPage();
            }

            _context.Plantdiseases.Remove(disease);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bệnh thành công!";
            return RedirectToPage();
        }
    }
}