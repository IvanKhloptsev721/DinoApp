using Microsoft.AspNetCore.Mvc;
using DinoApp.Services;
using DinoApp.Models;
using DinoApp.Attributes;
using Microsoft.AspNetCore.Http;
namespace DinoApp.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Dinosaurs");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.LoginAsync(model.Username, model.Password, model.RememberMe);

            if (result)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                TempData["SuccessMessage"] = $"Добро пожаловать, {model.Username}!";
                return RedirectToAction("Index", "Dinosaurs");
            }

            ModelState.AddModelError("", "Неверное имя пользователя или пароль");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Dinosaurs");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserDto model)
    {
        if (ModelState.IsValid)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают");
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result)
            {
                await _authService.LoginAsync(model.Username, model.Password, false);
                TempData["SuccessMessage"] = "Регистрация прошла успешно! Добро пожаловать в Dino_Mir!";
                return RedirectToAction("Index", "Dinosaurs");
            }

            ModelState.AddModelError("", "Пользователь с таким именем или email уже существует");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        _authService.Logout();
        TempData["SuccessMessage"] = "Вы успешно вышли из системы";
        return RedirectToAction("Index", "Dinosaurs");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ========== ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ ==========

    [HttpGet]
    [Authorize]
    public IActionResult Profile(int? id = null)
    {
        User? user;

        // Если id не указан, показываем профиль текущего пользователя
        if (id == null)
        {
            user = _authService.GetCurrentUser();
            ViewBag.IsOwnProfile = true;
        }
        else
        {
            user = _authService.GetUserById(id.Value);
            var currentUser = _authService.GetCurrentUser();
            ViewBag.IsOwnProfile = currentUser?.Id == id;
        }

        if (user == null)
            return NotFound();

        // Получаем статистику пользователя (можно расширить)
        ViewBag.UserStats = new UserStats
        {
            MemberDays = (DateTime.Now - user.CreatedAt).Days,
            LastActive = user.LastLoginAt ?? user.CreatedAt
        };

        return View(user);
    }

    [HttpGet]
    [Authorize]
    public IActionResult EditProfile()
    {
        var user = _authService.GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login");

        var model = new UpdateProfileDto
        {
            FullName = user.FullName,
            Bio = user.Bio,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(UpdateProfileDto model, IFormFile? avatarFile)
    {
        var user = _authService.GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login");

        if (ModelState.IsValid)
        {
            // Обработка загрузки аватара
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    // Создаём директорию для аватаров
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Генерируем уникальное имя файла
                    var fileName = $"{user.Username}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    model.AvatarUrl = $"/uploads/avatars/{fileName}";
                }
            }

            var result = await _authService.UpdateProfileAsync(user.Id, model);

            if (result)
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлён!";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Ошибка при обновлении профиля");
        }

        return View(model);
    }
}

public class UserStats
{
    public int MemberDays { get; set; }
    public DateTime LastActive { get; set; }
    public int DinosaursViewed { get; set; }
}