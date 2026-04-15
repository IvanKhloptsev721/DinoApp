using System.Text;
using System.Text.Json;
using DinoApp.Models;
using Microsoft.AspNetCore.Http;

namespace DinoApp.Services;

public class DinoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly string _baseUrl;
    private readonly ILogger<DinoApiClient> _logger;

    public DinoApiClient(HttpClient httpClient, IConfiguration config, IWebHostEnvironment environment, ILogger<DinoApiClient> logger)
    {
        _httpClient = httpClient;
        _environment = environment;
        _logger = logger;
        _baseUrl = config["ApiSettings:BaseUrl"] ?? "http://85.198.68.116:5000";

        // Устанавливаем BaseAddress
        _httpClient.BaseAddress = new Uri(_baseUrl);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _logger.LogInformation($"DinoApiClient initialized with base URL: {_httpClient.BaseAddress}");
    }

    // GET /api/dinosaurs
    public async Task<List<DinosaurDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all dinosaurs from API");
            var response = await _httpClient.GetAsync("/api/dinosaurs");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error {response.StatusCode}: {error}");
                return new List<DinosaurDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"API Response: {json}"); // Добавьте это для отладки

            var dinosaurs = JsonSerializer.Deserialize<List<DinosaurDto>>(json, _jsonOptions) ?? new();

            foreach (var dino in dinosaurs)
            {
                _logger.LogInformation($"Raw dino: {dino.Name}, PhotoUrl: {dino.PhotoUrl}, PhotoPath: {dino.PhotoPath}");
                FixImageUrl(dino);
                _logger.LogInformation($"Fixed dino: {dino.Name}, PhotoUrl: {dino.PhotoUrl}");
            }

            return dinosaurs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dinosaurs from API");
            return new List<DinosaurDto>();
        }
    }

    // GET /api/dinosaurs/{id}
    public async Task<DinosaurDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/dinosaurs/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error {response.StatusCode}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var dinosaur = JsonSerializer.Deserialize<DinosaurDto>(json, _jsonOptions);

            if (dinosaur != null)
            {
                FixImageUrl(dinosaur);
            }

            return dinosaur;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching dinosaur with ID {id}");
            return null;
        }
    }

    // POST /api/dinosaurs - СОЗДАТЬ (упрощенная версия без файлов)
    // Проверьте, какие поля ожидает API
    public async Task<DinosaurDto> CreateAsync(CreateDinosaurDto dto)
    {
        try
        {
            // Убедитесь, что имена полей совпадают с API
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Sending POST to /api/dinosaurs with: {json}");
            var response = await _httpClient.PostAsync("/api/dinosaurs", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error {response.StatusCode}: {error}");
                throw new Exception($"API вернул ошибку: {response.StatusCode} - {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DinosaurDto>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dinosaur");
            throw;
        }
    }
    // ✅ DELETE /api/dinosaurs/{id} - УДАЛИТЬ (этот метод отсутствовал)
    public async Task DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/dinosaurs/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error {response.StatusCode}: {error}");
                throw new Exception($"API вернул ошибку: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting dinosaur ID: {id}");
            throw;
        }
    }

    // Вспомогательный метод для фиксации URL изображений
    // Вспомогательный метод для фиксации URL изображений
    private void FixImageUrl(DinosaurDto dinosaur)
    {
        if (dinosaur == null) return;

        // Базовый URL API (жестко задаем, так как API на внешнем сервере)
        var baseUrl = "http://85.198.68.116:5000";

        // Пробуем получить URL изображения из разных полей
        var imagePath = !string.IsNullOrEmpty(dinosaur.PhotoUrl)
            ? dinosaur.PhotoUrl
            : dinosaur.PhotoPath;

        if (!string.IsNullOrEmpty(imagePath))
        {
            // Если это уже полный URL - оставляем
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
            {
                dinosaur.PhotoUrl = imagePath;
                dinosaur.PhotoPath = imagePath;
            }
            else
            {
                // Убираем ведущий слеш если есть
                var cleanPath = imagePath.TrimStart('/');
                // Формируем полный URL
                var fullUrl = $"{baseUrl}/{cleanPath}";

                dinosaur.PhotoUrl = fullUrl;
                dinosaur.PhotoPath = fullUrl;

                _logger.LogInformation($"Fixed image URL for {dinosaur.Name}: {fullUrl}");
            }
        }
        else
        {
            _logger.LogWarning($"No image for dinosaur: {dinosaur.Name}");
            // Устанавливаем заглушку
            dinosaur.PhotoUrl = "/images/default-dino.jpg";
            dinosaur.PhotoPath = "/images/default-dino.jpg";
        }
    }
    // Вспомогательный метод для добавления строковых полей
    private void AddStringContent(MultipartFormDataContent content, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            content.Add(new StringContent(value), name);
        }
    }
    public async Task<List<Page>> GetAllPagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/pages");
            if (!response.IsSuccessStatusCode)
                return new List<Page>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Page>>(json, _jsonOptions) ?? new List<Page>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pages");
            return new List<Page>();
        }
    }

    // GET: api/pages/public
    public async Task<List<Page>> GetPublicPagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/pages/public");
            if (!response.IsSuccessStatusCode)
                return new List<Page>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Page>>(json, _jsonOptions) ?? new List<Page>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching public pages");
            return new List<Page>();
        }
    }

    // GET: api/pages/navigation
    public async Task<List<Page>> GetNavigationPagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/pages/navigation");
            if (!response.IsSuccessStatusCode)
                return new List<Page>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Page>>(json, _jsonOptions) ?? new List<Page>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching navigation pages");
            return new List<Page>();
        }
    }

    // GET: api/pages/slug/{slug}
    public async Task<Page?> GetPageBySlugAsync(string slug)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/pages/slug/{slug}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Page>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching page with slug {slug}");
            return null;
        }
    }

    // POST: api/pages
    public async Task<Page?> CreatePageAsync(CreatePageDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/pages", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API error {response.StatusCode}: {responseJson}");
                throw new Exception($"API вернул ошибку: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<Page>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating page");
            throw;
        }
    }
    // PUT /api/dinosaurs/{id} - ОБНОВИТЬ
    public async Task<DinosaurDto?> UpdateAsync(int id, UpdateDinosaurDto dto)
    {
        try
        {
            // Убираем PhotoFile, используем только PhotoUrl
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Sending PUT request to /api/dinosaurs/{id} with: {json}");
            var response = await _httpClient.PutAsync($"/api/dinosaurs/{id}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API error {response.StatusCode}: {responseContent}");
                throw new Exception($"API вернул ошибку: {response.StatusCode} - {responseContent}");
            }

            _logger.LogInformation($"Update response: {responseContent}");
            var updatedDinosaur = JsonSerializer.Deserialize<DinosaurDto>(responseContent, _jsonOptions);

            if (updatedDinosaur != null)
            {
                FixImageUrl(updatedDinosaur);
            }

            return updatedDinosaur;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating dinosaur ID: {id}");
            throw;
        }
    }
    // PUT: api/pages/{id}
    public async Task<Page?> UpdatePageAsync(int id, UpdatePageDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/pages/{id}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"API error {response.StatusCode}: {responseJson}");
                throw new Exception($"API вернул ошибку: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<Page>(responseJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating page {id}");
            throw;
        }
    }

    // DELETE: api/pages/{id}
    public async Task DeletePageAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/pages/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error {response.StatusCode}: {error}");
                throw new Exception($"API вернул ошибку: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting page {id}");
            throw;
        }
    }
}