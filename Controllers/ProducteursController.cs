using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using tedd.Data;
using tedd.Models;

namespace tedd.Controllers
{
    public class ProducteursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProducteursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Producteurs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Producteurs.ToListAsync());
        }

        // GET: Producteurs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producteur = await _context.Producteurs
                .Include(p => p.Recoltes)
                    .ThenInclude(r => r.TypeProduit)
                .Include(p => p.Paiements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producteur == null)
            {
                return NotFound();
            }

            return View(producteur);
        }

        // GET: Producteurs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Producteurs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Telephone,Email,Adresse,EstActif")] Producteur producteur)
        {
            if (ModelState.IsValid)
            {
                producteur.DateInscription = DateTime.Now;
                _context.Add(producteur);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producteur créé avec succès !";
                return RedirectToAction(nameof(Index));
            }
            return View(producteur);
        }

        // GET: Producteurs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producteur = await _context.Producteurs.FindAsync(id);
            if (producteur == null)
            {
                return NotFound();
            }
            return View(producteur);
        }

        // POST: Producteurs/Edit/5
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Telephone,Email,Adresse,EstActif")] Producteur producteur)
         {
             if (id != producteur.Id)
             {
                 return NotFound();
             }

             if (ModelState.IsValid)
             {
                 try
                 {
                     var existing = await _context.Producteurs.FindAsync(id);
                     if (existing != null)
                     {
                         existing.Nom = producteur.Nom;
                         existing.Prenom = producteur.Prenom;
                         existing.Telephone = producteur.Telephone;
                         existing.Email = producteur.Email;
                         existing.Adresse = producteur.Adresse;
                         existing.EstActif = producteur.EstActif;

                         _context.Update(existing);
                         await _context.SaveChangesAsync();
                         TempData["Success"] = "Producteur mis à jour avec succès !";
                     }
                 }
                 catch (DbUpdateConcurrencyException)
                 {
                     if (!ProducteurExists(producteur.Id))
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
             return View(producteur);
         }

         // GET: Producteurs/Delete/5
         public async Task<IActionResult> Delete(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var producteur = await _context.Producteurs
                 .FirstOrDefaultAsync(m => m.Id == id);
             if (producteur == null)
             {
                 return NotFound();
             }

             return View(producteur);
         }

         // POST: Producteurs/Delete/5
         [HttpPost, ActionName("Delete")]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> DeleteConfirmed(int id)
         {
             var producteur = await _context.Producteurs.FindAsync(id);
             if (producteur != null)
             {
                 _context.Producteurs.Remove(producteur);
                 await _context.SaveChangesAsync();
                 TempData["Success"] = "Producteur supprimé avec succès !";
             }

             return RedirectToAction(nameof(Index));
         }

         private bool ProducteurExists(int id)
         {
             return _context.Producteurs.Any(e => e.Id == id);
         }

        
    }
}