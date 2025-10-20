using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.EditPlant
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Plant Plant { get; set; } = new();

        [BindProperty] public List<int> SelectedTypeIds { get; set; } = new();
        [BindProperty] public List<int> SelectedRegionIds { get; set; } = new();
        [BindProperty] public int? SelectedCycleId { get; set; }
        [BindProperty] public List<int> SelectedSeasonIds { get; set; } = new();
        [BindProperty] public List<int> SelectedSoilIds { get; set; } = new();
        [BindProperty] public List<int> SelectedFertilizerIds { get; set; } = new();
        [BindProperty] public List<int> SelectedDiseaseIds { get; set; } = new();
        [BindProperty] public List<int> SelectedPropertyIds { get; set; } = new();
        [BindProperty] public List<int> SelectedWateringIds { get; set; } = new();
        [BindProperty] public List<int> SelectedStatusIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public SelectList PlanttypeOptions { get; set; }
        public SelectList RegionOptions { get; set; }
        public SelectList CycleOptions { get; set; }
        public SelectList SeasonOptions { get; set; }
        public SelectList SoilOptions { get; set; }
        public SelectList FertilizerOptions { get; set; }
        public SelectList DiseaseOptions { get; set; }
        public SelectList PlantpropertyOptions { get; set; }
        public SelectList WateringmethodOptions { get; set; }
        public SelectList ConservationstatusOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Plant = await _context.Plants
                .Include(p => p.Types).Include(p => p.Regions).Include(p => p.Cycle)
                .Include(p => p.Seasons).Include(p => p.Soils).Include(p => p.Fertilizers)
                .Include(p => p.Diseases).Include(p => p.Properties)
                .Include(p => p.Waterings).Include(p => p.Statuses)
                .AsNoTracking().FirstOrDefaultAsync(p => p.Plantid == id);

            ReturnUrl ??= "/PlantsManager/Index";
            if (Plant == null) return NotFound();

            SelectedTypeIds = Plant.Types.Select(t => t.Typeid).ToList();
            SelectedRegionIds = Plant.Regions.Select(r => r.Regionid).ToList();
            SelectedCycleId = Plant.Cycle?.Cycleid;
            SelectedSeasonIds = Plant.Seasons.Select(s => s.Seasonid).ToList();
            SelectedSoilIds = Plant.Soils.Select(s => s.Soilid).ToList();
            SelectedFertilizerIds = Plant.Fertilizers.Select(f => f.Fertilizerid).ToList();
            SelectedDiseaseIds = Plant.Diseases.Select(d => d.Diseaseid).ToList();
            SelectedPropertyIds = Plant.Properties.Select(p => p.Propertyid).ToList();
            SelectedWateringIds = Plant.Waterings.Select(w => w.Wateringid).ToList();
            SelectedStatusIds = Plant.Statuses.Select(s => s.Statusid).ToList();
            
            await LoadOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            var plantToUpdate = await _context.Plants
                .Include(p => p.Types).Include(p => p.Regions).Include(p => p.Seasons)
                .Include(p => p.Soils).Include(p => p.Fertilizers).Include(p => p.Diseases)
                .Include(p => p.Properties).Include(p => p.Waterings).Include(p => p.Statuses)
                .FirstOrDefaultAsync(p => p.Plantid == Plant.Plantid);

            if (plantToUpdate == null) return NotFound();

            // Cập nhật các thuộc tính cơ bản
            _context.Entry(plantToUpdate).CurrentValues.SetValues(Plant);
            plantToUpdate.Lastupdated = DateTime.UtcNow;

            await UpdateManyToMany(plantToUpdate.Types, SelectedTypeIds, id => _context.Planttypes.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Regions, SelectedRegionIds, id => _context.Regions.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Seasons, SelectedSeasonIds, id => _context.Seasons.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Soils, SelectedSoilIds, id => _context.Soiltypes.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Fertilizers, SelectedFertilizerIds, id => _context.Fertilizers.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Diseases, SelectedDiseaseIds, id => _context.Plantdiseases.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Properties, SelectedPropertyIds, id => _context.Plantproperties.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Waterings, SelectedWateringIds, id => _context.Wateringmethods.FindAsync(id));
            await UpdateManyToMany(plantToUpdate.Statuses, SelectedStatusIds, id => _context.Conservationstatuses.FindAsync(id));
            plantToUpdate.Cycleid = SelectedCycleId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật thành công cây '{plantToUpdate.Plantname}'.";
            return Redirect(ReturnUrl ?? "/PlantsManager/Index");
        }

        private async Task LoadOptionsAsync()
        {
            PlanttypeOptions = new SelectList(await _context.Planttypes.OrderBy(t => t.Typename).ToListAsync(), "Typeid", "Typename");
            RegionOptions = new SelectList(await _context.Regions.OrderBy(r => r.Regionname).ToListAsync(), "Regionid", "Regionname");
            CycleOptions = new SelectList(await _context.Growthcycletypes.OrderBy(c => c.Cyclename).ToListAsync(), "Cycleid", "Cyclename");
            SeasonOptions = new SelectList(await _context.Seasons.OrderBy(s => s.Seasonname).ToListAsync(), "Seasonid", "Seasonname");
            SoilOptions = new SelectList(await _context.Soiltypes.OrderBy(s => s.Soilname).ToListAsync(), "Soilid", "Soilname");
            FertilizerOptions = new SelectList(await _context.Fertilizers.OrderBy(f => f.Fertilizername).ToListAsync(), "Fertilizerid", "Fertilizername");
            DiseaseOptions = new SelectList(await _context.Plantdiseases.OrderBy(d => d.Diseasename).ToListAsync(), "Diseaseid", "Diseasename");
            PlantpropertyOptions = new SelectList(await _context.Plantproperties.OrderBy(p => p.Propertyname).ToListAsync(), "Propertyid", "Propertyname");
            WateringmethodOptions = new SelectList(await _context.Wateringmethods.OrderBy(w => w.Methodname).ToListAsync(), "Wateringid", "Methodname");
            ConservationstatusOptions = new SelectList(await _context.Conservationstatuses.OrderBy(c => c.Statusname).ToListAsync(), "Statusid", "Statusname");
        }

        private async Task UpdateManyToMany<T>(ICollection<T> navigationProperty, List<int> selectedIds, Func<int, ValueTask<T?>> findAsync) where T : class
        {
            navigationProperty.Clear();
            foreach (var id in selectedIds)
            {
                var entity = await findAsync(id);
                if (entity != null)
                {
                    navigationProperty.Add(entity);
                }
            }
        }
    }
}