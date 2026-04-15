using Microsoft.AspNetCore.Mvc;
using DinoApp.Models;
using System.Text.Json;
using DinoApp.Attributes;
using DinoApp.Services;

namespace DinoApp.Controllers
{
    [Authorize(requireAdmin: true)]
    public class PagesController : Controller
    {
        private readonly DinoApiClient _apiClient;
        private readonly ILogger<PagesController> _logger;

        public PagesController(DinoApiClient apiClient, ILogger<PagesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // Главная страница управления страницами (только для админов)
        public async Task<IActionResult> Index()
        {
            var pages = await _apiClient.GetAllPagesAsync();
            return View(pages);
        }

        // Публичные страницы
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Faq()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Contacts()
        {
            return View(new ContactMessage());
        }

        [AllowAnonymous]
        public IActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contacts(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var messagesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "contact_messages.json");
                    var messages = new List<ContactMessage>();

                    if (System.IO.File.Exists(messagesFilePath))
                    {
                        var json = await System.IO.File.ReadAllTextAsync(messagesFilePath);
                        messages = JsonSerializer.Deserialize<List<ContactMessage>>(json) ?? new List<ContactMessage>();
                    }

                    messages.Add(new ContactMessage
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Subject = model.Subject,
                        Message = model.Message,
                        CreatedAt = DateTime.Now
                    });

                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    await System.IO.File.WriteAllTextAsync(messagesFilePath, JsonSerializer.Serialize(messages, jsonOptions));

                    TempData["SuccessMessage"] = "Ваше сообщение успешно отправлено! Мы ответим вам в ближайшее время.";
                    return RedirectToAction("Contacts");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact message");
                    ModelState.AddModelError("", "Произошла ошибка при отправке сообщения. Пожалуйста, попробуйте позже.");
                }
            }

            return View(model);
        }

        // Методы для управления страницами (админка)
        public IActionResult Create()
        {
            return View(new CreatePageDto { IsPublished = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePageDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var page = await _apiClient.CreatePageAsync(dto);
                TempData["SuccessMessage"] = $"Страница \"{page.Title}\" успешно создана!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page");
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var pages = await _apiClient.GetAllPagesAsync();
            var page = pages.FirstOrDefault(p => p.Id == id);

            if (page == null)
                return NotFound();

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Page page)
        {
            if (!ModelState.IsValid)
                return View(page);

            try
            {
                var updateDto = new UpdatePageDto
                {
                    Title = page.Title,
                    Slug = page.Slug,
                    Content = page.Content,
                    MetaDescription = page.MetaDescription,
                    MetaKeywords = page.MetaKeywords,
                    IsPublished = page.IsPublished,
                    ButtonText = page.ButtonText,
                    ButtonUrl = page.ButtonUrl,
                    ButtonColor = page.ButtonColor,
                    ShowInNavigation = page.ShowInNavigation,
                    NavigationOrder = page.NavigationOrder
                };

                var updatedPage = await _apiClient.UpdatePageAsync(id, updateDto);
                TempData["SuccessMessage"] = $"Страница \"{updatedPage.Title}\" успешно обновлена!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page");
                ModelState.AddModelError("", ex.Message);
                return View(page);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var pages = await _apiClient.GetAllPagesAsync();
            var page = pages.FirstOrDefault(p => p.Id == id);

            if (page == null)
                return NotFound();

            return View(page);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiClient.DeletePageAsync(id);
                TempData["SuccessMessage"] = "Страница успешно удалена!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("page/{slug}")]
        public async Task<IActionResult> ViewPage(string slug)
        {
            var page = await _apiClient.GetPageBySlugAsync(slug);

            if (page == null)
                return NotFound();

            return View(page);
        }
    }
}