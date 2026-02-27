using System.Diagnostics;
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

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dinosaurs");
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
        //public async Task<IActionResult> Index()
        //{
        //    var dinosaurs = await _apiClient.GetDinosaursAsync();
        //    return View(dinosaurs);
        //}
    }
}
