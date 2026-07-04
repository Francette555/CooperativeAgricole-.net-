using System.ComponentModel.DataAnnotations;

namespace tedd.Models
{
    public class Producteur
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        [StringLength(20)]
        public string Telephone { get; set; }

        [EmailAddress(ErrorMessage = "Email invalide")]
        [StringLength(100)]
        public string Email { get; set; }

        public string Adresse { get; set; }

        [Display(Name = "Date d'inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        [Display(Name = "Actif")]
        public bool EstActif { get; set; } = true;

        // Propriétés de navigation
        public virtual ICollection<Recolte> Recoltes { get; set; } = new List<Recolte>();
        public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();

        [Display(Name = "Nom complet")]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}