using Microsoft.AspNetCore.Mvc;
using DinoApp.Services;
using DinoApp.Models;
using DinoApp.Attributes;
using System.Security.Cryptography;

namespace DinoApp.Controllers;

using RegisterUserDto = DinoApp.Models.RegisterDto;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    // private readonly IEmailService _emailService; // Временно отключено
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, /* IEmailService emailService, */ ILogger<AuthController> logger)
    {
        _authService = authService;
        // _emailService = emailService; // Временно отключено
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Если уже авторизован - перенаправляем на главную
        if (User.Identity?.IsAuthenticated == true)
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

            if (result.success)
            {
                // Двухфакторная аутентификация временно отключена
                // if (result.requiresTwoFactor)
                // {
                //     TempData["InfoMessage"] = $"Код подтверждения отправлен на почту {result.email}";
                //     return RedirectToAction("VerifyCode", new { username = model.Username, returnUrl });
                // }

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
    public IActionResult VerifyCode(string username, string? returnUrl = null)
    {
        // Временно отключено
        TempData["InfoMessage"] = "Двухфакторная аутентификация временно отключена";
        return RedirectToAction("Login");

        // if (string.IsNullOrEmpty(username))
        //     return RedirectToAction("Login");
        //
        // ViewBag.Username = username;
        // ViewBag.ReturnUrl = returnUrl;
        // return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(string username, string code, string? returnUrl = null)
    {
        // Временно отключено
        TempData["InfoMessage"] = "Двухфакторная аутентификация временно отключена";
        return RedirectToAction("Login");

        // if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(code))
        // {
        //     ViewBag.Username = username;
        //     ModelState.AddModelError("", "Введите код подтверждения");
        //     return View();
        // }
        //
        // var result = await _authService.VerifyTwoFactorCodeAsync(username, code);
        //
        // if (result)
        // {
        //     if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        //         return Redirect(returnUrl);
        //
        //     TempData["SuccessMessage"] = "Вход выполнен успешно!";
        //     return RedirectToAction("Index", "Dinosaurs");
        // }
        //
        // ViewBag.Username = username;
        // ViewBag.ReturnUrl = returnUrl;
        // ModelState.AddModelError("", "Неверный код подтверждения или срок его действия истек");
        // return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode(string username)
    {
        // Временно отключено
        TempData["InfoMessage"] = "Двухфакторная аутентификация временно отключена";
        return RedirectToAction("Login");
    }

  [HttpGet]
[AllowAnonymous]
    public IActionResult Register()
    {
        // Если уже авторизован - перенаправляем на главную
        if (User.Identity?.IsAuthenticated == true)
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
                TempData["SuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти.";
                return RedirectToAction("Login");
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
    [Authorize]
    public IActionResult Profile(int? id = null)
    {
        User? user;

        if (id == null)
        {
            user = _authService.GetCurrentUser();
            ViewBag.IsOwnProfile = true;
        }
        else
        {
            user = _authService.GetUserById(id.Value);
            var currentUser = _authService.GetCurrentUser();
            ViewBag.IsOwnProfile = currentUser?.Id == id.ToString();
        }

        if (user == null)
            return NotFound();

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
    public async Task<IActionResult> EditProfile(UpdateProfileDto model)
    {
        var user = _authService.GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login");

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _authService.UpdateProfileAsync(user.Id, model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Профиль успешно обновлён!";
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", "Ошибка при обновлении профиля");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError("", $"Ошибка при обновлении профиля: {ex.Message}");
            }
        }

        return View(model);
    }
    [HttpPost]
    [Authorize] // Developer проверяется внутри
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDeveloper(int id)
    {
        var user = _authService.GetUserById(id);
        if (user != null && user.UserName != "admin" && user.UserName != "developer")
        {
            var result = await _authService.ToggleDeveloperAsync(user.Id);
            if (result)
            {
                TempData["SuccessMessage"] = $"Статус разработчика для {user.UserName} изменён!";
            }
            else
            {
                TempData["ErrorMessage"] = "Недостаточно прав для выполнения операции.";
            }
        }

        return RedirectToAction("Users");
    }
    [HttpPost]
    [Authorize(requireAdmin: true)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(int id)
    {
        var user = _authService.GetUserById(id);
        if (user != null && user.UserName != "admin")
        {
            await _authService.ToggleAdminAsync(user.Id);
            TempData["SuccessMessage"] = $"Статус администратора для {user.UserName} изменён!";
        }

        return RedirectToAction("Users");
    }

    [HttpGet]
    [Authorize(requireAdmin: true)]
    public IActionResult Users()
    {
        var users = _authService.GetAllUsers();
        return View(users);
    }

    public class UserStats
    {
        public int MemberDays { get; set; }
        public DateTime LastActive { get; set; }
        public int DinosaursViewed { get; set; }
    }
}