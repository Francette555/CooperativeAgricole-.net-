using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class StocksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stocks = await _context.Stocks
                .Include(s => s.TypeProduit)
                .OrderBy(s => s.TypeProduit.Nom)
                .ToListAsync();
            return View(stocks);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .Include(s => s.TypeProduit)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypeProduitId,Quantite,SeuilAlerte")] Stock stock)
        {
            if (id != stock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Stocks.FindAsync(id);
                    if (existing != null)
                    {
                        existing.Quantite = stock.Quantite;
                        existing.SeuilAlerte = stock.SeuilAlerte;
                        existing.DateMiseAJour = DateTime.Now;

                        _context.Update(existing);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Stock mis à jour avec succès !";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockExists(stock.Id))
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
            return View(stock);
        }

        private bool StockExists(int id)
        {
            return _context.Stocks.Any(e => e.Id == id);
        }
    }
}