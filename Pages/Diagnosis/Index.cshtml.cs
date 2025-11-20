using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SearchPlant.Pages.Diagnosis
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _factory;

        public IndexModel(IConfiguration config, IHttpClientFactory factory)
        {
            _config = config;
            _factory = factory;
        }

        [BindProperty]
        public IFormFile? PlantImage { get; set; }

        public PlantIdResponse? Response { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UploadedImageBase64 { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (PlantImage == null || PlantImage.Length == 0)
            {
                ErrorMessage = "Vui lòng chọn ảnh.";
                return Page();
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(PlantImage.ContentType.ToLower()))
            {
                ErrorMessage = "Chỉ chấp nhận file ảnh định dạng JPG, JPEG hoặc PNG.";
                return Page();
            }

            // Validate file size (max 10MB)
            if (PlantImage.Length > 10 * 1024 * 1024)
            {
                ErrorMessage = "Kích thước ảnh không được vượt quá 10MB.";
                return Page();
            }

            try
            {
                var apiKey = _config["PlantId:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    ErrorMessage = "API Key chưa được cấu hình.";
                    return Page();
                }

                // Convert image to base64 for display
                using (var ms = new MemoryStream())
                {
                    await PlantImage.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    UploadedImageBase64 = $"data:{PlantImage.ContentType};base64,{Convert.ToBase64String(imageBytes)}";
                }

                var client = _factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(60);

                // Convert image to base64
                using var imageStream = PlantImage.OpenReadStream();
                using var memStream = new MemoryStream();
                await imageStream.CopyToAsync(memStream);
                var imageBase64 = Convert.ToBase64String(memStream.ToArray());

                // Prepare request body theo Plant.id API v3
                var requestBody = new
                {
                    images = new[] { imageBase64 },
                    similar_images = true,
                    health = "all",
                    classification_level = "species"
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Add API key to header
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Api-Key", apiKey);

                // Gọi health_assessment endpoint
                var response = await client.PostAsync("https://plant.id/api/v3/health_assessment", httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Lỗi API (Status {(int)response.StatusCode}): {response.ReasonPhrase}";

                    // Try to parse error details
                    try
                    {
                        var errorDoc = JsonDocument.Parse(responseBody);
                        if (errorDoc.RootElement.TryGetProperty("error", out var error))
                        {
                            ErrorMessage += $"<br>Chi tiết: {error.GetString()}";
                        }
                        else if (errorDoc.RootElement.TryGetProperty("message", out var message))
                        {
                            ErrorMessage += $"<br>Chi tiết: {message.GetString()}";
                        }
                    }
                    catch { }

                    ErrorMessage += $"<br><pre>{responseBody}</pre>";
                    return Page();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                Response = JsonSerializer.Deserialize<PlantIdResponse>(responseBody, options);

                if (Response == null)
                {
                    ErrorMessage = "Không thể phân tích kết quả từ API.";
                }
            }
            catch (TaskCanceledException)
            {
                ErrorMessage = "Yêu cầu đã hết thời gian chờ. Vui lòng thử lại.";
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Lỗi kết nối API: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi không xác định: {ex.Message}<br>{ex.StackTrace}";
            }

            return Page();
        }
    }

    // ================================
    // Response Models for Plant.id API v3
    // ================================
    public class PlantIdResponse
    {
        [JsonPropertyName("result")]
        public PlantIdResult? Result { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("is_plant")]
        public IsPlantInfo? IsPlant { get; set; }

        [JsonPropertyName("is_healthy")]
        public IsHealthyInfo? IsHealthy { get; set; }
    }

    public class PlantIdResult
    {
        [JsonPropertyName("classification")]
        public Classification? Classification { get; set; }

        [JsonPropertyName("disease")]
        public DiseaseInfo? Disease { get; set; }
    }

    public class IsPlantInfo
    {
        [JsonPropertyName("binary")]
        public bool Binary { get; set; }

        [JsonPropertyName("probability")]
        public double Probability { get; set; }
    }

    public class IsHealthyInfo
    {
        [JsonPropertyName("binary")]
        public bool Binary { get; set; }

        [JsonPropertyName("probability")]
        public double Probability { get; set; }
    }

    public class Classification
    {
        [JsonPropertyName("suggestions")]
        public List<PlantSuggestion> Suggestions { get; set; } = new();
    }

    public class PlantSuggestion
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("probability")]
        public double Probability { get; set; }

        [JsonPropertyName("similar_images")]
        public List<SimilarImage>? SimilarImages { get; set; }

        [JsonPropertyName("details")]
        public PlantDetails? Details { get; set; }
    }

    public class SimilarImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("similarity")]
        public double Similarity { get; set; }

        [JsonPropertyName("url_small")]
        public string? UrlSmall { get; set; }
    }

    public class PlantDetails
    {
        [JsonPropertyName("common_names")]
        public List<string>? CommonNames { get; set; }

        [JsonPropertyName("taxonomy")]
        public Dictionary<string, string>? Taxonomy { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("description")]
        public Description? Description { get; set; }

        [JsonPropertyName("image")]
        public ImageInfo? Image { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("entity_id")]
        public string? EntityId { get; set; }
    }

    public class Description
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("citation")]
        public string? Citation { get; set; }

        [JsonPropertyName("license_name")]
        public string? LicenseName { get; set; }

        [JsonPropertyName("license_url")]
        public string? LicenseUrl { get; set; }
    }

    public class ImageInfo
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("citation")]
        public string? Citation { get; set; }

        [JsonPropertyName("license_name")]
        public string? LicenseName { get; set; }

        [JsonPropertyName("license_url")]
        public string? LicenseUrl { get; set; }
    }

    public class DiseaseInfo
    {
        [JsonPropertyName("suggestions")]
        public List<DiseaseSuggestion> Suggestions { get; set; } = new();
    }

    public class DiseaseSuggestion
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("probability")]
        public double Probability { get; set; }

        [JsonPropertyName("similar_images")]
        public List<SimilarImage>? SimilarImages { get; set; }

        [JsonPropertyName("details")]
        public DiseaseDetails? Details { get; set; }
    }

    public class DiseaseDetails
    {
        [JsonPropertyName("common_names")]
        public List<string>? CommonNames { get; set; }

        [JsonPropertyName("classification")]
        public List<string>? Classification { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("cause")]
        public string? Cause { get; set; }

        [JsonPropertyName("treatment")]
        public Treatment? Treatment { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("entity_id")]
        public string? EntityId { get; set; }
    }

    public class Treatment
    {
        [JsonPropertyName("chemical")]
        public List<string>? Chemical { get; set; }

        [JsonPropertyName("biological")]
        public List<string>? Biological { get; set; }

        [JsonPropertyName("prevention")]
        public List<string>? Prevention { get; set; }
    }
}