/*

using Microsoft.AspNetCore.Mvc;
using tedd.Services;

namespace tedd.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRapportService _rapportService;

        public HomeController(IRapportService rapportService)
        {
            _rapportService = rapportService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _rapportService.GetDashboardData();
            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}*/

using Microsoft.AspNetCore.Mvc;
using tedd.Services;

namespace tedd.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRapportService _rapportService;

        public HomeController(IRapportService rapportService)
        {
            _rapportService = rapportService;
        }

        public async Task<IActionResult> Index()
        {
            // Vérifier si l'utilisateur est connecté
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboard = await _rapportService.GetDashboardData();
            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}