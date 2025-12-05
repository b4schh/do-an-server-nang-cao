using System.Text.Json;
using System.Text.Json.Serialization;
using FootballField.API.Database;
using FootballField.API.Modules.LocationManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.LocationManagement.Services;

public class LocationSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocationSeeder> _logger;

    public LocationSeeder(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<LocationSeeder> logger)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task SeedLocationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting location seeding process...");

            // Kiểm tra xem đã có dữ liệu chưa
            var hasProvinces = await _context.Provinces.AnyAsync();
            var hasWards = await _context.Wards.AnyAsync();
            
            if (hasProvinces && hasWards)
            {
                _logger.LogInformation("Location data already exists. Skipping seeding.");
                return;
            }

            // Lấy danh sách tỉnh/thành phố
            _logger.LogInformation("Fetching provinces from API...");
            var provincesJson = await _httpClient.GetStringAsync("https://provinces.open-api.vn/api/v2/p/");
            var provinceResponses = JsonSerializer.Deserialize<List<ProvinceApiResponse>>(provincesJson);

            if (provinceResponses == null || !provinceResponses.Any())
            {
                _logger.LogWarning("No provinces data received from API");
                return;
            }

            _logger.LogInformation($"Received {provinceResponses.Count} provinces");

            var allProvinces = new List<Province>();
            var allWards = new List<Ward>();

            foreach (var provinceResponse in provinceResponses)
            {
                _logger.LogInformation($"Processing province: {provinceResponse.Name} (Code: {provinceResponse.Code})");

                var province = new Province
                {
                    Code = provinceResponse.Code,
                    Name = provinceResponse.Name,
                    Codename = provinceResponse.Codename,
                    DivisionType = provinceResponse.Division_type
                };

                allProvinces.Add(province);

                // Lấy danh sách phường/xã của tỉnh này
                try
                {
                    var wardsJson = await _httpClient.GetStringAsync(
                        $"https://provinces.open-api.vn/api/v2/p/{provinceResponse.Code}?depth=2");
                    var provinceDetailResponse = JsonSerializer.Deserialize<ProvinceDetailApiResponse>(wardsJson);

                    if (provinceDetailResponse?.Wards != null && provinceDetailResponse.Wards.Any())
                    {
                        foreach (var wardResponse in provinceDetailResponse.Wards)
                        {
                            var ward = new Ward
                            {
                                Code = wardResponse.Code,
                                Name = wardResponse.Name,
                                Codename = wardResponse.Codename,
                                DivisionType = wardResponse.Division_type,
                                ProvinceCode = provinceResponse.Code
                            };

                            allWards.Add(ward);
                        }

                        _logger.LogInformation($"Added {provinceDetailResponse.Wards.Count} wards for {provinceResponse.Name}");
                    }

                    // Tạm dừng ngắn để tránh quá tải API
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching wards for province {provinceResponse.Name}");
                }
            }

            // Lưu tất cả vào database
            _logger.LogInformation($"Saving {allProvinces.Count} provinces and {allWards.Count} wards to database...");

            await _context.Provinces.AddRangeAsync(allProvinces);
            await _context.SaveChangesAsync();

            await _context.Wards.AddRangeAsync(allWards);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Location seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during location seeding process");
            throw;
        }
    }
}

// DTOs for API responses
public class ProvinceApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("codename")]
    public string Codename { get; set; } = null!;
    
    [JsonPropertyName("division_type")]
    public string Division_type { get; set; } = null!;
}

public class WardApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("codename")]
    public string Codename { get; set; } = null!;
    
    [JsonPropertyName("division_type")]
    public string Division_type { get; set; } = null!;
    
    [JsonPropertyName("province_code")]
    public int Province_code { get; set; }
}

public class ProvinceDetailApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("codename")]
    public string Codename { get; set; } = null!;
    
    [JsonPropertyName("division_type")]
    public string Division_type { get; set; } = null!;
    
    [JsonPropertyName("wards")]
    public List<WardApiResponse> Wards { get; set; } = new List<WardApiResponse>();
}
