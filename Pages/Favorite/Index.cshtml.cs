using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace SearchPlant.Pages.Favorite;

// Bắt buộc người dùng phải đăng nhập để truy cập trang này
[Authorize]
public class IndexModel : PageModel
{
    private readonly SearchPlantContext _context;

    public IndexModel(SearchPlantContext context)
    {
        _context = context;
    }

    // Thuộc tính này sẽ chứa danh sách cây để hiển thị ra ngoài View
    public List<Plant> FavoritePlants { get; set; }

    public async Task OnGetAsync()
    {
        // Lấy ID của người dùng đang đăng nhập từ claims
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            // Trường hợp không tìm thấy ID (dù đã Authorize), có thể xử lý lỗi ở đây
            FavoritePlants = new List<Plant>();
            return;
        }

        var userId = int.Parse(userIdString);

        // Truy vấn CSDL để lấy danh sách cây yêu thích của người dùng này
        FavoritePlants = await _context.UserFavorites
            .Where(uf => uf.Userid == userId)
            .Include(uf => uf.Plant) // Lấy thông tin chi tiết của cây
                .ThenInclude(p => p.Regions) // Lấy thêm thông tin vùng trồng
            .Include(uf => uf.Plant)
                .ThenInclude(p => p.Cycle) // Lấy thêm thông tin chu kỳ
            .Select(uf => uf.Plant) // Chỉ chọn đối tượng Plant để trả về
            .ToListAsync();
    }
}