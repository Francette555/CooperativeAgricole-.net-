using System.ComponentModel.DataAnnotations;

namespace tedd.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Role { get; set; } = "Utilisateur";
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public bool EstActif { get; set; } = true;
    }
}