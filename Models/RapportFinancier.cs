/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class RapportFinancier
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Mois { get; set; }

        public int Annee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total ventes")]
        public decimal TotalVentes { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total paiements")]
        public decimal TotalPaiements { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Bénéfice")]
        public decimal Benefice { get; set; } = 0;

        [Display(Name = "Date de génération")]
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }
}*/

// Models/RapportFinancier.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class RapportFinancier
    {
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "Mois")]
        public string Mois { get; set; }

        [Display(Name = "Année")]
        public int Annee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Ventes")]
        [DisplayFormat(DataFormatString = "{0:N0} Ar")]
        public decimal TotalVentes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Paiements")]
        [DisplayFormat(DataFormatString = "{0:N0} Ar")]
        public decimal TotalPaiements { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Bénéfice")]
        [DisplayFormat(DataFormatString = "{0:N0} Ar")]
        public decimal Benefice { get; set; }

        [Display(Name = "Date de génération")]
        public DateTime DateGeneration { get; set; } = DateTime.Now;

        [Display(Name = "Période")]
        public string Periode => $"{Mois} {Annee}";
    }
}