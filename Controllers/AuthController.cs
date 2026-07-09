using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login()
        {
            // Si déjà connecté, rediriger vers l'accueil
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Rechercher l'utilisateur
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password && u.EstActif);

                if (utilisateur != null)
                {
                    // Stocker les informations en session
                    HttpContext.Session.SetString("UserId", utilisateur.Id.ToString());
                    HttpContext.Session.SetString("UserEmail", utilisateur.Email);
                    HttpContext.Session.SetString("UserRole", utilisateur.Role);
                    HttpContext.Session.SetString("UserNom", $"{utilisateur.Prenom} {utilisateur.Nom}");

                    TempData["Success"] = $"Bienvenue {utilisateur.Prenom} {utilisateur.Nom} !";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            }

            return View(model);
        }

        // GET: Logout
        public IActionResult Logout()
        {
            // Vider la session
            HttpContext.Session.Clear();
            TempData["Success"] = "Vous avez été déconnecté avec succès.";
            return RedirectToAction("Login");
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Utilisateur utilisateur)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'email existe déjà
                var existant = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == utilisateur.Email);

                if (existant != null)
                {
                    ModelState.AddModelError("", "Cet email est déjà utilisé.");
                    return View(utilisateur);
                }

                utilisateur.DateCreation = DateTime.Now;
                utilisateur.Role = "Utilisateur";
                utilisateur.EstActif = true;

                _context.Utilisateurs.Add(utilisateur);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
                return RedirectToAction("Login");
            }

            return View(utilisateur);
        }
    }
}