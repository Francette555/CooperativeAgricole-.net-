using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using tedd.Models;

namespace tedd.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producteur> Producteurs { get; set; }
        public DbSet<TypeProduit> TypesProduits { get; set; }
        public DbSet<Recolte> Recoltes { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Vente> Ventes { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<RapportFinancier> RapportsFinanciers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations
            modelBuilder.Entity<Recolte>()
                .HasOne(r => r.Producteur)
                .WithMany(p => p.Recoltes)
                .HasForeignKey(r => r.ProducteurId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Recolte>()
                .HasOne(r => r.TypeProduit)
                .WithMany(t => t.Recoltes)
                .HasForeignKey(r => r.TypeProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.TypeProduit)
                .WithMany(t => t.Stocks)
                .HasForeignKey(s => s.TypeProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vente>()
                .HasOne(v => v.TypeProduit)
                .WithMany(t => t.Ventes)
                .HasForeignKey(v => v.TypeProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Producteur)
                .WithMany(pr => pr.Paiements)
                .HasForeignKey(p => p.ProducteurId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}