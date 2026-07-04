using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class VentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .OrderByDescending(v => v.DateVente)
                .ToListAsync();
            return View(ventes);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeProduitId,Quantite,PrixUnitaire,Client")] Vente vente)
        {
            if (ModelState.IsValid)
            {
                // Vérifier le stock
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.TypeProduitId == vente.TypeProduitId);
                if (stock == null || stock.Quantite < vente.Quantite)
                {
                    ModelState.AddModelError("", "Stock insuffisant pour cette vente.");
                    ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
                    return View(vente);
                }

                vente.DateVente = DateTime.Now;
                _context.Add(vente);

                // Mettre à jour le stock
                stock.Quantite -= vente.Quantite;
                stock.DateMiseAJour = DateTime.Now;
                _context.Update(stock);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Vente enregistrée avec succès !";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View(vente);
        }

        // Autres actions CRUD similaires...
    }
}