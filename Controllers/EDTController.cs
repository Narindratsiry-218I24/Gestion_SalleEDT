using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/EDT")]
    [ApiController]
    public class EDTController : ControllerBase
    {
        private readonly EMITDbContext db;

        public EDTController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetHebdomadaire(
            int? classeId = null, int? profId = null, int? salleId = null,
            string? cycle = null, string? niveau = null, string? semaineType = null)
        {
            var query = db.Seances
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe).ThenInclude(cl => cl.Filiere).ThenInclude(f => f.Mention)
                .Include(s => s.Cours).ThenInclude(c => c.Classe).ThenInclude(cl => cl.Niveau)
                .Include(s => s.Salle)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(s => s.Cours.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(s => s.Cours.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(s => s.SalleId == salleId);

            if (!string.IsNullOrEmpty(cycle) && cycle != "all")
                query = query.Where(s => s.Cours.Classe.Filiere.NomFiliere.Contains(cycle));

            if (!string.IsNullOrEmpty(niveau) && niveau != "all")
                query = query.Where(s => s.Cours.Classe.Niveau.CodeNiveau.Contains(niveau));

            var seances = query.ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                    Classe = s.Cours.Classe != null ? new { 
                        s.Cours.Classe.IdClasse, 
                        s.Cours.Classe.NomClasse,
                        Filiere = s.Cours.Classe.Filiere?.NomFiliere,
                        Mention = s.Cours.Classe.Filiere?.Mention?.NomMention,
                        Niveau = s.Cours.Classe.Niveau?.CodeNiveau
                    } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle, s.Salle.Capacite } : null
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("ExportExcel")]
        public IActionResult ExportExcel(
            int? classeId = null, int? profId = null, int? salleId = null,
            string? cycle = null, string? niveau = null, string? semaineType = null)
        {
            var query = db.Seances
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .Include(s => s.Salle)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(s => s.Cours.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(s => s.Cours.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(s => s.SalleId == salleId);

            var seances = query.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Date;Heure de Debut;Heure de Fin;Matiere;Type de Cours;Professeur;Classe;Salle");
            
            foreach (var s in seances)
            {
                var prof = s.Cours?.Professeur != null ? $"{s.Cours.Professeur.Nom} {s.Cours.Professeur.Prenom}" : "-";
                var matiere = s.Cours?.Matiere?.NomMatiere ?? "-";
                var classe = s.Cours?.Classe?.NomClasse ?? "-";
                var salle = s.Salle?.NomSalle ?? "-";
                var typeCours = s.Cours?.TypeCours ?? "-";

                sb.AppendLine($"{s.Date:yyyy-MM-dd};{s.StartTime};{s.EndTime};\"{matiere}\";\"{typeCours}\";\"{prof}\";\"{classe}\";\"{salle}\"");
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var bytes = bom.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return File(bytes, "text/csv", "EmploiDuTemps.csv");
        }

        [HttpGet]
        [Route("ExportPdf")]
        public IActionResult ExportPdf(
            int? classeId = null, int? profId = null, int? salleId = null)
        {
            var query = db.Seances
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .Include(s => s.Salle)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(s => s.Cours.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(s => s.Cours.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(s => s.SalleId == salleId);

            var seances = query.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Emploi du Temps").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Début").Bold();
                            header.Cell().Text("Fin").Bold();
                            header.Cell().Text("Matière").Bold();
                            header.Cell().Text("Professeur").Bold();
                            header.Cell().Text("Classe").Bold();
                            header.Cell().Text("Salle").Bold();
                            
                            header.Cell().ColumnSpan(7).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var s in seances)
                        {
                            table.Cell().Text(s.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(s.StartTime.ToString(@"hh\:mm"));
                            table.Cell().Text(s.EndTime.ToString(@"hh\:mm"));
                            table.Cell().Text(s.Cours?.Matiere?.NomMatiere ?? "-");
                            table.Cell().Text(s.Cours?.Professeur != null ? $"{s.Cours.Professeur.Nom} {s.Cours.Professeur.Prenom}" : "-");
                            table.Cell().Text(s.Cours?.Classe?.NomClasse ?? "-");
                            table.Cell().Text(s.Salle?.NomSalle ?? "-");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "EmploiDuTemps.pdf");
        }

        [HttpGet]
        [Route("ParSalle/{id:int}")]
        public IActionResult GetEDTParSalle(int id)
        {
            var seances = db.Seances
                .Where(s => s.SalleId == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                    Classe = s.Cours.Classe != null ? new { s.Cours.Classe.IdClasse, s.Cours.Classe.NomClasse } : null,
                }
            });
            return Ok(result);
        }

        [HttpGet]
        [Route("ParClasse/{id:int}")]
        public IActionResult GetEDTParClasse(int id)
        {
            var seances = db.Seances
                .Where(s => s.Cours.IdClasse == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Salle)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle } : null
            });
            return Ok(result);
        }

        [HttpGet]
        [Route("ParProfesseur/{id:int}")]
        public IActionResult GetEDTParProfesseur(int id)
        {
            var seances = db.Seances
                .Where(s => s.Cours.IdProfesseur == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .Include(s => s.Salle)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Classe = s.Cours.Classe != null ? new { s.Cours.Classe.IdClasse, s.Cours.Classe.NomClasse } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle } : null
            });
            return Ok(result);
        }
    }
}
