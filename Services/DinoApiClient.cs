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

    public DinoApiClient(IConfiguration config, IWebHostEnvironment environment)
    {
        _baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7127";
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // GET /api/dinosaurs - получить всех (ИСПРАВЛЕНО)
    public async Task<List<DinosaurDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("/api/dinosaurs");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var dinosaurs = JsonSerializer.Deserialize<List<DinosaurDto>>(json, _jsonOptions) ?? new();

        // ПРЕОБРАЗУЕМ ОТНОСИТЕЛЬНЫЕ ПУТИ В ПОЛНЫЕ URL ДЛЯ КАЖДОГО ДИНОЗАВРА
        foreach (var dino in dinosaurs)
        {
            FixImageUrl(dino);
        }

        return dinosaurs;
    }

    // GET /api/dinosaurs/{id} - получить одного (ИСПРАВЛЕНО)
    public async Task<DinosaurDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/dinosaurs/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var dinosaur = JsonSerializer.Deserialize<DinosaurDto>(json, _jsonOptions);

        // ПРЕОБРАЗУЕМ ОТНОСИТЕЛЬНЫЙ ПУТЬ В ПОЛНЫЙ URL
        if (dinosaur != null)
        {
            FixImageUrl(dinosaur);
        }

        return dinosaur;
    }

    // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ФИКСАЦИИ URL ИЗОБРАЖЕНИЙ
    private void FixImageUrl(DinosaurDto dinosaur)
    {
        if (dinosaur == null) return;

        // Проверяем PhotoPath
        if (!string.IsNullOrEmpty(dinosaur.PhotoPath) && !dinosaur.PhotoPath.StartsWith("http"))
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            var fixedUrl = $"{baseUrl}{dinosaur.PhotoPath}";
            dinosaur.PhotoPath = fixedUrl;
            dinosaur.PhotoUrl = fixedUrl; // Для обратной совместимости
        }

        // Проверяем PhotoUrl как запасной вариант
        if (!string.IsNullOrEmpty(dinosaur.PhotoUrl) && !dinosaur.PhotoUrl.StartsWith("http"))
        {
            var baseUrl = _baseUrl.TrimEnd('/');
            dinosaur.PhotoUrl = $"{baseUrl}{dinosaur.PhotoUrl}";
            if (string.IsNullOrEmpty(dinosaur.PhotoPath))
            {
                dinosaur.PhotoPath = dinosaur.PhotoUrl;
            }
        }

        // Отладочная информация
        Console.WriteLine($"Fixed URL for {dinosaur.Name}: {dinosaur.PhotoPath}");
    }

    // POST /api/dinosaurs - создать с файлом
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

        // Добавляем URL фотографии, если есть
        AddStringContent(content, "PhotoUrl", dto.PhotoUrl);

        // Добавляем файл, если он есть - ВАЖНО: имя поля должно быть "ImageFile"
        if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
        {
            var fileContent = new StreamContent(dto.PhotoFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.PhotoFile.ContentType);
            content.Add(fileContent, "ImageFile", dto.PhotoFile.FileName);
        }

        var response = await _httpClient.PostAsync("/api/dinosaurs", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API вернул ошибку: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DinosaurDto>(responseJson, _jsonOptions);

        // Преобразуем относительный путь в полный URL
        if (result != null)
        {
            FixImageUrl(result);
        }

        return result ?? throw new Exception("Failed to create dinosaur");
    }

    // PUT /api/dinosaurs/{id} - обновить с файлом
    // PUT /api/dinosaurs/{id} - обновить с файлом (ПОЛНОСТЬЮ ИСПРАВЛЕННЫЙ)
   // DinoApp/Services/DinoApiClient.cs (фрагмент)
public async Task<DinosaurDto?> UpdateAsync(int id, UpdateDinosaurDto dto)
{
    var content = new MultipartFormDataContent();
    
    // Добавляем все поля
    if (!string.IsNullOrEmpty(dto.Name)) content.Add(new StringContent(dto.Name), "Name");
    if (!string.IsNullOrEmpty(dto.Clade)) content.Add(new StringContent(dto.Clade), "Clade");
    if (!string.IsNullOrEmpty(dto.Era)) content.Add(new StringContent(dto.Era), "Era");
    if (!string.IsNullOrEmpty(dto.Period)) content.Add(new StringContent(dto.Period), "Period");
    if (!string.IsNullOrEmpty(dto.GroupName)) content.Add(new StringContent(dto.GroupName), "GroupName");
    if (!string.IsNullOrEmpty(dto.Genus)) content.Add(new StringContent(dto.Genus), "Genus");
    if (!string.IsNullOrEmpty(dto.Species)) content.Add(new StringContent(dto.Species), "Species");
    if (!string.IsNullOrEmpty(dto.Description)) content.Add(new StringContent(dto.Description), "Description");
    
    // Новые поля
    if (!string.IsNullOrEmpty(dto.Size)) content.Add(new StringContent(dto.Size), "Size");
    if (!string.IsNullOrEmpty(dto.FullDescription)) content.Add(new StringContent(dto.FullDescription), "FullDescription");
    if (!string.IsNullOrEmpty(dto.Diet)) content.Add(new StringContent(dto.Diet), "Diet");
    if (!string.IsNullOrEmpty(dto.Locomotion)) content.Add(new StringContent(dto.Locomotion), "Locomotion");
    if (!string.IsNullOrEmpty(dto.Continent)) content.Add(new StringContent(dto.Continent), "Continent");
    if (!string.IsNullOrEmpty(dto.Status)) content.Add(new StringContent(dto.Status), "Status");
    if (!string.IsNullOrEmpty(dto.DiscoveryLocation)) content.Add(new StringContent(dto.DiscoveryLocation), "DiscoveryLocation");
    if (!string.IsNullOrEmpty(dto.Comments)) content.Add(new StringContent(dto.Comments), "Comments");
    
    content.Add(new StringContent(dto.IsFeatured.ToString()), "IsFeatured");
    content.Add(new StringContent(dto.AllowComments.ToString()), "AllowComments");
    
    // Файл изображения
    if (dto.PhotoFile != null)
    {
        var stream = dto.PhotoFile.OpenReadStream();
        content.Add(new StreamContent(stream), "ImageFile", dto.PhotoFile.FileName);
    }
    else if (!string.IsNullOrEmpty(dto.PhotoUrl))
    {
        content.Add(new StringContent(dto.PhotoUrl), "PhotoUrl");
    }
    
    var response = await _httpClient.PutAsync($"api/dinosaurs/{id}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API вернул ошибку: {response.StatusCode} - {responseContent}");
        }

        // Получаем обновленного динозавра из ответа
        var updatedDinosaur = JsonSerializer.Deserialize<DinosaurDto>(responseContent, _jsonOptions);

        // Преобразуем URL изображения
        if (updatedDinosaur != null)
        {
            FixImageUrl(updatedDinosaur);
            Console.WriteLine($"Обновленный динозавр: {updatedDinosaur.Name}, PhotoPath: {updatedDinosaur.PhotoPath}");
        }

        return updatedDinosaur ?? throw new Exception("Failed to update dinosaur");
    }

    private void AddStringContent(MultipartFormDataContent content, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            content.Add(new StringContent(value), name);
            Console.WriteLine($"Добавлено поле {name}: {value}");
        }
    }
    // DELETE /api/dinosaurs/{id} - удалить
    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/dinosaurs/{id}");
        response.EnsureSuccessStatusCode();
    }

}