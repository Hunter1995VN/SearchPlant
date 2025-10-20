using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
#if !DEBUG // Chỉ áp dụng [Authorize] khi KHÔNG ở chế độ Debug
[Authorize(Roles = "Admin")]
#endif
public class DataController : ControllerBase
{
    private readonly SearchPlantContext _context;

    public DataController(SearchPlantContext context)
    {
        _context = context;
    }

    [HttpGet("plants")]
    public async Task<IActionResult> GetPlantsForReport()
    {
        var reportData = await _context.Plants
            // Include tất cả các bảng liên quan mà bạn muốn hiển thị trong báo cáo
            .Include(p => p.Cycle)
            .Include(p => p.Types)
            .Include(p => p.Regions)
            .Include(p => p.Seasons)
            .Include(p => p.Soils)
            .Include(p => p.Fertilizers)
            .Include(p => p.Diseases)
            .Include(p => p.Properties)
            .Include(p => p.Waterings)
            .Include(p => p.Statuses)
            .AsNoTracking()
            .Select(p => new // Tạo một đối tượng DTO (Data Transfer Object) phẳng
            {
                // Thông tin cơ bản
                PlantId = p.Plantid,
                PlantName = p.Plantname,
                ScientificName = p.Scientificname,
                Description = p.Description,
                ImageUrl = p.Imagepath, // Giả sử bạn có trường này

                // Thông tin môi trường
                MinPh = p.Minph,
                MaxPh = p.Maxph,
                MinHumidity = p.Minhumidity,
                MaxHumidity = p.Maxhumidity,
                MinTemperature = p.Mintemperature,
                MaxTemperature = p.Maxtemperature,
                LightType = p.Lighttype,

                // Thông tin từ các bảng liên quan (đã được join)
                CycleName = p.Cycle != null ? p.Cycle.Cyclename : "N/A",

                // Chuyển các danh sách (many-to-many) thành chuỗi, phân cách bằng dấu phẩy
                Types = string.Join(", ", p.Types.Select(t => t.Typename)),
                Regions = string.Join(", ", p.Regions.Select(r => r.Regionname)),
                Seasons = string.Join(", ", p.Seasons.Select(s => s.Seasonname)),
                Soils = string.Join(", ", p.Soils.Select(s => s.Soilname)),
                Fertilizers = string.Join(", ", p.Fertilizers.Select(f => f.Fertilizername)),
                Diseases = string.Join(", ", p.Diseases.Select(d => d.Diseasename)),
                Properties = string.Join(", ", p.Properties.Select(prop => prop.Propertyname)),
                WateringMethods = string.Join(", ", p.Waterings.Select(w => w.Methodname)),
                ConservationStatus = string.Join(", ", p.Statuses.Select(s => s.Statusname)),
                
                LastUpdated = p.Lastupdated
            })
            .OrderBy(p => p.PlantName) // Sắp xếp theo tên cho dễ nhìn
            .ToListAsync();

        return Ok(reportData);
    }
}