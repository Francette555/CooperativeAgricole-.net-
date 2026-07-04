/*using tedd.Models;

namespace tedd.Services
{
    public interface IRapportService
    {
        Task<RapportFinancier> GenererRapportMensuel(int mois, int annee);
        Task<decimal> CalculerBeneficeMensuel(int mois, int annee);
        Task<Dictionary<string, decimal>> GetVentesParProduit(int mois, int annee);
        Task<List<Paiement>> GetPaiementsNonPayes();
        Task<decimal> GetSoldeProducteur(int producteurId);
    }
}

/*using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;

namespace tedd.Services
{
    public class RapportService : IRapportService
    {
        private readonly ApplicationDbContext _context;

        public RapportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RapportFinancier> GenererRapportMensuel(int mois, int annee)
        {
            var rapport = new RapportFinancier
            {
                Mois = new DateTime(annee, mois, 1).ToString("MMMM"),
                Annee = annee,
                TotalVentes = await _context.Ventes
                    .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                    .SumAsync(v => v.Quantite * v.PrixUnitaire) ?? 0,
                TotalPaiements = await _context.Paiements
                    .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                    .SumAsync(p => p.Montant) ?? 0,
                DateGeneration = DateTime.Now
            };

            rapport.Benefice = rapport.TotalVentes - rapport.TotalPaiements;

            _context.RapportsFinanciers.Add(rapport);
            await _context.SaveChangesAsync();

            return rapport;
        }

        public async Task<decimal> CalculerBeneficeMensuel(int mois, int annee)
        {
            var totalVentes = await _context.Ventes
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .SumAsync(v => v.Quantite * v.PrixUnitaire) ?? 0;

            var totalPaiements = await _context.Paiements
                .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                .SumAsync(p => p.Montant) ?? 0;

            return totalVentes - totalPaiements;
        }

        public async Task<Dictionary<string, decimal>> GetVentesParProduit(int mois, int annee)
        {
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .GroupBy(v => v.TypeProduit.Nom)
                .Select(g => new
                {
                    Produit = g.Key,
                    Total = g.Sum(v => v.Quantite * v.PrixUnitaire)
                })
                .ToDictionaryAsync(k => k.Produit, v => v.Total);

            return ventes ?? new Dictionary<string, decimal>();
        }

        public async Task<List<Paiement>> GetPaiementsNonPayes()
        {
            return await _context.Paiements
                .Include(p => p.Producteur)
                .Where(p => p.Statut == "En attente")
                .OrderBy(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<decimal> GetSoldeProducteur(int producteurId)
        {
            var totalRecoltes = await _context.Recoltes
                .Where(r => r.ProducteurId == producteurId)
                .SumAsync(r => r.Quantite);

            var totalPaiements = await _context.Paiements
                .Where(p => p.ProducteurId == producteurId && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            // Prix moyen estimé par kg (à ajuster selon votre logique métier)
            decimal prixMoyen = 1000; // Exemple: 1000 FCFA/kg

            return (totalRecoltes * prixMoyen) - totalPaiements;
        }
    }
}*/

// Services/IRapportService.cs
/*using tedd.Models;

namespace tedd.Services
{
    public interface IRapportService
    {
        Task<RapportFinancier> GenererRapportMensuel(int mois, int annee);
        Task<decimal> CalculerBeneficeMensuel(int mois, int annee);
        Task<Dictionary<string, decimal>> GetVentesParProduit(int mois, int annee);
        Task<List<Paiement>> GetPaiementsNonPayes();
        Task<decimal> GetSoldeProducteur(int producteurId);
        Task<List<RapportFinancier>> GetRapportsHistorique();
        Task<RapportFinancier> GetRapportById(int id);
        Task<byte[]> ExporterRapportPDF(int id);
        Task<byte[]> ExporterRapportExcel(int id);
    }
}*/

// Services/RapportService.cs
using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;
using System.Globalization;

namespace tedd.Services
{
    public class RapportService : IRapportService
    {
        private readonly ApplicationDbContext _context;

        public RapportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RapportFinancier> GenererRapportMensuel(int mois, int annee)
        {
            // Récupérer les données
            var totalVentes = await _context.Ventes
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .SumAsync(v => v.Quantite * v.PrixUnitaire);

            var totalPaiements = await _context.Paiements
                .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            var rapport = new RapportFinancier
            {
                Mois = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mois),
                Annee = annee,
                TotalVentes = totalVentes,
                TotalPaiements = totalPaiements,
                Benefice = totalVentes - totalPaiements,
                DateGeneration = DateTime.Now
            };

            // Enregistrer le rapport
            _context.RapportsFinanciers.Add(rapport);
            await _context.SaveChangesAsync();

            return rapport;
        }

        public async Task<decimal> CalculerBeneficeMensuel(int mois, int annee)
        {
            var totalVentes = await _context.Ventes
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .SumAsync(v => v.Quantite * v.PrixUnitaire);

            var totalPaiements = await _context.Paiements
                .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            return totalVentes - totalPaiements;
        }

        public async Task<Dictionary<string, decimal>> GetVentesParProduit(int mois, int annee)
        {
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .GroupBy(v => v.TypeProduit.Nom)
                .Select(g => new
                {
                    Produit = g.Key,
                    Total = g.Sum(v => v.Quantite * v.PrixUnitaire)
                })
                .ToDictionaryAsync(k => k.Produit, v => v.Total);

            return ventes ?? new Dictionary<string, decimal>();
        }

        public async Task<List<Paiement>> GetPaiementsNonPayes()
        {
            return await _context.Paiements
                .Include(p => p.Producteur)
                .Where(p => p.Statut == "En attente")
                .OrderBy(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<decimal> GetSoldeProducteur(int producteurId)
        {
            var totalRecoltes = await _context.Recoltes
                .Where(r => r.ProducteurId == producteurId)
                .SumAsync(r => r.Quantite);

            var totalPaiements = await _context.Paiements
                .Where(p => p.ProducteurId == producteurId && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            // Prix moyen estimé par kg (à ajuster)
            decimal prixMoyen = 1500; // 1500 Ar/kg
            return (totalRecoltes * prixMoyen) - totalPaiements;
        }

        public async Task<List<RapportFinancier>> GetRapportsHistorique()
        {
            return await _context.RapportsFinanciers
                .OrderByDescending(r => r.Annee)
                .ThenByDescending(r => r.Mois)
                .ToListAsync();
        }

        public async Task<RapportFinancier> GetRapportById(int id)
        {
            return await _context.RapportsFinanciers
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<byte[]> ExporterRapportPDF(int id)
        {
            var rapport = await GetRapportById(id);
            if (rapport == null) return null;

            // Simuler une exportation PDF
            // Vous pouvez utiliser iTextSharp ou autres librairies
            var content = $"Rapport Financier\n" +
                         $"==================\n\n" +
                         $"Période: {rapport.Mois} {rapport.Annee}\n" +
                         $"Total Ventes: {rapport.TotalVentes:N0} Ar\n" +
                         $"Total Paiements: {rapport.TotalPaiements:N0} Ar\n" +
                         $"Bénéfice: {rapport.Benefice:N0} Ar\n" +
                         $"Date de génération: {rapport.DateGeneration:dd/MM/yyyy HH:mm}\n";

            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        public async Task<byte[]> ExporterRapportExcel(int id)
        {
            var rapport = await GetRapportById(id);
            if (rapport == null) return null;

            // Simuler une exportation Excel
            // Vous pouvez utiliser EPPlus ou autres librairies
            var content = $"Mois\tAnnée\tTotal Ventes\tTotal Paiements\tBénéfice\n" +
                         $"{rapport.Mois}\t{rapport.Annee}\t{rapport.TotalVentes}\t{rapport.TotalPaiements}\t{rapport.Benefice}\n";

            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}