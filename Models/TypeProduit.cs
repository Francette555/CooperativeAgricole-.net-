using System.Collections;

using System.ComponentModel.DataAnnotations;

namespace tedd.Models
{
    public class TypeProduit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du produit est obligatoire")]
        [StringLength(50)]
        public string Nom { get; set; }

        public string Description { get; set; }

        [StringLength(20)]
        public string Unite { get; set; } = "kg";

        // Propriétés de navigation
        public virtual ICollection<Recolte> Recoltes { get; set; } = new List<Recolte>();
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<Vente> Ventes { get; set; } = new List<Vente>();
    }
}