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

                return View("Edit", dinosaur); // ВСЕГДА возвращаем DinosaurDto
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке для редактирования: {ex.Message}");
                TempData["ErrorMessage"] = "Не удалось загрузить данные динозавра.";
                return RedirectToAction(nameof(Index));
            }
        }

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
                    PhotoUrl = dto.PhotoFile == null ? dto.PhotoUrl : null // Важно!
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

    }
}