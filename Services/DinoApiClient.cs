// Services/DinoApiClient.cs
using System.Text;
using System.Text.Json;
using DinoApp.Models;
using Microsoft.AspNetCore.Http;

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

    // POST /api/dinosaurs - создать с файлом
    public async Task<DinosaurDto> CreateAsync(CreateDinosaurDto dto)
    {
        using var content = new MultipartFormDataContent();

        // Добавляем текстовые поля
        content.Add(new StringContent(dto.Name ?? ""), "Name");
        content.Add(new StringContent(dto.Era ?? ""), "Era");
        content.Add(new StringContent(dto.Clade ?? ""), "Clade");
        content.Add(new StringContent(dto.Period ?? ""), "Period");
        content.Add(new StringContent(dto.GroupName ?? ""), "GroupName");
        content.Add(new StringContent(dto.Size ?? ""), "Size");
        content.Add(new StringContent(dto.Description ?? ""), "Description");
        content.Add(new StringContent(dto.FullDescription ?? ""), "FullDescription");
        content.Add(new StringContent(dto.Diet ?? ""), "Diet");
        content.Add(new StringContent(dto.Locomotion ?? ""), "Locomotion");
        content.Add(new StringContent(dto.Continent ?? ""), "Continent");
        content.Add(new StringContent(dto.Status ?? ""), "Status");
        content.Add(new StringContent(dto.IsFeatured.ToString()), "IsFeatured");
        content.Add(new StringContent(dto.AllowComments.ToString()), "AllowComments");
        content.Add(new StringContent(dto.DiscoveryLocation ?? ""), "DiscoveryLocation");

        // Добавляем файл, если он есть
        if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
        {
            var fileContent = new StreamContent(dto.PhotoFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.PhotoFile.ContentType);
            content.Add(fileContent, "PhotoFile", dto.PhotoFile.FileName);
        }

        var response = await _httpClient.PostAsync("/api/dinosaurs", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DinosaurDto>(responseJson, _jsonOptions)
               ?? throw new Exception("Failed to create dinosaur");
    }

    // PUT /api/dinosaurs/{id} - обновить с файлом
    public async Task UpdateAsync(int id, UpdateDinosaurDto dto)
    {
        using var content = new MultipartFormDataContent();

        // Добавляем текстовые поля
        content.Add(new StringContent(dto.Name ?? ""), "Name");
        content.Add(new StringContent(dto.Era ?? ""), "Era");
        content.Add(new StringContent(dto.Clade ?? ""), "Clade");
        content.Add(new StringContent(dto.Period ?? ""), "Period");
        content.Add(new StringContent(dto.GroupName ?? ""), "GroupName");
        content.Add(new StringContent(dto.Size ?? ""), "Size");
        content.Add(new StringContent(dto.Description ?? ""), "Description");
        content.Add(new StringContent(dto.FullDescription ?? ""), "FullDescription");
        content.Add(new StringContent(dto.Diet ?? ""), "Diet");
        content.Add(new StringContent(dto.Locomotion ?? ""), "Locomotion");
        content.Add(new StringContent(dto.Continent ?? ""), "Continent");
        content.Add(new StringContent(dto.Status ?? ""), "Status");
        content.Add(new StringContent(dto.IsFeatured.ToString()), "IsFeatured");
        content.Add(new StringContent(dto.AllowComments.ToString()), "AllowComments");
        content.Add(new StringContent(dto.DiscoveryLocation ?? ""), "DiscoveryLocation");
        content.Add(new StringContent(dto.ExistingPhotoPath ?? ""), "ExistingPhotoPath");

        // Добавляем новый файл, если он есть
        if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
        {
            var fileContent = new StreamContent(dto.PhotoFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.PhotoFile.ContentType);
            content.Add(fileContent, "PhotoFile", dto.PhotoFile.FileName);
        }

        var response = await _httpClient.PutAsync($"/api/dinosaurs/{id}", content);
        response.EnsureSuccessStatusCode();
    }

    // DELETE /api/dinosaurs/{id} - удалить
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/dinosaurs/{id}");
        response.EnsureSuccessStatusCode();
    }
}