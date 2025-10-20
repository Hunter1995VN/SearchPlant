using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models; // Giả sử DbContext của bạn ở đây
using System.Security.Claims;

[Authorize] // Bắt buộc người dùng phải đăng nhập
[Route("api/[controller]")]
[ApiController]
public class FavoritesController : ControllerBase
{
    private readonly SearchPlantContext _context; 
        public FavoritesController(SearchPlantContext context)
    {
        _context = context;
    }

    // Lấy user ID của người dùng đang đăng nhập
    private int GetCurrentUserId()
    {
        // Lấy user ID từ claims. Tùy vào cách bạn cấu hình JWT hoặc Identity
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        throw new Exception("User ID not found in token.");
    }

    // GET: api/Favorites
    // Lấy danh sách cây yêu thích của người dùng hiện tại
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plant>>> GetFavoritePlants()
    {
        var userId = GetCurrentUserId();

        var favoritePlants = await _context.UserFavorites
            .Where(uf => uf.Userid == userId)
            .Include(uf => uf.Plant) // Lấy thông tin chi tiết của cây
            .Select(uf => uf.Plant) // Chỉ chọn đối tượng Plant để trả về
            .ToListAsync();

        return Ok(favoritePlants);
    }

    // POST: api/Favorites
    // Thêm một cây vào danh sách yêu thích
    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequestDto request)
    {
        var userId = GetCurrentUserId();
        var plantId = request.PlantId;

        // Kiểm tra xem cây có tồn tại không
        var plantExists = await _context.Plants.AnyAsync(p => p.Plantid == plantId);
        if (!plantExists)
        {
            return NotFound(new { message = "Plant not found." });
        }

        // Kiểm tra xem đã yêu thích cây này chưa
        var alreadyFavorite = await _context.UserFavorites
            .AnyAsync(uf => uf.Userid == userId && uf.Plantid == plantId);

        if (alreadyFavorite)
        {
            // Nếu đã thích rồi thì không làm gì cả, trả về thành công
            return Ok(new { message = "Plant is already in favorites." });
        }

        var userFavorite = new UserFavorite
        {
            Userid = userId,
            Plantid = plantId,
            CreatedAt = DateTime.UtcNow // Sử dụng UTC để tránh lỗi Npgsql
        };

        _context.UserFavorites.Add(userFavorite);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFavoritePlants), new { id = userFavorite.Plantid }, userFavorite);
    }

    // DELETE: api/Favorites/{plantId}
    // Xóa một cây khỏi danh sách yêu thích
    [HttpDelete("{plantId}")]
    public async Task<IActionResult> RemoveFavorite(int plantId)
    {
        var userId = GetCurrentUserId();

        var favorite = await _context.UserFavorites
            .FirstOrDefaultAsync(uf => uf.Userid == userId && uf.Plantid == plantId);

        if (favorite == null)
        {
            return NotFound(new { message = "Favorite entry not found." });
        }

        _context.UserFavorites.Remove(favorite);
        await _context.SaveChangesAsync();

        return NoContent(); // Trả về 204 No Content khi xóa thành công
    }
}

// DTO (Data Transfer Object) để nhận request body
public class FavoriteRequestDto
{
    public int PlantId { get; set; }
}