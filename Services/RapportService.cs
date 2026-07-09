using Microsoft.EntityFrameworkCore;
using tedd.Data;
using tedd.Models;
using tedd.ViewModels;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace tedd.Services
{
    public class RapportService : IRapportService
    {
        private readonly ApplicationDbContext _context;

        public RapportService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<DashboardViewModel> GetDashboardData()
        {
            // ===== DONNÉES RÉELLES DE LA BASE =====

            // 1. Total des ventes (toutes périodes confondues)
            var totalVentes = await _context.Ventes
                .SumAsync(v => v.Quantite * v.PrixUnitaire);

            // 2. Total des paiements effectués
            var totalPaiements = await _context.Paiements
                .Where(p => p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            // 3. Nombre de producteurs actifs
            var nbProducteurs = await _context.Producteurs
                .Where(p => p.EstActif)
                .CountAsync();

            // 4. Nombre total de récoltes
            var nbRecoltes = await _context.Recoltes.CountAsync();

            // 5. Quantité totale récoltée
            var totalRecoltes = await _context.Recoltes
                .SumAsync(r => r.Quantite);

            // 6. État des stocks (avec données réelles)
            var produitsEnStock = await _context.Stocks
                .Include(s => s.TypeProduit)
                .Where(s => s.Quantite > 0)
                .Select(s => new ProduitStock
                {
                    NomProduit = s.TypeProduit.Nom,
                    Quantite = s.Quantite,
                    Unite = s.TypeProduit.Unite,
                    EstEnAlerte = s.Quantite < s.SeuilAlerte
                })
                .ToListAsync();

            // 7. Activités récentes
            var activitesRecentes = await GetRecentActivitiesAsync();

            return new DashboardViewModel
            {
                NombreProducteurs = nbProducteurs,
                NombreRecoltes = nbRecoltes,
                TotalRecoltes = totalRecoltes,
                TotalVentes = totalVentes,
                TotalPaiements = totalPaiements,
                Benefice = totalVentes - totalPaiements,
                ProduitsEnStock = produitsEnStock,
                ActivitesRecentes = activitesRecentes
            };
        }

        private async Task<List<RecenteActivite>> GetRecentActivitiesAsync()
        {
            var activites = new List<RecenteActivite>();

            // ===== DERNIÈRES RÉCOLTES =====
            var recoltes = await _context.Recoltes
                .Include(r => r.Producteur)
                .Include(r => r.TypeProduit)
                .OrderByDescending(r => r.DateRecolte)
                .Take(5)
                .Select(r => new RecenteActivite
                {
                    Description = $"{r.Producteur.NomComplet} a apporté {r.Quantite:F2} kg de {r.TypeProduit.Nom}",
                    Date = r.DateRecolte,
                    Type = "Récolte",
                    Icone = "fa-tractor",
                    Couleur = "success"
                })
                .ToListAsync();

            // ===== DERNIÈRES VENTES =====
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .OrderByDescending(v => v.DateVente)
                .Take(5)
                .Select(v => new RecenteActivite
                {
                    Description = $"Vente de {v.Quantite:F2} kg de {v.TypeProduit.Nom} à {v.Client ?? "client"} - {(v.Quantite * v.PrixUnitaire):N0} Ar",
                    Date = v.DateVente,
                    Type = "Vente",
                    Icone = "fa-shopping-cart",
                    Couleur = "info"
                })
                .ToListAsync();

            // ===== DERNIERS PAIEMENTS =====
            var paiements = await _context.Paiements
                .Include(p => p.Producteur)
                .OrderByDescending(p => p.DatePaiement)
                .Take(5)
                .Select(p => new RecenteActivite
                {
                    Description = $"Paiement de {p.Montant:N0} Ar à {p.Producteur.NomComplet} ({p.Statut})",
                    Date = p.DatePaiement,
                    Type = "Paiement",
                    Icone = "fa-money-bill-wave",
                    Couleur = p.Statut == "Payé" ? "success" : "warning"
                })
                .ToListAsync();

            // ===== DERNIERS RAPPORTS =====
            var rapports = await _context.RapportsFinanciers
                .OrderByDescending(r => r.DateGeneration)
                .Take(3)
                .Select(r => new RecenteActivite
                {
                    Description = $"Rapport généré pour {r.Mois} {r.Annee} (Bénéfice: {r.Benefice:N0} Ar)",
                    Date = r.DateGeneration,
                    Type = "Rapport",
                    Icone = "fa-file-alt",
                    Couleur = "primary"
                })
                .ToListAsync();

            activites.AddRange(recoltes);
            activites.AddRange(ventes);
            activites.AddRange(paiements);
            activites.AddRange(rapports);

            return activites.OrderByDescending(a => a.Date).Take(15).ToList();
        }

        public async Task<RapportFinancier> GenererRapportMensuel(int mois, int annee)
        {
            // ===== DONNÉES RÉELLES POUR UN MOIS SPÉCIFIQUE =====

            // 1. Total des ventes pour le mois
            var totalVentes = await _context.Ventes
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .SumAsync(v => v.Quantite * v.PrixUnitaire);

            // 2. Total des paiements pour le mois
            var totalPaiements = await _context.Paiements
                .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            // 3. Bénéfice = Ventes - Paiements
            var benefice = totalVentes - totalPaiements;

            // 4. Créer et enregistrer le rapport
            var rapport = new RapportFinancier
            {
                Mois = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mois),
                Annee = annee,
                TotalVentes = totalVentes,
                TotalPaiements = totalPaiements,
                Benefice = benefice,
                DateGeneration = DateTime.Now
            };

            _context.RapportsFinanciers.Add(rapport);
            await _context.SaveChangesAsync();

            return rapport;
        }

        public async Task<decimal> CalculerBeneficeMensuel(int mois, int annee)
        {
            var totalVentes = await _context.Ventes
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .SumAsync(v => v.Quantite * v.PrixUnitaire);

            var totalPaiements = await _context.Paiements
                .Where(p => p.DatePaiement.Month == mois && p.DatePaiement.Year == annee && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            return totalVentes - totalPaiements;
        }

        public async Task<Dictionary<string, decimal>> GetVentesParProduit(int mois, int annee)
        {
            // ===== VENTES PAR PRODUIT (DONNÉES RÉELLES) =====
            var ventes = await _context.Ventes
                .Include(v => v.TypeProduit)
                .Where(v => v.DateVente.Month == mois && v.DateVente.Year == annee)
                .GroupBy(v => v.TypeProduit.Nom)
                .Select(g => new
                {
                    Produit = g.Key,
                    Total = g.Sum(v => v.Quantite * v.PrixUnitaire)
                })
                .ToDictionaryAsync(k => k.Produit, v => v.Total);

            return ventes ?? new Dictionary<string, decimal>();
        }

        public async Task<List<Paiement>> GetPaiementsNonPayes()
        {
            // ===== PAIEMENTS EN ATTENTE (DONNÉES RÉELLES) =====
            return await _context.Paiements
                .Include(p => p.Producteur)
                .Where(p => p.Statut == "En attente")
                .OrderBy(p => p.DatePaiement)
                .ToListAsync();
        }

        public async Task<decimal> GetSoldeProducteur(int producteurId)
        {
            var totalRecoltes = await _context.Recoltes
                .Where(r => r.ProducteurId == producteurId)
                .SumAsync(r => r.Quantite);

            var totalPaiements = await _context.Paiements
                .Where(p => p.ProducteurId == producteurId && p.Statut == "Payé")
                .SumAsync(p => p.Montant);

            decimal prixMoyen = 1500;
            return (totalRecoltes * prixMoyen) - totalPaiements;
        }

        public async Task<List<RapportFinancier>> GetRapportsHistorique()
        {
            return await _context.RapportsFinanciers
                .OrderByDescending(r => r.Annee)
                .ThenByDescending(r => r.Mois)
                .ToListAsync();
        }

        public async Task<RapportFinancier> GetRapportById(int id)
        {
            return await _context.RapportsFinanciers
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<byte[]> ExporterRapportPDF(int id)
        {
            var rapport = await GetRapportById(id);
            if (rapport == null) return null;

            var mois = DateTime.ParseExact(rapport.Mois, "MMMM", CultureInfo.CurrentCulture).Month;

            // ===== RÉCUPÉRER LES DONNÉES RÉELLES POUR LE PDF =====
            var ventesParProduit = await GetVentesParProduit(mois, rapport.Annee);
            var paiementsNonPayes = await GetPaiementsNonPayes();
            var nbProducteurs = await _context.Producteurs.Where(p => p.EstActif).CountAsync();
            var nbRecoltes = await _context.Recoltes.CountAsync();

            // Créer le document PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // ===== EN-TÊTE =====
                    page.Header()
                        .AlignCenter()
                        .Column(col =>
                        {
                            col.Item().Text("RAPPORT FINANCIER")
                                .SemiBold()
                                .FontSize(24)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item().Text($"Période : {rapport.Mois} {rapport.Annee}")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken1);

                            col.Item().PaddingVertical(10).LineHorizontal(1);
                        });

                    // ===== CONTENU =====
                    page.Content()
                        .PaddingVertical(20)
                        .Column(col =>
                        {
                            // ---- STATISTIQUES GÉNÉRALES ----
                            col.Item().Text("STATISTIQUES GÉNÉRALES")
                                .SemiBold()
                                .FontSize(16)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().Text("Total Ventes :").Bold();
                                table.Cell().AlignRight().Text($"{rapport.TotalVentes:N0} Ar").FontColor(Colors.Green.Darken2);

                                table.Cell().Text("Total Paiements :").Bold();
                                table.Cell().AlignRight().Text($"{rapport.TotalPaiements:N0} Ar").FontColor(Colors.Red.Darken2);

                                table.Cell().Text("Bénéfice :").Bold();
                                var beneficeColor = rapport.Benefice >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                                table.Cell().AlignRight().Text($"{rapport.Benefice:N0} Ar").Bold().FontColor(beneficeColor);

                                table.Cell().Text("Producteurs actifs :").Bold();
                                table.Cell().AlignRight().Text($"{nbProducteurs}").FontColor(Colors.Blue.Darken1);

                                table.Cell().Text("Nombre de récoltes :").Bold();
                                table.Cell().AlignRight().Text($"{nbRecoltes}").FontColor(Colors.Blue.Darken1);
                            });

                            // ---- VENTES PAR PRODUIT ----
                            if (ventesParProduit.Any())
                            {
                                col.Item().PaddingTop(20).LineHorizontal(0.5f);

                                col.Item().PaddingTop(10).Text("VENTES PAR PRODUIT")
                                    .SemiBold()
                                    .FontSize(16)
                                    .FontColor(Colors.Blue.Darken2);

                                var totalVentes = ventesParProduit.Values.Sum();

                                col.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Text("Produit").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text("Montant").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text("%").Bold();
                                    });

                                    foreach (var item in ventesParProduit)
                                    {
                                        var pourcentage = totalVentes > 0 ? (item.Value / totalVentes * 100) : 0;
                                        table.Cell().Text(item.Key);
                                        table.Cell().AlignRight().Text($"{item.Value:N0} Ar");
                                        table.Cell().AlignRight().Text($"{pourcentage:F1}%");
                                    }

                                    table.Footer(footer =>
                                    {
                                        footer.Cell().Text("TOTAL").Bold();
                                        footer.Cell().AlignRight().Text($"{totalVentes:N0} Ar").Bold();
                                        footer.Cell().AlignRight().Text("100%").Bold();
                                    });
                                });
                            }

                            // ---- PAIEMENTS EN ATTENTE ----
                            if (paiementsNonPayes.Any())
                            {
                                col.Item().PaddingTop(20).LineHorizontal(0.5f);

                                col.Item().PaddingTop(10).Text("PAIEMENTS EN ATTENTE")
                                    .SemiBold()
                                    .FontSize(16)
                                    .FontColor(Colors.Orange.Darken2);

                                col.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Text("Producteur").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text("Montant").Bold();
                                    });

                                    foreach (var item in paiementsNonPayes.Take(10))
                                    {
                                        table.Cell().Text(item.Producteur?.NomComplet ?? "Inconnu");
                                        table.Cell().AlignRight().Text($"{item.Montant:N0} Ar").FontColor(Colors.Orange.Darken2);
                                    }

                                    if (paiementsNonPayes.Count > 10)
                                    {
                                        table.Cell().Text($"... et {paiementsNonPayes.Count - 10} autres").Italic();
                                        table.Cell().Text("");
                                    }

                                    table.Footer(footer =>
                                    {
                                        footer.Cell().Text("TOTAL EN ATTENTE").Bold();
                                        footer.Cell().AlignRight().Text($"{paiementsNonPayes.Sum(p => p.Montant):N0} Ar")
                                            .Bold()
                                            .FontColor(Colors.Orange.Darken2);
                                    });
                                });
                            }
                        });

                    // ===== PIED DE PAGE =====
                    page.Footer()
                        .AlignCenter()
                        .Text($"Généré le {rapport.DateGeneration:dd/MM/yyyy à HH:mm} - Coopérative Agricole")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}