using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class RecoltesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecoltesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recoltes = await _context.Recoltes
                .Include(r => r.Producteur)
                .Include(r => r.TypeProduit)
                .OrderByDescending(r => r.DateRecolte)
                .ToListAsync();
            return View(recoltes);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProducteurId,TypeProduitId,Quantite,Qualite")] Recolte recolte)
        {
            if (ModelState.IsValid)
            {
                recolte.DateRecolte = DateTime.Now;
                _context.Add(recolte);
                await _context.SaveChangesAsync();

                // Mettre à jour le stock
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.TypeProduitId == recolte.TypeProduitId);
                if (stock != null)
                {
                    stock.Quantite += recolte.Quantite;
                    stock.DateMiseAJour = DateTime.Now;
                }
                else
                {
                    stock = new Stock
                    {
                        TypeProduitId = recolte.TypeProduitId,
                        Quantite = recolte.Quantite,
                        DateMiseAJour = DateTime.Now
                    };
                    _context.Add(stock);
                }
                await _context.SaveChangesAsync();

                TempData["Success"] = "Récolte enregistrée avec succès !";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View(recolte);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recolte = await _context.Recoltes.FindAsync(id);
            if (recolte == null)
            {
                return NotFound();
            }

            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View(recolte);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProducteurId,TypeProduitId,Quantite,DateRecolte,Qualite")] Recolte recolte)
        {
            if (id != recolte.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Récupérer l'ancienne quantité
                    var ancienne = await _context.Recoltes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    if (ancienne != null)
                    {
                        // Ajuster le stock
                        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.TypeProduitId == recolte.TypeProduitId);
                        if (stock != null)
                        {
                            stock.Quantite = stock.Quantite - ancienne.Quantite + recolte.Quantite;
                            stock.DateMiseAJour = DateTime.Now;
                            _context.Update(stock);
                        }
                    }

                    _context.Update(recolte);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Récolte modifiée avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecolteExists(recolte.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Producteurs = await _context.Producteurs.Where(p => p.EstActif).ToListAsync();
            ViewBag.TypesProduits = await _context.TypesProduits.ToListAsync();
            return View(recolte);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recolte = await _context.Recoltes
                .Include(r => r.Producteur)
                .Include(r => r.TypeProduit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recolte == null)
            {
                return NotFound();
            }

            return View(recolte);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recolte = await _context.Recoltes.FindAsync(id);
            if (recolte != null)
            {
                // Ajuster le stock
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.TypeProduitId == recolte.TypeProduitId);
                if (stock != null)
                {
                    stock.Quantite -= recolte.Quantite;
                    if (stock.Quantite < 0) stock.Quantite = 0;
                    stock.DateMiseAJour = DateTime.Now;
                    _context.Update(stock);
                }

                _context.Recoltes.Remove(recolte);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Récolte supprimée avec succès !";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RecolteExists(int id)
        {
            return _context.Recoltes.Any(e => e.Id == id);
        }
    }
}