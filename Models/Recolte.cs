using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class Recolte
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le producteur est obligatoire")]
        public int ProducteurId { get; set; }

        [Required(ErrorMessage = "Le type de produit est obligatoire")]
        public int TypeProduitId { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantite { get; set; }

        [Display(Name = "Date de récolte")]
        public DateTime DateRecolte { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Qualite { get; set; }

        // Propriétés de navigation
        [ForeignKey("ProducteurId")]
        public virtual Producteur Producteur { get; set; }

        [ForeignKey("TypeProduitId")]
        public virtual TypeProduit TypeProduit { get; set; }
    }
}