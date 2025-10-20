using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models; // Đảm bảo namespace này đúng
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.PlantsManager
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;
        private const int PageSize = 8;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        // --- Các thuộc tính BindProperty cho tất cả các bộ lọc ---
        [BindProperty(SupportsGet = true)] public string? SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public string SortOrder { get; set; } = "date_desc";

        // CHÍNH XÁC HÓA: Tên thuộc tính filter để khớp với model
        [BindProperty(SupportsGet = true)] public int? FilterCycleId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterTypeId { get; set; } // Planttype -> Types
        [BindProperty(SupportsGet = true)] public int? FilterRegionId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterSeasonId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterSoilId { get; set; } // Soiltype -> Soils
        [BindProperty(SupportsGet = true)] public int? FilterFertilizerId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterDiseaseId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterPropertyId { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilterWateringId { get; set; } // Wateringmethod -> Waterings
        [BindProperty(SupportsGet = true)] public int? FilterStatusId { get; set; } // Conservationstatus -> Statuses

        // --- Các List<SelectListItem> để đổ dữ liệu ra dropdowns ---
        public List<SelectListItem> Cycles { get; set; }
        public List<SelectListItem> PlantTypes { get; set; }
        public List<SelectListItem> Regions { get; set; }
        public List<SelectListItem> Seasons { get; set; }
        public List<SelectListItem> SoilTypes { get; set; }
        public List<SelectListItem> Fertilizers { get; set; }
        public List<SelectListItem> Diseases { get; set; }
        public List<SelectListItem> Properties { get; set; }
        public List<SelectListItem> WateringMethods { get; set; }
        public List<SelectListItem> ConservationStatuses { get; set; }

        public int TotalPages { get; set; }
        public int TotalPlants { get; set; }
        public List<Plant> Plants { get; set; } = new();

        public async Task OnGetAsync()
        {
            // --- Lấy dữ liệu cho tất cả các dropdown bộ lọc ---
            Cycles = await _context.Growthcycletypes.OrderBy(c => c.Cyclename).Select(c => new SelectListItem { Value = c.Cycleid.ToString(), Text = c.Cyclename }).ToListAsync();
            Cycles.Insert(0, new SelectListItem { Value = "", Text = "Tất cả chu kỳ" });

            PlantTypes = await _context.Planttypes.OrderBy(pt => pt.Typename).Select(pt => new SelectListItem { Value = pt.Typeid.ToString(), Text = pt.Typename }).ToListAsync();
            PlantTypes.Insert(0, new SelectListItem { Value = "", Text = "Tất cả loại cây" });

            Regions = await _context.Regions.OrderBy(r => r.Regionname).Select(r => new SelectListItem { Value = r.Regionid.ToString(), Text = r.Regionname }).ToListAsync();
            Regions.Insert(0, new SelectListItem { Value = "", Text = "Tất cả vùng" });

            Seasons = await _context.Seasons.OrderBy(s => s.Seasonname).Select(s => new SelectListItem { Value = s.Seasonid.ToString(), Text = s.Seasonname }).ToListAsync();
            Seasons.Insert(0, new SelectListItem { Value = "", Text = "Tất cả mùa vụ" });

            SoilTypes = await _context.Soiltypes.OrderBy(s => s.Soilname).Select(s => new SelectListItem { Value = s.Soilid.ToString(), Text = s.Soilname }).ToListAsync();
            SoilTypes.Insert(0, new SelectListItem { Value = "", Text = "Tất cả loại đất" });

            Fertilizers = await _context.Fertilizers.OrderBy(f => f.Fertilizername).Select(f => new SelectListItem { Value = f.Fertilizerid.ToString(), Text = f.Fertilizername }).ToListAsync();
            Fertilizers.Insert(0, new SelectListItem { Value = "", Text = "Tất cả phân bón" });

            Diseases = await _context.Plantdiseases.OrderBy(d => d.Diseasename).Select(d => new SelectListItem { Value = d.Diseaseid.ToString(), Text = d.Diseasename }).ToListAsync();
            Diseases.Insert(0, new SelectListItem { Value = "", Text = "Tất cả bệnh" });

            Properties = await _context.Plantproperties.OrderBy(p => p.Propertyname).Select(p => new SelectListItem { Value = p.Propertyid.ToString(), Text = p.Propertyname }).ToListAsync();
            Properties.Insert(0, new SelectListItem { Value = "", Text = "Tất cả đặc tính" });

            WateringMethods = await _context.Wateringmethods.OrderBy(w => w.Methodname).Select(w => new SelectListItem { Value = w.Wateringid.ToString(), Text = w.Methodname }).ToListAsync();
            WateringMethods.Insert(0, new SelectListItem { Value = "", Text = "Tất cả tưới nước" });

            ConservationStatuses = await _context.Conservationstatuses.OrderBy(cs => cs.Statusname).Select(cs => new SelectListItem { Value = cs.Statusid.ToString(), Text = cs.Statusname }).ToListAsync();
            ConservationStatuses.Insert(0, new SelectListItem { Value = "", Text = "Tất cả tình trạng" });

            // --- Câu truy vấn ban đầu ---
            IQueryable<Plant> plantsQuery = _context.Plants.AsNoTracking().Include(p => p.Cycle).Include(p => p.Regions);

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchLower = SearchString.ToLower().Trim();
                plantsQuery = plantsQuery.Where(p => p.Plantname.ToLower().Contains(searchLower) || p.Scientificname.ToLower().Contains(searchLower));
            }

            // --- Áp dụng tất cả các bộ lọc ---
            if (FilterCycleId.HasValue) plantsQuery = plantsQuery.Where(p => p.Cycleid == FilterCycleId.Value);

            // CHÍNH XÁC HÓA: Sử dụng đúng tên collection và ID cho các quan hệ nhiều-nhiều
            if (FilterTypeId.HasValue) plantsQuery = plantsQuery.Where(p => p.Types.Any(t => t.Typeid == FilterTypeId.Value));
            if (FilterRegionId.HasValue) plantsQuery = plantsQuery.Where(p => p.Regions.Any(r => r.Regionid == FilterRegionId.Value));
            if (FilterSeasonId.HasValue) plantsQuery = plantsQuery.Where(p => p.Seasons.Any(s => s.Seasonid == FilterSeasonId.Value));
            if (FilterSoilId.HasValue) plantsQuery = plantsQuery.Where(p => p.Soils.Any(s => s.Soilid == FilterSoilId.Value));
            if (FilterFertilizerId.HasValue) plantsQuery = plantsQuery.Where(p => p.Fertilizers.Any(f => f.Fertilizerid == FilterFertilizerId.Value));
            if (FilterDiseaseId.HasValue) plantsQuery = plantsQuery.Where(p => p.Diseases.Any(d => d.Diseaseid == FilterDiseaseId.Value));
            if (FilterPropertyId.HasValue) plantsQuery = plantsQuery.Where(p => p.Properties.Any(prop => prop.Propertyid == FilterPropertyId.Value));
            if (FilterWateringId.HasValue) plantsQuery = plantsQuery.Where(p => p.Waterings.Any(w => w.Wateringid == FilterWateringId.Value));
            if (FilterStatusId.HasValue) plantsQuery = plantsQuery.Where(p => p.Statuses.Any(s => s.Statusid == FilterStatusId.Value));

            // Sắp xếp
            switch (SortOrder)
            {
                case "date_asc": plantsQuery = plantsQuery.OrderBy(p => p.Lastupdated); break;
                case "date_desc": plantsQuery = plantsQuery.OrderByDescending(p => p.Lastupdated); break;
                case "name_asc": plantsQuery = plantsQuery.OrderBy(p => p.Plantname); break;
                case "name_desc": plantsQuery = plantsQuery.OrderByDescending(p => p.Plantname); break;
                case "id_asc": plantsQuery = plantsQuery.OrderBy(p => p.Plantid); break;
                case "id_desc": plantsQuery = plantsQuery.OrderByDescending(p => p.Plantid); break;

                default: plantsQuery = plantsQuery.OrderByDescending(p => p.Lastupdated); break;
            }

            // Phân trang
            TotalPlants = await plantsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalPlants / (double)PageSize);
            Plants = await plantsQuery.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant != null)
            {
                _context.Plants.Remove(plant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa thành công cây '{plant.Plantname}'.";
            }
            // ...
            return RedirectToPage(new
            {
                SearchString,
                CurrentPage,
                SortOrder,
                FilterCycleId,
                FilterTypeId,
                FilterRegionId,
                FilterSeasonId,
                FilterSoilId,
                FilterFertilizerId,
                FilterDiseaseId,
                FilterPropertyId,
                FilterWateringId,
                FilterStatusId
            });
        }
    }
}