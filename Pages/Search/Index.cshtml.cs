using Microsoft.AspNetCore.Mvc.RazorPages;
using SearchPlant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace SearchPlant.Pages.Search
{
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Pagenum { get; set; } = 1;

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public int? FilterTypeId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterRegionId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterCycleId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterSeasonId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterSoilId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterFertilizerId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterDiseaseId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterPropertyId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterWateringId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? FilterStatusId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "name_asc";

        public IList<Plant> Results { get; set; } = new List<Plant>();
        public int PageIndex => Pagenum;
        public int TotalPages { get; set; }

        // SelectList items for filters
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList PlantTypes { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Regions { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Cycles { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Seasons { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList SoilTypes { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Fertilizers { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Diseases { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList Properties { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList WateringMethods { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList ConservationStatuses { get; set; }

        private const int PageSize = 9;

        public async Task OnGetAsync()
        {
            // Load filter data
            PlantTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Planttypes.ToListAsync(), "Typeid", "Planttypename");
            Regions = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Regions.ToListAsync(), "Regionid", "Regionname");
            Cycles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Growthcycletypes.ToListAsync(), "Growthcycletypeid", "Cyclename");
            Seasons = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Seasons.ToListAsync(), "Seasonid", "Seasonname");
            SoilTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Soiltypes.ToListAsync(), "Soilid", "Soiltypename");
            Fertilizers = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Fertilizers.ToListAsync(), "Fertilizerid", "Fertilizername");
            Diseases = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Plantdiseases.ToListAsync(), "Diseaseid", "Diseasename");
            Properties = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Plantproperties.ToListAsync(), "Propertyid", "Propertyname");
            WateringMethods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Wateringmethods.ToListAsync(), "Wateringid", "Methodname");
            ConservationStatuses = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Conservationstatuses.ToListAsync(), "Statusid", "Statusname");

            IQueryable<Plant> queryable = _context.Plants
                .Include(p => p.Cycle)
                .Include(p => p.Regions)
                .Include(p => p.Types)
                .Include(p => p.Seasons)
                .Include(p => p.Soils)
                .Include(p => p.Fertilizers)
                .Include(p => p.Diseases)
                .Include(p => p.Properties)
                .Include(p => p.Waterings)
                .Include(p => p.Statuses);

            // Apply search query
            if (!string.IsNullOrWhiteSpace(Query))
            {
                queryable = queryable.Where(p =>
                    EF.Functions.ILike(p.Plantname, $"%{Query}%") ||
                    EF.Functions.ILike(p.Scientificname, $"%{Query}%"));
            }

            // Apply filters
            if (FilterTypeId.HasValue)
                queryable = queryable.Where(p => p.Types.Any(t => t.Typeid == FilterTypeId.Value));
            if (FilterRegionId.HasValue)
                queryable = queryable.Where(p => p.Regions.Any(r => r.Regionid == FilterRegionId.Value));
            if (FilterCycleId.HasValue)
                queryable = queryable.Where(p => p.Cycleid == FilterCycleId.Value);
            if (FilterSeasonId.HasValue)
                queryable = queryable.Where(p => p.Seasons.Any(s => s.Seasonid == FilterSeasonId.Value));
            if (FilterSoilId.HasValue)
                queryable = queryable.Where(p => p.Soils.Any(s => s.Soilid == FilterSoilId.Value));
            if (FilterFertilizerId.HasValue)
                queryable = queryable.Where(p => p.Fertilizers.Any(f => f.Fertilizerid == FilterFertilizerId.Value));
            if (FilterDiseaseId.HasValue)
                queryable = queryable.Where(p => p.Diseases.Any(d => d.Diseaseid == FilterDiseaseId.Value));
            if (FilterPropertyId.HasValue)
                queryable = queryable.Where(p => p.Properties.Any(prop => prop.Propertyid == FilterPropertyId.Value));
            if (FilterWateringId.HasValue)
                queryable = queryable.Where(p => p.Waterings.Any(w => w.Wateringid == FilterWateringId.Value));
            if (FilterStatusId.HasValue)
                queryable = queryable.Where(p => p.Statuses.Any(s => s.Statusid == FilterStatusId.Value));

            // Apply sorting
            switch (SortOrder)
            {
                case "date_desc":
                    queryable = queryable.OrderByDescending(p => p.Plantid);
                    break;
                case "date_asc":
                    queryable = queryable.OrderBy(p => p.Plantid);
                    break;
                case "name_desc":
                    queryable = queryable.OrderByDescending(p => p.Plantname);
                    break;
                case "name_asc":
                default:
                    queryable = queryable.OrderBy(p => p.Plantname);
                    break;
                case "id_asc":
                    queryable = queryable.OrderBy(p => p.Plantid);
                    break;
                case "id_desc":
                    queryable = queryable.OrderByDescending(p => p.Plantid);
                    break;
            }

            int totalCount = await queryable.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            if (Pagenum < 1) Pagenum = 1;
            if (Pagenum > TotalPages) Pagenum = TotalPages;

            Results = await queryable
                .Skip((Pagenum - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}