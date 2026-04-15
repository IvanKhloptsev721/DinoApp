using System.Diagnostics;
using System.Text.Json;
using DinoApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DinoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        // В HomeController.cs

        [HttpGet]
        public IActionResult Contacts()
        {
            return View(new ContactMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contacts(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем сообщение в файл
                    var messagesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "contact_messages.json");
                    var messages = new List<ContactMessage>();

                    if (System.IO.File.Exists(messagesFilePath))
                    {
                        var json = System.IO.File.ReadAllText(messagesFilePath);
                        messages = JsonSerializer.Deserialize<List<ContactMessage>>(json) ?? new List<ContactMessage>();
                    }

                    messages.Add(new ContactMessage
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Subject = model.Subject,
                        Message = model.Message
                    });

                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    System.IO.File.WriteAllText(messagesFilePath, JsonSerializer.Serialize(messages, jsonOptions));

                    // Отправляем уведомление администратору (опционально)
                    // await _emailService.SendEmailAsync("admin@dino-mir.com", 
                    //     $"Новое сообщение: {model.Subject}", 
                    //     $"От: {model.Name} ({model.Email})\n\n{model.Message}");

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
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dinosaurs");
        }

        // ДОБАВЬТЕ ЭТИ МЕТОДЫ:
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}