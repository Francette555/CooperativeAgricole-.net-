namespace tedd.ViewModels
{
    public class RecenteActivite
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Icone { get; set; } = "fa-circle";
        public string Couleur { get; set; } = "secondary";
    }
}