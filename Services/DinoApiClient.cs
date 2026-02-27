// Services/DinoApiClient.cs
using System.Text;
using System.Text.Json;
using DinoApp.Models;

namespace DinoApp.Services;

public class DinoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DinoApiClient(IConfiguration config)
    {
        var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7127";
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // GET /api/dinosaurs - получить всех
    public async Task<List<DinosaurDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("/api/dinosaurs");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DinosaurDto>>(json, _jsonOptions) ?? new();
    }

    // GET /api/dinosaurs/{id} - получить одного
    public async Task<DinosaurDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/dinosaurs/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DinosaurDto>(json, _jsonOptions);
    }

    // GET /api/dinosaurs/slug/{slug} - получить по slug
    public async Task<DinosaurDto?> GetBySlugAsync(string slug)
    {
        var response = await _httpClient.GetAsync($"/api/dinosaurs/slug/{slug}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DinosaurDto>(json, _jsonOptions);
    }

    // POST /api/dinosaurs - создать
    public async Task<DinosaurDto> CreateAsync(CreateDinosaurDto dto)
    {
        var json = JsonSerializer.Serialize(dto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/dinosaurs", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DinosaurDto>(responseJson, _jsonOptions)
               ?? throw new Exception("Failed to create dinosaur");
    }

    // PUT /api/dinosaurs/{id} - обновить
    public async Task UpdateAsync(int id, UpdateDinosaurDto dto)
    {
        var json = JsonSerializer.Serialize(dto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/api/dinosaurs/{id}", content);
        response.EnsureSuccessStatusCode();
    }

    // DELETE /api/dinosaurs/{id} - удалить
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/dinosaurs/{id}");
        response.EnsureSuccessStatusCode();
    }

    // GET /api/dinosaurs/era/{era} - фильтр по эре
    public async Task<List<DinosaurDto>> GetByEraAsync(string era)
    {
        var response = await _httpClient.GetAsync($"/api/dinosaurs/era/{era}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DinosaurDto>>(json, _jsonOptions) ?? new();
    }

    // GET /api/dinosaurs/clade/{clade} - фильтр по кладе
    public async Task<List<DinosaurDto>> GetByCladeAsync(string clade)
    {
        var response = await _httpClient.GetAsync($"/api/dinosaurs/clade/{clade}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DinosaurDto>>(json, _jsonOptions) ?? new();
    }
}