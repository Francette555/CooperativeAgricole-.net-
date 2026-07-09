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

        // GET: Ventes
        public async Task<IActionResult> Index()
        {
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .OrderByDescending(v => v.DateVente)
                .ToListAsync();
            return View(ventes);
        }

        // GET: Ventes/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View();
        }

        // POST: Ventes/Create
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

        // GET: Ventes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vente = await _context.Ventes
                .Include(v => v.TypeProduit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vente == null)
            {
                return NotFound();
            }

            // Calculer le nouveau stock après suppression
            var stockActuel = await _context.Stocks
                .FirstOrDefaultAsync(s => s.TypeProduitId == vente.TypeProduitId);

            // Initialiser les ViewBag avec des valeurs par défaut
            ViewBag.StockActuel = stockActuel?.Quantite ?? 0;
            ViewBag.NouveauStock = (stockActuel?.Quantite ?? 0) + vente.Quantite;
            ViewBag.Unite = vente.TypeProduit?.Unite ?? "kg";
            ViewBag.ProduitNom = vente.TypeProduit?.Nom ?? "Produit inconnu";

            return View(vente);
        }

        // POST: Ventes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vente = await _context.Ventes
                .Include(v => v.TypeProduit)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vente != null)
            {
                // Restaurer le stock
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.TypeProduitId == vente.TypeProduitId);
                if (stock != null)
                {
                    stock.Quantite += vente.Quantite;
                    stock.DateMiseAJour = DateTime.Now;
                    _context.Update(stock);
                }

                _context.Ventes.Remove(vente);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vente supprimée avec succès !";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenteExists(int id)
        {
            return _context.Ventes.Any(e => e.Id == id);
        }
    }
}