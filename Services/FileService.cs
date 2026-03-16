// Services/FileService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DinoApp.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "dinosaurs");

            // Создаем папку, если её нет
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл не выбран или пуст");

            // Проверяем размер
            if (file.Length > _maxFileSize)
                throw new InvalidOperationException($"Файл слишком большой. Максимальный размер: {_maxFileSize / 1024 / 1024} MB");

            // Проверяем расширение
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Недопустимый формат файла. Разрешены: " + string.Join(", ", _allowedExtensions));

            // Генерируем уникальное имя файла
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadsFolder, fileName);

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Возвращаем относительный путь для сохранения в БД
            return $"/uploads/dinosaurs/{fileName}";
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}