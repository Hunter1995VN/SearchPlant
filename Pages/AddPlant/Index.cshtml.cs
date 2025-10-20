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

namespace SearchPlant.Pages.AddPlant
{

    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        [BindProperty] public Plant Plant { get; set; } = new();
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

        public async Task<IActionResult> OnGetAsync()
        {
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

            Plant.Types = await _context.Planttypes.Where(t => SelectedTypeIds.Contains(t.Typeid)).ToListAsync();
            Plant.Regions = await _context.Regions.Where(r => SelectedRegionIds.Contains(r.Regionid)).ToListAsync();
            Plant.Seasons = await _context.Seasons.Where(s => SelectedSeasonIds.Contains(s.Seasonid)).ToListAsync();
            Plant.Soils = await _context.Soiltypes.Where(s => SelectedSoilIds.Contains(s.Soilid)).ToListAsync();
            Plant.Fertilizers = await _context.Fertilizers.Where(f => SelectedFertilizerIds.Contains(f.Fertilizerid)).ToListAsync();
            Plant.Diseases = await _context.Plantdiseases.Where(d => SelectedDiseaseIds.Contains(d.Diseaseid)).ToListAsync();
            Plant.Properties = await _context.Plantproperties.Where(p => SelectedPropertyIds.Contains(p.Propertyid)).ToListAsync();
            Plant.Waterings = await _context.Wateringmethods.Where(w => SelectedWateringIds.Contains(w.Wateringid)).ToListAsync();
            Plant.Statuses = await _context.Conservationstatuses.Where(s => SelectedStatusIds.Contains(s.Statusid)).ToListAsync();
            Plant.Cycleid = SelectedCycleId;
            Plant.Lastupdated = DateTime.UtcNow;

            _context.Plants.Add(Plant);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm thành công cây '{Plant.Plantname}'.";
            return RedirectToPage("/PlantsManager/Index");
        }

        // PAGE HANDLER OnPostAddAttributeAsync ĐÃ ĐƯỢC XÓA BỎ

        private async Task LoadOptionsAsync()
        {
            PlanttypeOptions = new SelectList(await _context.Planttypes.OrderBy(x => x.Typename).ToListAsync(), "Typeid", "Typename");
            RegionOptions = new SelectList(await _context.Regions.OrderBy(x => x.Regionname).ToListAsync(), "Regionid", "Regionname");
            CycleOptions = new SelectList(await _context.Growthcycletypes.OrderBy(x => x.Cyclename).ToListAsync(), "Cycleid", "Cyclename");
            SeasonOptions = new SelectList(await _context.Seasons.OrderBy(x => x.Seasonname).ToListAsync(), "Seasonid", "Seasonname");
            SoilOptions = new SelectList(await _context.Soiltypes.OrderBy(x => x.Soilname).ToListAsync(), "Soilid", "Soilname");
            FertilizerOptions = new SelectList(await _context.Fertilizers.OrderBy(x => x.Fertilizername).ToListAsync(), "Fertilizerid", "Fertilizername");
            DiseaseOptions = new SelectList(await _context.Plantdiseases.OrderBy(x => x.Diseasename).ToListAsync(), "Diseaseid", "Diseasename");
            PlantpropertyOptions = new SelectList(await _context.Plantproperties.OrderBy(x => x.Propertyname).ToListAsync(), "Propertyid", "Propertyname");
            WateringmethodOptions = new SelectList(await _context.Wateringmethods.OrderBy(x => x.Methodname).ToListAsync(), "Wateringid", "Methodname");
            ConservationstatusOptions = new SelectList(await _context.Conservationstatuses.OrderBy(x => x.Statusname).ToListAsync(), "Statusid", "Statusname");
        }
    }
}