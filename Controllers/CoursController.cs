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
                var courses = db.Cours.Include(c => c.Matiere).Include(c => c.Seances).ToList();
                return Ok(courses);
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
            var course = db.Cours
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Classe)
                .Include(c => c.Seances)
                .FirstOrDefault(c => c.IdCours == id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateCourse([FromBody] Cours course)
        {
            if (course == null) return BadRequest("Course is null");
            
            ModelState.Remove("Matiere");
            ModelState.Remove("Professeur");
            ModelState.Remove("Classe");
            ModelState.Remove("Salle");
            ModelState.Remove("Semestre");
            ModelState.Remove("Creneaux");
            ModelState.Remove("DemandesEdt");
            ModelState.Remove("Seances");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Erreur de validation: " + string.Join(", ", errors) });
            }

            var result = await _planningService.PlanifierCourseAsync(course);
            return Ok(result);
        }

        [HttpPost]
        [Route("{id:int}/Status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            var course = db.Cours.Find(id);
            if (course == null) return NotFound();
            if (Enum.TryParse<CourseStatus>(newStatus, true, out var status))
            {
                course.Statut = status.ToString();
                db.SaveChanges();
                return Ok(course);
            }
            return BadRequest("Invalid status");
        }

        [HttpPost]
        [Route("{id:int}/Seance")]
        public async Task<IActionResult> ScheduleSeance(int id, [FromBody] SeanceRequest request)
        {
            try
            {
                var seance = await _planningService.PlanifierSeanceAsync(id, request.Date, request.StartTime, request.EndTime, request.SalleId, request.GroupeId);
                return Ok(seance);
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

        public class SeanceRequest
        {
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int? SalleId { get; set; }
            public int? GroupeId { get; set; }
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
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Heure").Bold();
                            header.Cell().Text("Cours").Bold();
                            header.Cell().Text("Professeur").Bold();
                            header.Cell().Text("Salle").Bold();
                        });

                        foreach (var s in emplois)
                        {
                            table.Cell().Text(s.Date.ToShortDateString());
                            table.Cell().Text($"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}");
                            table.Cell().Text(s.Cours?.Matiere?.NomMatiere ?? "N/A");
                            table.Cell().Text(s.Cours?.Professeur?.Nom ?? "N/A");
                            table.Cell().Text(s.Salle?.NomSalle ?? "N/A");
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
    }
}