using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;

namespace SearchPlant.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        public List<Plant> Plants { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<int, (double Average, int Count)> PlantRatings { get; set; } = new();

        public List<Plant> TopRatedPlants { get; set; } = new();
        public async Task OnGetAsync([FromQuery] int page = 1)
        {

            int pageSize = 6;
            int totalCount = await _context.Plants.CountAsync();

            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            CurrentPage = page;


            Plants = await _context.Plants
         .Include(p => p.Cycle)
         .Include(p => p.Regions)
         .OrderBy(p => p.Plantname)
         .Skip((page - 1) * pageSize)
         .Take(pageSize)
         .ToListAsync();

            var PlantsIdOnPages = Plants.Select(a => a.Plantid).ToList();
            if (PlantsIdOnPages.Any())
            {
                PlantRatings = await _context.Ratings
                    .Where(r => PlantsIdOnPages.Contains(r.Plantid))
                    .GroupBy(r => r.Plantid)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => (g.Average(r => r.RatingValue), g.Count())
                    );
            }



            var topRatedPlantIds = await _context.Ratings
                          .GroupBy(r => r.Plantid)
                          .Select(g => new
                          {
                              Plantid = g.Key,
                              AverageRating = g.Average(r => r.RatingValue),
                              RatingCount = g.Count()
                          })
                          .Where(r => r.RatingCount >= 2) // Chỉ lấy cây có ít nhất 2 lượt đánh giá
                          .OrderByDescending(r => r.AverageRating) // Sắp xếp theo điểm trung bình giảm dần
                          .ThenByDescending(r => r.RatingCount) // Nếu điểm bằng nhau, ưu tiên cây có nhiều lượt đánh giá hơn
                          .Take(8) // Lấy 8 cây hàng đầu
                          .Select(r => r.Plantid)
                          .ToListAsync();

            if (topRatedPlantIds.Any())
            {
                TopRatedPlants = await _context.Plants
                    .Where(p => topRatedPlantIds.Contains(p.Plantid))
                    .Include(p => p.Regions)
                    .ToListAsync();

                // Gắn lại thứ tự đúng sau khi lấy từ CSDL
                TopRatedPlants = TopRatedPlants.OrderBy(p => topRatedPlantIds.IndexOf(p.Plantid)).ToList();
            }


        }



    }
}
