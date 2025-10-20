using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchPlant.Models;
using System.Threading.Tasks;

namespace SearchPlant.Controllers
{
    // DTO để nhận dữ liệu từ client
    public class CreateAttributeDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Composition { get; set; }
        public string? UsageNote { get; set; }
        public string? Symptoms { get; set; }
        public string? Cause { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Đảm bảo chỉ Admin mới có thể gọi API này
    public class AttributesApiController : ControllerBase
    {
        private readonly SearchPlantContext _context;

        public AttributesApiController(SearchPlantContext context)
        {
            _context = context;
        }

        [HttpPost] // Xử lý yêu cầu POST đến /api/AttributesApi
        public async Task<IActionResult> CreateAttribute([FromBody] CreateAttributeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name) || string.IsNullOrWhiteSpace(dto.Type))
            {
                return BadRequest("Tên và loại thuộc tính không được để trống.");
            }

            object newItem = null;
            var trimmedName = dto.Name.Trim();
            var upperTrimmedName = trimmedName.ToUpper();

            switch (dto.Type.ToLower())
            {
                case "planttype":
                    if (await _context.Planttypes.AnyAsync(p => p.Typename.ToUpper() == upperTrimmedName))
                        return Conflict($"Loại cây '{trimmedName}' đã tồn tại.");
                    var newType = new Planttype { Typename = trimmedName, Typedescription = dto.Description };
                    _context.Planttypes.Add(newType); await _context.SaveChangesAsync();
                    newItem = new { id = newType.Typeid, text = newType.Typename }; break;

                case "region":
                    if (await _context.Regions.AnyAsync(p => p.Regionname.ToUpper() == upperTrimmedName))
                        return Conflict($"Vùng trồng '{trimmedName}' đã tồn tại.");
                    var newRegion = new Region { Regionname = trimmedName, Regiondescription = dto.Description };
                    _context.Regions.Add(newRegion); await _context.SaveChangesAsync();
                    newItem = new { id = newRegion.Regionid, text = newRegion.Regionname }; break;

                case "fertilizer":
                    if (await _context.Fertilizers.AnyAsync(p => p.Fertilizername.ToUpper() == upperTrimmedName))
                        return Conflict($"Phân bón '{trimmedName}' đã tồn tại.");
                    var newFertilizer = new Fertilizer { Fertilizername = trimmedName, Composition = dto.Composition, Usagenote = dto.UsageNote };
                    _context.Fertilizers.Add(newFertilizer); await _context.SaveChangesAsync();
                    newItem = new { id = newFertilizer.Fertilizerid, text = newFertilizer.Fertilizername }; break;

                case "plantdisease":
                    if (await _context.Plantdiseases.AnyAsync(p => p.Diseasename.ToUpper() == upperTrimmedName))
                        return Conflict($"Bệnh '{trimmedName}' đã tồn tại.");
                    var newDisease = new Plantdisease { Diseasename = trimmedName, Symptoms = dto.Symptoms, Cause = dto.Cause };
                    _context.Plantdiseases.Add(newDisease); await _context.SaveChangesAsync();
                    newItem = new { id = newDisease.Diseaseid, text = newDisease.Diseasename }; break;

                // === BỔ SUNG CÁC TRƯỜNG HỢP CÒN LẠI ===
                case "season":
                    if (await _context.Seasons.AnyAsync(p => p.Seasonname.ToUpper() == upperTrimmedName))
                        return Conflict($"Mùa vụ '{trimmedName}' đã tồn tại.");
                    var newSeason = new Season { Seasonname = trimmedName, Description = dto.Description };
                    _context.Seasons.Add(newSeason); await _context.SaveChangesAsync();
                    newItem = new { id = newSeason.Seasonid, text = newSeason.Seasonname }; break;

                case "soiltype":
                    if (await _context.Soiltypes.AnyAsync(p => p.Soilname.ToUpper() == upperTrimmedName))
                        return Conflict($"Loại đất '{trimmedName}' đã tồn tại.");
                    var newSoil = new Soiltype { Soilname = trimmedName, Description = dto.Description };
                    _context.Soiltypes.Add(newSoil); await _context.SaveChangesAsync();
                    newItem = new { id = newSoil.Soilid, text = newSoil.Soilname }; break;

                case "plantproperty":
                    if (await _context.Plantproperties.AnyAsync(p => p.Propertyname.ToUpper() == upperTrimmedName))
                        return Conflict($"Đặc tính '{trimmedName}' đã tồn tại.");
                    var newProperty = new Plantproperty { Propertyname = trimmedName, Description = dto.Description };
                    _context.Plantproperties.Add(newProperty); await _context.SaveChangesAsync();
                    newItem = new { id = newProperty.Propertyid, text = newProperty.Propertyname }; break;

                case "wateringmethod":
                    if (await _context.Wateringmethods.AnyAsync(p => p.Methodname.ToUpper() == upperTrimmedName))
                        return Conflict($"Phương pháp tưới '{trimmedName}' đã tồn tại.");
                    var newWatering = new Wateringmethod { Methodname = trimmedName, Description = dto.Description };
                    _context.Wateringmethods.Add(newWatering); await _context.SaveChangesAsync();
                    newItem = new { id = newWatering.Wateringid, text = newWatering.Methodname }; break;

                case "conservationstatus":
                    if (await _context.Conservationstatuses.AnyAsync(p => p.Statusname.ToUpper() == upperTrimmedName))
                        return Conflict($"Tình trạng bảo tồn '{trimmedName}' đã tồn tại.");
                    var newStatus = new Conservationstatus { Statusname = trimmedName, Description = dto.Description };
                    _context.Conservationstatuses.Add(newStatus); await _context.SaveChangesAsync();
                    newItem = new { id = newStatus.Statusid, text = newStatus.Statusname }; break;

                default:
                    return BadRequest("Loại thuộc tính không hợp lệ.");
            }

            return Ok(newItem); // Trả về 200 OK với dữ liệu
        }
    }
}