using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchPlant.Pages.Admin
{
        [Authorize(Roles = "Admin")]

    public class IndexModel : PageModel
    {
        private readonly SearchPlantContext _context;

        public IndexModel(SearchPlantContext context)
        {
            _context = context;
        }

        // --- Các thuộc tính cho card tổng quan ---
        public int TotalPlants { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAdmin { get; set; }
        public int TotalUser { get; set; }
        public int TotalType { get; set; }

        // --- Các thuộc tính để chứa dữ liệu cho biểu đồ ---
        public List<string> PlantTypeLabels { get; set; } = new List<string>();
        public List<int> PlantTypeValues { get; set; } = new List<int>();

        // ✅ SỬA LỖI: Sửa lại phương thức để hỗ trợ async/await
        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền truy cập
            if (User?.Identity?.IsAuthenticated != true || !User.IsInRole("Admin"))
            {
                return RedirectToPage("/Login");
            }

            // ✅ Tối ưu: Dùng phiên bản Async cho tất cả các truy vấn
            TotalPlants = await _context.Plants.CountAsync();
            TotalUsers = await _context.Appusers.CountAsync();
            TotalAdmin = await _context.Appusers.CountAsync(u => u.Role == "Admin");
            TotalUser = await _context.Appusers.CountAsync(i => i.Role == "User");
            TotalType = await _context.Planttypes.CountAsync();

            // Lấy dữ liệu cho biểu đồ
            var plantTypeData = await _context.Planttypes
                .Include(pt => pt.Plants)
                .Select(pt => new
                {
                    Label = pt.Typename,
                    Value = pt.Plants.Count()
                })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            // ✅ SỬA LỖI LOGIC: Gán dữ liệu đã lấy được vào các thuộc tính của Model
            foreach (var item in plantTypeData)
            {
                PlantTypeLabels.Add(item.Label);
                PlantTypeValues.Add(item.Value);
            }

            return Page();
        }
    }
}