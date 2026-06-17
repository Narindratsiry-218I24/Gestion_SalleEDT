using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Cours")]
    [ApiController]
    public class CoursController : ControllerBase
    {
        private readonly EMITDbContext db;
        private readonly IPlanningService _planningService;

        public CoursController(EMITDbContext context, IPlanningService planningService)
        {
            db = context;
            _planningService = planningService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetCours()
        {
            try
            {
                var cours = db.Cours.Include(c => c.Matiere).ToList();
                return Ok(cours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message, Inner = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetCours(int id)
        {
            var cours = db.Cours
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Classe)
                .Include(c => c.Salle)
                .FirstOrDefault(c => c.IdCours == id);
            if (cours == null) return NotFound();
            return Ok(cours);
        }

        [HttpGet]
        [Route("MatieresDisponibles")]
        public IActionResult GetMatieresDisponibles()
        {
            try
            {
                var matieres = db.Matieres.ToList();
                return Ok(matieres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet]
        [Route("SallesDisponibles")]
        public IActionResult GetSallesDisponibles([FromQuery] DateTime debut, [FromQuery] DateTime fin)
        {
            try
            {
                var jour = debut.ToString("dddd");
                var heureDebut = debut.TimeOfDay;
                var heureFin = fin.TimeOfDay;

                var sallesOccupeesIds = db.Creneaux
                    .Include(c => c.Cours)
                    .Where(c => c.JourSemaine == jour && 
                               ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin) ||
                                (heureFin > c.HeureDebut && heureFin <= c.HeureFin) ||
                                (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)))
                    .Where(c => c.Cours.IdSalle != null)
                    .Select(c => c.Cours.IdSalle)
                    .ToList();

                var sallesDispo = db.Salles
                    .Where(s => !sallesOccupeesIds.Contains(s.IdSalle))
                    .ToList();

                return Ok(sallesDispo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost]
        [Route("PlanifierAuto")]
        public async Task<IActionResult> PlanifierAuto([FromBody] Cours cours, [FromQuery] DateTime debut, [FromQuery] DateTime fin)
        {
            try
            {
                var result = await _planningService.PlanifierCoursAsync(cours, debut, fin);
                return Ok(result);
            }
            catch (PlanningException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet]
        [Route("ExportPdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var emplois = await _planningService.ObtenirEmploisDuTempsAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
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
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Jour").Bold();
                            header.Cell().Text("Heure").Bold();
                            header.Cell().Text("Matière").Bold();
                            header.Cell().Text("Professeur").Bold();
                            header.Cell().Text("Salle").Bold();
                        });

                        foreach (var c in emplois)
                        {
                            table.Cell().Text(c.JourSemaine);
                            table.Cell().Text($"{c.HeureDebut:hh\\:mm} - {c.HeureFin:hh\\:mm}");
                            table.Cell().Text(c.Cours?.Matiere?.NomMatiere ?? "N/A");
                            table.Cell().Text(c.Cours?.Professeur?.Nom ?? "N/A");
                            table.Cell().Text(c.Cours?.Salle?.NomSalle ?? "N/A");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" sur ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "EmploiDuTemps.pdf");
        }


        [HttpPost]
        [Route("VerifierConflit")]
        public IActionResult VerifierConflit([FromBody] Cours cours)
        {
            if (cours.Creneaux == null || !cours.Creneaux.Any())
                return Ok(new { HasConflict = false });

            foreach (var creneau in cours.Creneaux)
            {
                // Conflit de salle
                if (cours.IdSalle != null && cours.IdSalle > 0)
                {
                    var cSalle = db.Creneaux.Include(c => c.Cours.Matiere).FirstOrDefault(c =>
                        c.Cours.IdSalle == cours.IdSalle &&
                        c.JourSemaine == creneau.JourSemaine &&
                        c.IdCours != cours.IdCours &&
                        ((creneau.HeureDebut >= c.HeureDebut && creneau.HeureDebut < c.HeureFin) ||
                         (creneau.HeureFin > c.HeureDebut && creneau.HeureFin <= c.HeureFin) ||
                         (creneau.HeureDebut <= c.HeureDebut && creneau.HeureFin >= c.HeureFin))
                    );
                    if (cSalle != null)
                        return Ok(new { HasConflict = true, Message = $"La salle est occupée par {cSalle.Cours.Matiere.NomMatiere}." });
                }

                // Conflit de professeur
                var cProf = db.Creneaux.Include(c => c.Cours.Matiere).FirstOrDefault(c =>
                    c.Cours.IdProfesseur == cours.IdProfesseur &&
                    c.JourSemaine == creneau.JourSemaine &&
                    c.IdCours != cours.IdCours &&
                    ((creneau.HeureDebut >= c.HeureDebut && creneau.HeureDebut < c.HeureFin) ||
                     (creneau.HeureFin > c.HeureDebut && creneau.HeureFin <= c.HeureFin) ||
                     (creneau.HeureDebut <= c.HeureDebut && creneau.HeureFin >= c.HeureFin))
                );
                if (cProf != null)
                    return Ok(new { HasConflict = true, Message = $"Le professeur enseigne déjà {cProf.Cours.Matiere.NomMatiere}." });
                    
                // Conflit de classe
                var cClasse = db.Creneaux.Include(c => c.Cours.Matiere).FirstOrDefault(c =>
                    c.Cours.IdClasse == cours.IdClasse &&
                    c.JourSemaine == creneau.JourSemaine &&
                    c.IdCours != cours.IdCours &&
                    ((creneau.HeureDebut >= c.HeureDebut && creneau.HeureDebut < c.HeureFin) ||
                     (creneau.HeureFin > c.HeureDebut && creneau.HeureFin <= c.HeureFin) ||
                     (creneau.HeureDebut <= c.HeureDebut && creneau.HeureFin >= c.HeureFin))
                );
                if (cClasse != null)
                    return Ok(new { HasConflict = true, Message = $"La classe a déjà le cours {cClasse.Cours.Matiere.NomMatiere}." });
            }

            return Ok(new { HasConflict = false });
        }

        [HttpPost]
        [Route("{id:int}/Valider")]
        public IActionResult ValiderCours(int id)
        {
            var cours = db.Cours.Find(id);
            if (cours == null) return NotFound();
            cours.Statut = "validé";
            db.SaveChanges();
            return Ok(cours);
        }

        [HttpPost]
        [Route("{id:int}/Annuler")]
        public IActionResult AnnulerCours(int id)
        {
            var cours = db.Cours.Find(id);
            if (cours == null) return NotFound();
            cours.Statut = "annulé";
            db.SaveChanges();
            return Ok(cours);
        }
    }
}