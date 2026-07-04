using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace tedd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalVentes = await _context.Ventes.SumAsync(v => v.Quantite * v.PrixUnitaire);
            var totalPaiements = await _context.Paiements.SumAsync(p => p.Montant);

            var dashboard = new DashboardViewModel
            {
                NombreProducteurs = await _context.Producteurs.CountAsync(),
                NombreRecoltes = await _context.Recoltes.CountAsync(),
                TotalRecoltes = await _context.Recoltes.SumAsync(r => r.Quantite),
                TotalVentes = totalVentes,
                TotalPaiements = totalPaiements,
                Benefice = totalVentes - totalPaiements,
                ProduitsEnStock = await _context.Stocks
                    .Include(s => s.TypeProduit)
                    .Select(s => new ProduitStock
                    {
                        NomProduit = s.TypeProduit.Nom,
                        Quantite = s.Quantite,
                        Unite = s.TypeProduit.Unite,
                        EstEnAlerte = s.Quantite < s.SeuilAlerte
                    })
                    .ToListAsync(),
                ActivitesRecentes = await GetRecentActivitiesAsync()
            };

            return View(dashboard);
        }

        private async Task<List<RecenteActivite>> GetRecentActivitiesAsync()
        {
            var activites = new List<RecenteActivite>();

            // Récupérer les dernières récoltes
            var recoltes = await _context.Recoltes
                .Include(r => r.Producteur)
                .Include(r => r.TypeProduit)
                .OrderByDescending(r => r.DateRecolte)
                .Take(5)
                .Select(r => new RecenteActivite
                {
                    Description = $"{r.Producteur.NomComplet} a apporté {r.Quantite} {r.TypeProduit.Unite} de {r.TypeProduit.Nom}",
                    Date = r.DateRecolte,
                    Type = "Récolte"
                })
                .ToListAsync();

            // Récupérer les dernières ventes
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .OrderByDescending(v => v.DateVente)
                .Take(5)
                .Select(v => new RecenteActivite
                {
                    Description = $"Vente de {v.Quantite} {v.TypeProduit.Unite} de {v.TypeProduit.Nom} - {v.Quantite * v.PrixUnitaire:C}",
                    Date = v.DateVente,
                    Type = "Vente"
                })
                .ToListAsync();

            // Récupérer les derniers paiements
            var paiements = await _context.Paiements
                .Include(p => p.Producteur)
                .OrderByDescending(p => p.DatePaiement)
                .Take(5)
                .Select(p => new RecenteActivite
                {
                    Description = $"Paiement de {p.Montant:C} à {p.Producteur.NomComplet}",
                    Date = p.DatePaiement,
                    Type = "Paiement"
                })
                .ToListAsync();

            activites.AddRange(recoltes);
            activites.AddRange(ventes);
            activites.AddRange(paiements);

            return activites.OrderByDescending(a => a.Date).Take(10).ToList();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}