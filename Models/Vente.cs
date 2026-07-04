using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class Vente
    {
        public int Id { get; set; }

        [Required]
        public int TypeProduitId { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantite { get; set; }

        [Required(ErrorMessage = "Le prix unitaire est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Prix unitaire")]
        public decimal PrixUnitaire { get; set; }

        [Display(Name = "Date de vente")]
        public DateTime DateVente { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string Client { get; set; }

        [Display(Name = "Montant total")]
        [NotMapped]
        public decimal MontantTotal => Quantite * PrixUnitaire;

        // Propriétés de navigation
        [ForeignKey("TypeProduitId")]
        public virtual TypeProduit TypeProduit { get; set; }
    }
}