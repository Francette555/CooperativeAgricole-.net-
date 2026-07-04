using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tedd.Models
{
    public class Paiement
    {
        public int Id { get; set; }

        [Required]
        public int ProducteurId { get; set; }

        [Required(ErrorMessage = "Le montant est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Montant { get; set; }

        [Display(Name = "Date de paiement")]
        public DateTime DatePaiement { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Mois { get; set; }

        public int Annee { get; set; }

        [StringLength(20)]
        public string Statut { get; set; } = "En attente";

        // Propriétés de navigation
        [ForeignKey("ProducteurId")]
        public virtual Producteur Producteur { get; set; }
    }
}