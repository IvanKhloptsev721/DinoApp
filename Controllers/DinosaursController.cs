using Microsoft.AspNetCore.Mvc;
using DinoApp.Services;
using DinoApp.Models;

namespace DinoApp.Controllers
{
    public class DinosaursController : Controller
    {
        private readonly DinoApiClient _apiClient;

        public DinosaursController(DinoApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Dinosaurs
        public async Task<IActionResult> Index()
        {
            try
            {
                var dinosaurs = await _apiClient.GetAllAsync();
                return View(dinosaurs ?? new List<DinosaurDto>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке динозавров: {ex.Message}");
                ViewBag.ErrorMessage = "Не удалось загрузить динозавров. Проверьте подключение к API.";
                return View(new List<DinosaurDto>());
            }
        }

        // GET: /Dinosaurs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var dinosaur = await _apiClient.GetByIdAsync(id);
                if (dinosaur == null)
                {
                    return NotFound();
                }
                return View("Viewing", dinosaur);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке динозавра: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Dinosaurs/Create
        public IActionResult Create()
        {
            return View("CreateDinosaurs", new CreateDinosaurDto());
        }

        // POST: /Dinosaurs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDinosaurDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateDinosaurs", dto);
            }

            try
            {
                await _apiClient.CreateAsync(dto);
                TempData["SuccessMessage"] = "Динозавр успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании: {ex.Message}");
                ModelState.AddModelError("", "Ошибка при создании динозавра. Проверьте подключение к API.");
                return View("CreateDinosaurs", dto);
            }
        }

        // GET: /Dinosaurs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var dinosaur = await _apiClient.GetByIdAsync(id);
                if (dinosaur == null)
                {
                    return NotFound();
                }

                return View("Edit", dinosaur);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке для редактирования: {ex.Message}");
                TempData["ErrorMessage"] = "Не удалось загрузить данные динозавра.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Dinosaurs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DinosaurDto dto)
        {
            Console.WriteLine($"=== РЕДАКТИРОВАНИЕ ДИНОЗАВРА ===");
            Console.WriteLine($"ID: {id}");
            Console.WriteLine($"DTO ID: {dto.Id}");
            Console.WriteLine($"Name: {dto.Name}");
            Console.WriteLine($"PhotoFile: {dto.PhotoFile?.FileName ?? "null"}");
            Console.WriteLine($"PhotoUrl: {dto.PhotoUrl}");

            if (id != dto.Id)
            {
                Console.WriteLine("ID не совпадают!");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState не валиден!");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка: {error.ErrorMessage}");
                }
                return View("Edit", dto);
            }

            try
            {
                var updateDto = new UpdateDinosaurDto
                {
                    Name = dto.Name,
                    Era = dto.Era,
                    Clade = dto.Clade,
                    Period = dto.Period,
                    GroupName = dto.GroupName,
                    Genus = dto.Genus,
                    Species = dto.Species,
                    Size = dto.Size,
                    Description = dto.Description,
                    FullDescription = dto.FullDescription,
                    Diet = dto.Diet,
                    Locomotion = dto.Locomotion,
                    Continent = dto.Continent,
                    Status = dto.Status,
                    IsFeatured = dto.IsFeatured,
                    AllowComments = dto.AllowComments,
                    DiscoveryLocation = dto.DiscoveryLocation,
                    Comments = dto.Comments,
                    PhotoFile = dto.PhotoFile,
                    PhotoUrl = dto.PhotoFile == null ? dto.PhotoUrl : null
                };

                Console.WriteLine("Отправляем запрос на обновление...");
                var updatedDinosaur = await _apiClient.UpdateAsync(id, updateDto);

                Console.WriteLine($"Обновление успешно! Новый PhotoPath: {updatedDinosaur?.PhotoPath}");
                TempData["SuccessMessage"] = "Динозавр успешно обновлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА ПРИ ОБНОВЛЕНИИ: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Ошибка при обновлении динозавра: {ex.Message}");
                return View("Edit", dto);
            }
        }

        // POST: /Dinosaurs/AddComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string userName, string commentText)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Имя и текст комментария обязательны!";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                // Получаем текущего динозавра
                var dinosaur = await _apiClient.GetByIdAsync(id);
                if (dinosaur == null)
                {
                    return NotFound();
                }

                // Формируем новый комментарий в формате "Дата||Имя||Текст"
                var newComment = $"{DateTime.Now:dd.MM.yyyy}||{userName}||{commentText}";

                // Добавляем к существующим комментариям
                string updatedComments;
                if (string.IsNullOrEmpty(dinosaur.Comments))
                {
                    updatedComments = newComment;
                }
                else
                {
                    updatedComments = dinosaur.Comments + "\n" + newComment;
                }

                // Создаем DTO для обновления
                var updateDto = new UpdateDinosaurDto
                {
                    Name = dinosaur.Name,
                    Era = dinosaur.Era,
                    Clade = dinosaur.Clade,
                    Period = dinosaur.Period,
                    GroupName = dinosaur.GroupName,
                    Genus = dinosaur.Genus,
                    Species = dinosaur.Species,
                    Size = dinosaur.Size,
                    Description = dinosaur.Description,
                    FullDescription = dinosaur.FullDescription,
                    Diet = dinosaur.Diet,
                    Locomotion = dinosaur.Locomotion,
                    Continent = dinosaur.Continent,
                    Status = dinosaur.Status,
                    IsFeatured = dinosaur.IsFeatured,
                    AllowComments = dinosaur.AllowComments,
                    DiscoveryLocation = dinosaur.DiscoveryLocation,
                    Comments = updatedComments,
                    PhotoUrl = dinosaur.PhotoPath
                };

                // Отправляем обновление
                await _apiClient.UpdateAsync(id, updateDto);

                TempData["SuccessMessage"] = "Комментарий успешно добавлен!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комментария: {ex.Message}");
                TempData["ErrorMessage"] = "Не удалось добавить комментарий.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Dinosaurs/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dinosaur = await _apiClient.GetByIdAsync(id);
                if (dinosaur == null)
                {
                    return NotFound();
                }
                return View(dinosaur);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке для удаления: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Dinosaurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiClient.DeleteAsync(id);
                TempData["SuccessMessage"] = "Динозавр успешно удален!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении: {ex.Message}");
                TempData["ErrorMessage"] = "Ошибка при удалении динозавра.";
            }

            return RedirectToAction(nameof(Index));
        }
        // Добавьте в DinosaursController.cs

// GET: /Dinosaurs/Map
public async Task<IActionResult> Map(string? continent, string? dinoName)
{
    try
    {
        var allDinosaurs = await _apiClient.GetAllAsync();
        var filteredDinosaurs = allDinosaurs?.ToList() ?? new List<DinosaurDto>();
        
        // Фильтрация по континенту
        if (!string.IsNullOrEmpty(continent))
        {
            filteredDinosaurs = filteredDinosaurs
                .Where(d => d.Continent != null && d.Continent.Contains(continent, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        // Фильтрация по названию динозавра
        if (!string.IsNullOrEmpty(dinoName))
        {
            filteredDinosaurs = filteredDinosaurs
                .Where(d => d.Name != null && d.Name.Contains(dinoName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        var viewModel = new DinoMapViewModel
        {
            Dinosaurs = filteredDinosaurs,
            SelectedContinent = continent,
            SelectedDinoName = dinoName
        };
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при загрузке карты: {ex.Message}");
        return View(new DinoMapViewModel());
    }
}
    }
}