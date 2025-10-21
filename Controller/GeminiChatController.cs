using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public GeminiChatController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return BadRequest("Missing Gemini API key.");

            var userPrompt = request?.Message ?? "";
            var prompt = $"Bạn là chuyên gia cây trồng. Nếu câu hỏi không liên quan đến cây trồng, hãy trả lời duy nhất: 'Tôi chỉ hỗ trợ tư vấn về cây trồng.' Không hỏi lại người dùng, không trả lời ngoài nghiệp vụ.\nCâu hỏi: {userPrompt}";
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
            return Ok(new { reply = text });
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
    }
}
