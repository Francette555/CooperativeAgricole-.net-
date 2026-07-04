namespace tedd.ViewModels
{
    public class ProducteurViewModel
    {
        public int Id { get; set; }
        public string NomComplet { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public DateTime DateInscription { get; set; }
        public bool EstActif { get; set; }
        public int NombreRecoltes { get; set; }
        public decimal TotalRecoltes { get; set; }
        public decimal TotalPaiements { get; set; }
        public decimal Solde { get; set; }
    }
}