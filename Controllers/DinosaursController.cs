// Controllers/DinosaursController.cs
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
                // Логируем ошибку
                Console.WriteLine($"Ошибка при загрузке динозавров: {ex.Message}");

                // Показываем пустой список с сообщением об ошибке
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
                return View("Viewing", dinosaur); // Открывает DinosaurDetails.cshtml
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
            return View("CreateDinosaurs");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DinosaurDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                // Преобразуем DinosaurDto в UpdateDinosaurDto
                var updateDto = new UpdateDinosaurDto
                {
                    Name = dto.Name,
                    Era = dto.Era,
                    Clade = dto.Clade,
                    Period = dto.Period,
                    GroupName = dto.GroupName,
                    // Добавьте остальные поля, которые есть в UpdateDinosaurDto
                    PhotoUrl = dto.PhotoUrl,
                    Description = dto.Description,
                    // ... остальные поля
                };

                await _apiClient.UpdateAsync(id, updateDto);
                TempData["SuccessMessage"] = "Динозавр успешно обновлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении: {ex.Message}");
                ModelState.AddModelError("", "Ошибка при обновлении динозавра. Проверьте подключение к API.");
                return View(dto);
            }
        }

        // POST: /Dinosaurs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDinosaurDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit",dto);
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
                return View("Edit",dto);
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

                // Сохраняем ID для представления
                ViewBag.DinosaurId = id;

                // Просто передаем полученный DinosaurDto в представление
                return View("Edit", dinosaur); // dinosaur уже имеет тип DinosaurDto
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке для редактирования: {ex.Message}");
                return RedirectToAction(nameof(Index));
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
