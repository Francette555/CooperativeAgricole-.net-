using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class PaiementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaiementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var paiements = await _context.Paiements
                .Include(p => p.Producteur)
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();
            return View(paiements);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProducteurId,Montant,Statut")] Paiement paiement)
        {
            if (ModelState.IsValid)
            {
                paiement.DatePaiement = DateTime.Now;
                paiement.Mois = DateTime.Now.ToString("MMMM");
                paiement.Annee = DateTime.Now.Year;
                _context.Add(paiement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Paiement enregistré avec succès !";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            return View(paiement);
        }

        // Autres actions CRUD...
    }
}