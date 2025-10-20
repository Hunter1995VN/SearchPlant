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
        public int Pagenum { get; set; } = 1; // Trang hiện tại, mặc định là 1

        public IList<Plant> Results { get; set; } = new List<Plant>();
        public int PageIndex => Pagenum;
        public int TotalPages { get; set; }

        private const int PageSize = 9; // Số cây trên mỗi trang

        public async Task OnGetAsync()
        {
            IQueryable<Plant> queryable = _context.Plants
                .Include(p => p.Cycle);

            if (!string.IsNullOrWhiteSpace(Query))
            {
                queryable = queryable.Where(p =>
                    EF.Functions.ILike(p.Plantname, $"%{Query}%") ||
                    EF.Functions.ILike(p.Scientificname, $"%{Query}%"));
            }

            int totalCount = await queryable.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            // Đảm bảo Page >= 1 và <= TotalPages
            if (Pagenum < 1) Pagenum = 1;
            if (Pagenum > TotalPages) Pagenum = TotalPages;

            Results = await queryable
                .OrderBy(p => p.Plantname)
                .Skip((Pagenum - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}