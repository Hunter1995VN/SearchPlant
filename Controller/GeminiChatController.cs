using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class GeminiChatController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SearchPlant.Models.SearchPlantContext _db;
    public GeminiChatController(IConfiguration config, IHttpClientFactory httpClientFactory, SearchPlant.Models.SearchPlantContext db)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _db = db;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var question = request?.Message ?? string.Empty;
            var lowerQuestion = question.ToLower();

            // Lấy danh sách tên cây tiếng Anh
            var plantNames = _db.Plants.Select(p => p.Plantname).ToList();
            string? foundPlant = plantNames.FirstOrDefault(name => lowerQuestion.Contains(name.ToLower()));

            bool isAskingForNewPlant = foundPlant != null && string.IsNullOrWhiteSpace(request?.ContextPlant);

            // Nếu không bắt được tên cây trong câu hỏi, sử dụng bối cảnh trước đó
            if (foundPlant == null && !string.IsNullOrWhiteSpace(request?.ContextPlant))
            {
                foundPlant = plantNames.FirstOrDefault(name => name.ToLower() == request.ContextPlant.ToLower());
            }

            SearchPlant.Models.Plant? plant = null;
            if (foundPlant != null)
            {
                var foundPlantLower = foundPlant.ToLower();
                plant = await _db.Plants
                    .Include(p => p.Cycle)
                    .Include(p => p.Types)
                    .Include(p => p.Properties)
                    .Include(p => p.Diseases)
                    .Include(p => p.Regions)
                    .Include(p => p.Seasons)
                    .Include(p => p.Waterings)
                    .FirstOrDefaultAsync(p => p.Plantname.ToLower() == foundPlantLower);
            }
            else if (!string.IsNullOrWhiteSpace(request?.ContextPlant))
            {
                var contextLower = request.ContextPlant.ToLower();
                plant = await _db.Plants
                    .Include(p => p.Cycle)
                    .Include(p => p.Types)
                    .Include(p => p.Properties)
                    .Include(p => p.Diseases)
                    .Include(p => p.Regions)
                    .Include(p => p.Seasons)
                    .Include(p => p.Waterings)
                    .FirstOrDefaultAsync(p => p.Plantname.ToLower() == contextLower);
            }

            if (isAskingForNewPlant && plant != null)
            {
                var replyText = BuildPlantOverviewMessage(plant);
                var imageUrl = plant.Imagepath ?? string.Empty;
                var detailUrl = $"/Detail/{plant.Plantid}";

                return Ok(new {
                    reply = replyText,
                    imageUrl = !string.IsNullOrWhiteSpace(imageUrl) ? imageUrl : null,
                    plantName = plant.Plantname,
                    detailUrl
                });
            }

            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return BadRequest("Missing Gemini API key.");

            var userPrompt = request?.Message ?? "";
            var promptBuilder = new StringBuilder();
            promptBuilder.Append("Bạn là chuyên gia cây trồng chuyên nghiệp với kiến thức sâu rộng. ");
            promptBuilder.Append("Nếu câu hỏi không liên quan đến cây trồng, hãy trả lời duy nhất: 'Tôi chỉ hỗ trợ tư vấn về cây trồng.' ");
            promptBuilder.Append("Trả lời chi tiết, chuyên môn, dễ hiểu với cấu trúc rõ ràng. ");
            promptBuilder.Append("Dùng emoji tinh tế nếu cần nhấn mạnh, nhưng giữ văn phong chuyên nghiệp. ");

            if (plant != null)
            {
                promptBuilder.Append("\n\nThông tin nội bộ về cây:");
                promptBuilder.Append("\n" + BuildPlantDataForPrompt(plant));
            }
            else if (!string.IsNullOrWhiteSpace(request?.ContextPlant))
            {
                promptBuilder.Append($"\n\nNgười dùng đang hỏi về cây {request.ContextPlant}.");
            }

            promptBuilder.Append($"\n\nCâu hỏi: {userPrompt}");
            var prompt = promptBuilder.ToString();
            if (string.IsNullOrWhiteSpace(prompt))
                return BadRequest("Message is required.");

            var client = _httpClientFactory.CreateClient();
            var url = "https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key=" + apiKey;
            var payload = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Gemini API error: {response.StatusCode} - {result}");
                return StatusCode((int)response.StatusCode, result);
            }

            // Parse Gemini response
            using var doc = JsonDocument.Parse(result);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();
            return Ok(new {
                reply = text,
                plantName = plant?.Plantname ?? foundPlant,
                imageUrl = plant != null && !string.IsNullOrWhiteSpace(plant.Imagepath) ? plant.Imagepath : null,
                detailUrl = plant != null ? $"/Detail/{plant.Plantid}" : null
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GeminiChatController Exception: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    public class ChatRequest
    {
        public string? Message { get; set; }
        public string? ContextPlant { get; set; }
    }

    private static string BuildPlantOverviewMessage(SearchPlant.Models.Plant plant)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Tên cây: {plant.Plantname}");
        if (!string.IsNullOrWhiteSpace(plant.Description))
            builder.AppendLine($"Mô tả: {plant.Description}");
        if (!string.IsNullOrWhiteSpace(plant.Scientificname))
            builder.AppendLine($"Tên khoa học: {plant.Scientificname}");
        if (!string.IsNullOrWhiteSpace(plant.Tooltiptext))
            builder.AppendLine($"Vùng phân bố: {plant.Tooltiptext}");

        var lines = new List<string>();
        if (plant.Cycle != null)
            lines.Add($"Chu kỳ: {plant.Cycle.Cyclename}");
        if (plant.Mintemperature.HasValue && plant.Maxtemperature.HasValue)
            lines.Add($"Nhiệt độ phù hợp: {plant.Mintemperature}°C - {plant.Maxtemperature}°C");
        if (plant.Minhumidity.HasValue && plant.Maxhumidity.HasValue)
            lines.Add($"Độ ẩm thích hợp: {plant.Minhumidity}% - {plant.Maxhumidity}%");
        if (plant.Minph.HasValue && plant.Maxph.HasValue)
            lines.Add($"Độ pH đất: {plant.Minph} - {plant.Maxph}");
        if (!string.IsNullOrWhiteSpace(plant.Lighttype))
            lines.Add($"Ánh sáng: {plant.Lighttype}");

        if (lines.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Điều kiện sinh trưởng:");
            foreach (var line in lines)
                builder.AppendLine("- " + line);
        }

        var typeNames = plant.Types?
            .Where(t => !string.IsNullOrWhiteSpace(t.Typename))
            .Select(t => t.Typename)
            .ToList();
        if (typeNames is { Count: > 0 })
            builder.AppendLine("Phân loại: " + string.Join(", ", typeNames));

        var seasonNames = plant.Seasons?
            .Where(s => !string.IsNullOrWhiteSpace(s.Seasonname))
            .Select(s => s.Seasonname)
            .ToList();
        if (seasonNames is { Count: > 0 })
            builder.AppendLine("Mùa vụ phù hợp: " + string.Join(", ", seasonNames));

        var propertyNames = plant.Properties?
            .Where(p => !string.IsNullOrWhiteSpace(p.Propertyname))
            .Select(p => p.Propertyname)
            .ToList();
        if (propertyNames is { Count: > 0 })
            builder.AppendLine("Đặc điểm nổi bật: " + string.Join(", ", propertyNames));

        var diseaseNames = plant.Diseases?
            .Where(d => !string.IsNullOrWhiteSpace(d.Diseasename))
            .Select(d => d.Diseasename)
            .ToList();
        if (diseaseNames is { Count: > 0 })
            builder.AppendLine("Bệnh thường gặp: " + string.Join(", ", diseaseNames));

        builder.AppendLine();
        builder.Append("Gợi ý: Nhấn vào ảnh để xem hướng dẫn chi tiết và chăm sóc trên trang thông tin cây.");
        return builder.ToString();
    }

    private static string BuildPlantDataForPrompt(SearchPlant.Models.Plant plant)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Tên cây: {plant.Plantname}");
        if (!string.IsNullOrWhiteSpace(plant.Scientificname))
            builder.AppendLine($"Tên khoa học: {plant.Scientificname}");
        if (!string.IsNullOrWhiteSpace(plant.Description))
            builder.AppendLine($"Mô tả: {plant.Description}");
        if (!string.IsNullOrWhiteSpace(plant.Tooltiptext))
            builder.AppendLine($"Vùng: {plant.Tooltiptext}");
        if (plant.Cycle != null)
        {
            var cycleNote = string.IsNullOrWhiteSpace(plant.Cycle.Description) ? string.Empty : $" ({plant.Cycle.Description})";
            builder.AppendLine($"Chu kỳ sinh trưởng: {plant.Cycle.Cyclename}{cycleNote}");
        }
        if (plant.Mintemperature.HasValue && plant.Maxtemperature.HasValue)
            builder.AppendLine($"Nhiệt độ: {plant.Mintemperature}°C - {plant.Maxtemperature}°C");
        if (plant.Minhumidity.HasValue && plant.Maxhumidity.HasValue)
            builder.AppendLine($"Độ ẩm: {plant.Minhumidity}% - {plant.Maxhumidity}%");
        if (plant.Minph.HasValue && plant.Maxph.HasValue)
            builder.AppendLine($"pH đất: {plant.Minph} - {plant.Maxph}");
        if (!string.IsNullOrWhiteSpace(plant.Lighttype))
            builder.AppendLine($"Ánh sáng: {plant.Lighttype}");
        var propertyNames = plant.Properties?
            .Where(p => !string.IsNullOrWhiteSpace(p.Propertyname))
            .Select(p => p.Propertyname)
            .ToList();
        if (propertyNames is { Count: > 0 })
            builder.AppendLine("Thuộc tính: " + string.Join(", ", propertyNames));

        var typeNames = plant.Types?
            .Where(t => !string.IsNullOrWhiteSpace(t.Typename))
            .Select(t => t.Typename)
            .ToList();
        if (typeNames is { Count: > 0 })
            builder.AppendLine("Phân loại: " + string.Join(", ", typeNames));

        var seasonNames = plant.Seasons?
            .Where(s => !string.IsNullOrWhiteSpace(s.Seasonname))
            .Select(s => s.Seasonname)
            .ToList();
        if (seasonNames is { Count: > 0 })
            builder.AppendLine("Mùa trồng: " + string.Join(", ", seasonNames));

        var diseaseNames = plant.Diseases?
            .Where(d => !string.IsNullOrWhiteSpace(d.Diseasename))
            .Select(d => d.Diseasename)
            .ToList();
        if (diseaseNames is { Count: > 0 })
            builder.AppendLine("Bệnh dễ gặp: " + string.Join(", ", diseaseNames));

        var wateringNames = plant.Waterings?
            .Where(w => !string.IsNullOrWhiteSpace(w.Methodname))
            .Select(w => w.Methodname)
            .ToList();
        if (wateringNames is { Count: > 0 })
            builder.AppendLine("Phương pháp tưới: " + string.Join(", ", wateringNames));

        var regionNames = plant.Regions?
            .Where(r => !string.IsNullOrWhiteSpace(r.Regionname))
            .Select(r => r.Regionname)
            .ToList();
        if (regionNames is { Count: > 0 })
            builder.AppendLine("Khu vực trồng: " + string.Join(", ", regionNames));
        return builder.ToString();
    }
}
