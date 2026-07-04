using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class Stock
    {
        public int Id { get; set; }

        [Required]
        public int TypeProduitId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "La quantité ne peut pas être négative")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantite { get; set; } = 0;

        [Display(Name = "Seuil d'alerte")]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SeuilAlerte { get; set; } = 100;

        [Display(Name = "Date de mise à jour")]
        public DateTime DateMiseAJour { get; set; } = DateTime.Now;

        // Propriétés de navigation
        [ForeignKey("TypeProduitId")]
        public virtual TypeProduit TypeProduit { get; set; }
    }
}