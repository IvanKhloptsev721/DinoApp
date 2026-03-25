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
    // Services/DinoApiClient.cs (исправленный CreateAsync)
    public async Task<DinosaurDto> CreateAsync(CreateDinosaurDto dto)
    {
        using var content = new MultipartFormDataContent();

        // Добавляем текстовые поля
        AddStringContent(content, "Name", dto.Name);
        AddStringContent(content, "Era", dto.Era);
        AddStringContent(content, "Clade", dto.Clade);
        AddStringContent(content, "Period", dto.Period);
        AddStringContent(content, "GroupName", dto.GroupName);
        AddStringContent(content, "Genus", dto.Genus);
        AddStringContent(content, "Species", dto.Species);
        AddStringContent(content, "Size", dto.Size);
        AddStringContent(content, "Description", dto.Description);
        AddStringContent(content, "FullDescription", dto.FullDescription);
        AddStringContent(content, "Diet", dto.Diet);
        AddStringContent(content, "Locomotion", dto.Locomotion);
        AddStringContent(content, "Continent", dto.Continent);
        AddStringContent(content, "Status", dto.Status);
        AddStringContent(content, "IsFeatured", dto.IsFeatured.ToString());
        AddStringContent(content, "AllowComments", dto.AllowComments.ToString());
        AddStringContent(content, "DiscoveryLocation", dto.DiscoveryLocation);
        AddStringContent(content, "Comments", dto.Comments);

        // Добавляем файл, если он есть
        if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
        {
            var fileContent = new StreamContent(dto.PhotoFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.PhotoFile.ContentType);
            content.Add(fileContent, "ImageFile", dto.PhotoFile.FileName); // Изменено с PhotoFile на ImageFile
        }

        var response = await _httpClient.PostAsync("/api/dinosaurs", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DinosaurDto>(responseJson, _jsonOptions)
               ?? throw new Exception("Failed to create dinosaur");
    }

    // Services/DinoApiClient.cs (исправленный UpdateAsync)
    public async Task UpdateAsync(int id, UpdateDinosaurDto dto)
    {
        using var content = new MultipartFormDataContent();

        // Добавляем текстовые поля
        AddStringContent(content, "Name", dto.Name);
        AddStringContent(content, "Era", dto.Era);
        AddStringContent(content, "Clade", dto.Clade);
        AddStringContent(content, "Period", dto.Period);
        AddStringContent(content, "GroupName", dto.GroupName);
        AddStringContent(content, "Genus", dto.Genus);
        AddStringContent(content, "Species", dto.Species);
        AddStringContent(content, "Size", dto.Size);
        AddStringContent(content, "Description", dto.Description);
        AddStringContent(content, "FullDescription", dto.FullDescription);
        AddStringContent(content, "Diet", dto.Diet);
        AddStringContent(content, "Locomotion", dto.Locomotion);
        AddStringContent(content, "Continent", dto.Continent);
        AddStringContent(content, "Status", dto.Status);
        AddStringContent(content, "IsFeatured", dto.IsFeatured.ToString());
        AddStringContent(content, "AllowComments", dto.AllowComments.ToString());
        AddStringContent(content, "DiscoveryLocation", dto.DiscoveryLocation);
        AddStringContent(content, "Comments", dto.Comments);

        // Добавляем новый файл, если он есть
        if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
        {
            var fileContent = new StreamContent(dto.PhotoFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.PhotoFile.ContentType);
            content.Add(fileContent, "ImageFile", dto.PhotoFile.FileName); // Изменено с PhotoFile на ImageFile
        }

        var response = await _httpClient.PutAsync($"/api/dinosaurs/{id}", content);
        response.EnsureSuccessStatusCode();
    }

    // Вспомогательный метод для добавления строковых полей
    private void AddStringContent(MultipartFormDataContent content, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            content.Add(new StringContent(value), name);
        }
    }
    // DELETE /api/dinosaurs/{id} - удалить
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/dinosaurs/{id}");
        response.EnsureSuccessStatusCode();
    }
}