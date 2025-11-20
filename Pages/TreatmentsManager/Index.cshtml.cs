using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.TreatmentsManager
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
        public int TotalTreatments { get; set; }
        public int TotalDiseases { get; set; }
        public int TotalPlants { get; set; }
        public List<Treatment> Treatments { get; set; } = new();

        [BindProperty] public Treatment Treatment { get; set; } = new();
        [BindProperty] public List<int> SelectedDiseaseIds { get; set; } = new();

        public List<SelectListItem> Diseases { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Load diseases for dropdown
            Diseases = await _context.Plantdiseases
                .OrderBy(d => d.Diseasename)
                .Select(d => new SelectListItem { Value = d.Diseaseid.ToString(), Text = d.Diseasename })
                .ToListAsync();

            IQueryable<Treatment> query = _context.Treatments.AsNoTracking()
                .Include(t => t.Diseases);

            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchLower = SearchString.ToLower().Trim();
                query = query.Where(t => t.Treatmentname.ToLower().Contains(searchLower) ||
                                        (t.Description != null && t.Description.ToLower().Contains(searchLower)) ||
                                        (t.Methodtype != null && t.Methodtype.ToLower().Contains(searchLower)));
            }

            TotalTreatments = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalTreatments / (double)PageSize);

            // Calculate totals for stats cards
            // TotalDiseases should be unique diseases that are linked to at least one treatment
            TotalDiseases = await _context.Treatments
                .SelectMany(t => t.Diseases)
                .Select(d => d.Diseaseid)
                .Distinct()
                .CountAsync();

            // TotalPlants should be distinct plants that are associated to diseases which have at least one treatment
            TotalPlants = await _context.Plants
                .Where(p => p.Diseases.Any(d => d.Treatments.Any()))
                .CountAsync();

            Treatments = await query
                .OrderBy(t => t.Treatmentname)
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

            var trimmedName = Treatment.Treatmentname.Trim();
            if (await _context.Treatments.AnyAsync(t => t.Treatmentname.ToUpper() == trimmedName.ToUpper()))
            {
                ModelState.AddModelError("Treatment.Treatmentname", "Phương pháp trị liệu này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            Treatment.Treatmentname = trimmedName;
            Treatment.Methodtype = Treatment.Methodtype?.Trim();
            Treatment.Description = Treatment.Description?.Trim();
            Treatment.Precautions = Treatment.Precautions?.Trim();

            // Add selected diseases
            if (SelectedDiseaseIds != null && SelectedDiseaseIds.Any())
            {
                var diseases = await _context.Plantdiseases
                    .Where(d => SelectedDiseaseIds.Contains(d.Diseaseid))
                    .ToListAsync();
                foreach (var disease in diseases)
                {
                    Treatment.Diseases.Add(disease);
                }
            }

            _context.Treatments.Add(Treatment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm phương pháp trị liệu thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            var treatment = await _context.Treatments
                .Include(t => t.Diseases)
                .FirstOrDefaultAsync(t => t.Treatmentid == id);
            if (treatment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var trimmedName = Treatment.Treatmentname.Trim();
            if (await _context.Treatments.AnyAsync(t => t.Treatmentname.ToUpper() == trimmedName.ToUpper() && t.Treatmentid != id))
            {
                ModelState.AddModelError("Treatment.Treatmentname", "Phương pháp trị liệu này đã tồn tại.");
                await OnGetAsync();
                return Page();
            }

            treatment.Treatmentname = trimmedName;
            treatment.Methodtype = Treatment.Methodtype?.Trim();
            treatment.Description = Treatment.Description?.Trim();
            treatment.Precautions = Treatment.Precautions?.Trim();

            // Update diseases
            treatment.Diseases.Clear();
            if (SelectedDiseaseIds != null && SelectedDiseaseIds.Any())
            {
                var diseases = await _context.Plantdiseases
                    .Where(d => SelectedDiseaseIds.Contains(d.Diseaseid))
                    .ToListAsync();
                foreach (var disease in diseases)
                {
                    treatment.Diseases.Add(disease);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật phương pháp trị liệu thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            _context.Treatments.Remove(treatment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa phương pháp trị liệu thành công!";
            return RedirectToPage();
        }
    }
}