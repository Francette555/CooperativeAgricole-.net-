// Controllers/RapportsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using tedd.Data;
using tedd.Models;
using tedd.Services;

namespace tedd.Controllers
{
    public class RapportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRapportService _rapportService;

        public RapportsController(ApplicationDbContext context, IRapportService rapportService)
        {
            _context = context;
            _rapportService = rapportService;
        }

        // GET: Rapports
        public async Task<IActionResult> Index()
        {
            var rapports = await _rapportService.GetRapportsHistorique();
            return View(rapports);
        }

        // GET: Rapports/Generate
        public async Task<IActionResult> Generate(int? mois, int? annee)
        {
            // Valeurs par défaut
            int moisActuel = mois ?? DateTime.Now.Month;
            int anneeActuelle = annee ?? DateTime.Now.Year;

            ViewBag.Mois = moisActuel;
            ViewBag.Annee = anneeActuelle;
            ViewBag.ListeMois = GetListeMois();
            ViewBag.ListeAnnees = GetListeAnnees();

            // Vérifier si un rapport existe déjà pour cette période
            var rapportExistant = await _context.RapportsFinanciers
                .FirstOrDefaultAsync(r => r.Mois == CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(moisActuel)
                    && r.Annee == anneeActuelle);

            if (rapportExistant != null)
            {
                ViewBag.RapportExistant = rapportExistant;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int mois, int annee)
        {
            if (mois == 0 || annee == 0)
            {
                TempData["Error"] = "Veuillez sélectionner un mois et une année valides.";
                return RedirectToAction(nameof(Generate));
            }

            // Vérifier si le rapport existe déjà
            var rapportExistant = await _context.RapportsFinanciers
                .FirstOrDefaultAsync(r => r.Mois == CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mois)
                    && r.Annee == annee);

            if (rapportExistant != null)
            {
                TempData["Warning"] = $"Un rapport pour {rapportExistant.Mois} {rapportExistant.Annee} existe déjà.";
                return RedirectToAction(nameof(Details), new { id = rapportExistant.Id });
            }

            try
            {
                var rapport = await _rapportService.GenererRapportMensuel(mois, annee);
                TempData["Success"] = $"Rapport pour {rapport.Mois} {rapport.Annee} généré avec succès !";
                return RedirectToAction(nameof(Details), new { id = rapport.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la génération du rapport : {ex.Message}";
                return RedirectToAction(nameof(Generate));
            }
        }

        // GET: Rapports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _rapportService.GetRapportById(id.Value);
            if (rapport == null)
            {
                return NotFound();
            }

            // Récupérer les détails supplémentaires
            var mois = DateTime.ParseExact(rapport.Mois, "MMMM", CultureInfo.CurrentCulture).Month;
            var ventesParProduit = await _rapportService.GetVentesParProduit(mois, rapport.Annee);
            var paiementsNonPayes = await _rapportService.GetPaiementsNonPayes();

            ViewBag.VentesParProduit = ventesParProduit;
            ViewBag.PaiementsNonPayes = paiementsNonPayes;

            return View(rapport);
        }

        // GET: Rapports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rapport = await _context.RapportsFinanciers
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rapport == null)
            {
                return NotFound();
            }

            return View(rapport);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rapport = await _context.RapportsFinanciers.FindAsync(id);
            if (rapport != null)
            {
                _context.RapportsFinanciers.Remove(rapport);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Rapport supprimé avec succès !";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Rapports/ExportPDF/5
        public async Task<IActionResult> ExportPDF(int id)
        {
            var rapport = await _rapportService.GetRapportById(id);
            if (rapport == null)
            {
                return NotFound();
            }

            var content = await _rapportService.ExporterRapportPDF(id);
            return File(content, "application/pdf", $"Rapport_{rapport.Mois}_{rapport.Annee}.pdf");
        }

        // GET: Rapports/ExportExcel/5
        public async Task<IActionResult> ExportExcel(int id)
        {
            var rapport = await _rapportService.GetRapportById(id);
            if (rapport == null)
            {
                return NotFound();
            }

            var content = await _rapportService.ExporterRapportExcel(id);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Rapport_{rapport.Mois}_{rapport.Annee}.xlsx");
        }

        // Méthodes helper
        private List<SelectListItem> GetListeMois()
        {
            var mois = new List<SelectListItem>();
            var culture = CultureInfo.CurrentCulture;

            for (int i = 1; i <= 12; i++)
            {
                var nomMois = culture.DateTimeFormat.GetMonthName(i);
                mois.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = nomMois
                });
            }

            return mois;
        }

        private List<SelectListItem> GetListeAnnees()
        {
            var annees = new List<SelectListItem>();
            int anneeActuelle = DateTime.Now.Year;

            for (int i = anneeActuelle - 5; i <= anneeActuelle + 1; i++)
            {
                annees.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }

            return annees;
        }
    }
}