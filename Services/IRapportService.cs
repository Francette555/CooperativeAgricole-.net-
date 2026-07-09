using tedd.Models;
using tedd.ViewModels;

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
        Task<DashboardViewModel> GetDashboardData();
    }
}