namespace tedd.ViewModels
{
    public class DashboardViewModel
    {
        public int NombreProducteurs { get; set; }
        public int NombreRecoltes { get; set; }
        public decimal TotalRecoltes { get; set; }
        public decimal TotalVentes { get; set; }
        public decimal TotalPaiements { get; set; }
        public decimal Benefice { get; set; }
        public List<ProduitStock> ProduitsEnStock { get; set; }
        public List<RecenteActivite> ActivitesRecentes { get; set; }
    }

    public class ProduitStock
    {
        public string NomProduit { get; set; }
        public decimal Quantite { get; set; }
        public string Unite { get; set; }
        public bool EstEnAlerte { get; set; }
    }

    public class RecenteActivite
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}