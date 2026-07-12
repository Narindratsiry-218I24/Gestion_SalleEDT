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
        private readonly IConflitService _conflitService;

        public CoursController(EMITDbContext context, IPlanningService planningService, IConflitService conflitService)
        {
            db = context;
            _planningService = planningService;
            _conflitService = conflitService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetCours(
            [FromQuery] int? anneeId = null,
            [FromQuery] int? filiereId = null,
            [FromQuery] int? parcoursId = null,
            [FromQuery] int? classeId = null,
            [FromQuery] int? matiereId = null,
            [FromQuery] string? statut = null,
            [FromQuery] int? profId = null,
            [FromQuery] string? profEmail = null)
        {
            try
            {
                var courses = db.Cours
                    .Include(c => c.Matiere)
                        .ThenInclude(m => m.Filiere)
                    .Include(c => c.Matiere)
                        .ThenInclude(m => m.RefSemestre)
                            .ThenInclude(r => r.Niveau)
                    .Include(c => c.Professeur)
                    .Include(c => c.Classe)
                    .Include(c => c.Seances)
                    .AsQueryable();

                if (anneeId.HasValue)
                {
                    courses = courses.Where(c => c.Classe != null && c.Classe.IdAnneeAcademique == anneeId.Value);
                }

                if (filiereId.HasValue)
                {
                    courses = courses.Where(c => c.Matiere != null && c.Matiere.IdFiliere == filiereId.Value);
                }

                if (parcoursId.HasValue)
                {
                    courses = courses.Where(c => c.Matiere != null && c.Matiere.RefSemestre != null && c.Matiere.RefSemestre.IdNiveau == parcoursId.Value);
                }

                if (classeId.HasValue)
                {
                    courses = courses.Where(c => c.IdClasse == classeId.Value);
                }

                if (matiereId.HasValue)
                {
                    courses = courses.Where(c => c.IdMatiere == matiereId.Value);
                }

                if (!string.IsNullOrEmpty(statut))
                {
                    courses = courses.Where(c => c.Statut == statut);
                }

                if (profId.HasValue)
                {
                    courses = courses.Where(c => c.IdProfesseur == profId.Value);
                }

                if (!string.IsNullOrEmpty(profEmail))
                {
                    courses = courses.Where(c => c.Professeur != null && c.Professeur.Email == profEmail);
                }

                var result = courses
                    .OrderByDescending(c => c.IdCours)
                    .Select(c => new
                    {
                        c.IdCours,
                        MatiereNom = c.Matiere != null ? c.Matiere.NomMatiere : "Non défini",
                        MatiereCode = c.Matiere != null ? c.Matiere.CodeMatiere : "",
                        FiliereNom = c.Matiere != null && c.Matiere.Filiere != null ? c.Matiere.Filiere.NomFiliere : "",
                        ClasseNom = c.Classe != null ? c.Classe.NomClasse : "Non assignée",
                        c.TypeCours,
                        c.VolumeHours,
                        RealizedHours = c.Seances != null ? c.Seances.Where(s => s.Statut != "Annulee").Sum(s => (int)(s.EndTime - s.StartTime).TotalHours) : 0,
                        RemainingHours = c.VolumeHours - (c.Seances != null ? c.Seances.Where(s => s.Statut != "Annulee").Sum(s => (int)(s.EndTime - s.StartTime).TotalHours) : 0),
                        ProfesseurNom = c.Professeur != null ? c.Professeur.Prenom + " " + c.Professeur.Nom : "Auto",
                        c.IdProfesseur,
                        c.Statut,
                        c.IdMatiere,
                        c.IdClasse
                    })
                    .ToList();

                return Ok(result);
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

            var realizedHours = course.Seances?.Where(s => s.Statut != "Annulee").Sum(s => (int)(s.EndTime - s.StartTime).TotalHours) ?? 0;

            return Ok(new
            {
                course.IdCours,
                MatiereNom = course.Matiere?.NomMatiere ?? "Non assigné",
                ClasseNom = course.Classe?.NomClasse ?? "Non assigné",
                TypeCours = course.TypeCours,
                VolumeHours = course.VolumeHours,
                ProfesseurNom = course.Professeur != null ? $"{course.Professeur.Prenom} {course.Professeur.Nom}" : "Non assigné",
                RealizedHours = realizedHours,
                ClasseEffectif = course.Classe?.Effectif ?? 0,
                course.IdProfesseur,
                course.IdClasse,
                course.IdGroupe
            });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateCourse([FromBody] Cours course)
        {
            if (course == null) return BadRequest("Course is null");

            // Si le cours pointe vers une affectation, récupérer les données
            if (course.IdAffectation.HasValue && course.IdAffectation > 0)
            {
                var affectation = await db.AffectationsMatieres
                    .Include(a => a.Classe)
                    .Include(a => a.Matiere)
                    .FirstOrDefaultAsync(a => a.IdAffectation == course.IdAffectation);

                if (affectation != null)
                {
                    course.IdMatiere = affectation.IdMatiere;
                    course.IdProfesseur = affectation.IdProfesseur;
                    course.IdClasse = affectation.IdClasse;
                    course.IdSemestre = affectation.IdSemestre;
                    course.Capacity = affectation.Classe?.Effectif ?? 30;

                    // Le volume est défini en fonction du type de cours choisi
                    if (course.TypeCours == "CM") course.VolumeHours = affectation.HeuresCm;
                    else if (course.TypeCours == "TD") course.VolumeHours = affectation.HeuresTd;
                    else if (course.TypeCours == "TP") course.VolumeHours = affectation.HeuresTp;
                    else course.VolumeHours = affectation.VolumeHoraireTotal;

                    // Au cas où les heures spécifiques sont 0, fallback sur le total
                    if (course.VolumeHours == 0) course.VolumeHours = affectation.VolumeHoraireTotal;
                }
            }
            
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

            var result = await _planningService.PlanifierCoursAsync(course);
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
        [Route("{id:int}/PlanifierSerie")]
        public async Task<IActionResult> PlanifierSerie(int id, [FromBody] PlanifierSerieRequest request)
        {
            if (request == null) return BadRequest(new { Message = "Requête invalide." });

            var cours = await db.Cours.FindAsync(id);
            if (cours == null) return NotFound(new { Message = "Cours introuvable." });

            var occurrences = new List<object>();

            // Just a basic 4 weeks repetition for the sake of the DryRun
            var currentDate = DateTime.SpecifyKind(request.DateDebut, DateTimeKind.Utc);
            for (int i = 0; i < 4; i++)
            {
                var verif = await _planningService.VerifierPlanificationAsync(id, currentDate, request.StartTime, request.EndTime, request.SalleId);
                occurrences.Add(new
                {
                    Date = currentDate,
                    HasConflict = verif.HasConflict,
                    IsWarningOnly = verif.IsWarningOnly,
                    Message = verif.Message
                });

                if (!request.DryRun && !verif.HasConflict)
                {
                    await _planningService.PlanifierSeanceAsync(id, currentDate, request.StartTime, request.EndTime, request.SalleId, request.GroupeId);
                }

                currentDate = currentDate.AddDays(7);
            }

            return Ok(new { Occurrences = occurrences });
        }

        public class PlanifierSerieRequest
        {
            public DateTime DateDebut { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int? SalleId { get; set; }
            public int? GroupeId { get; set; }
            public bool DryRun { get; set; }
        }

        [HttpPost]
        [Route("{id:int}/VerifierCreneau")]
        public async Task<IActionResult> VerifierCreneau(int id, [FromBody] VerifierCreneauRequest request)
        {
            if (request == null) return BadRequest("Requête invalide.");

            var cours = await db.Cours.FindAsync(id);
            if (cours == null) return NotFound("Cours introuvable.");

            var utcDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            var result = await _planningService.VerifierPlanificationAsync(id, utcDate, request.StartTime, request.EndTime, request.SalleId);
            return Ok(result);
        }

        public class VerifierCreneauRequest
        {
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int? SalleId { get; set; }
        }

        [HttpPost]
        [Route("{id:int}/Seance")]
        public async Task<IActionResult> ScheduleSeance(int id, [FromBody] SeanceRequest request)
        {
            try
            {
                var utcDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
                var seance = await _planningService.PlanifierSeanceAsync(id, utcDate, request.StartTime, request.EndTime, request.SalleId, request.GroupeId);
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
        [Route("Professeur/{idProfesseur:int}/Agenda")]
        public async Task<IActionResult> GetProfesseurAgenda(int idProfesseur, [FromQuery] string? date)
        {
            try
            {
                var prof = await db.Professeurs
                    .Include(p => p.Disponibilites)
                    .FirstOrDefaultAsync(p => p.IdProfesseur == idProfesseur);

                if (prof == null) return NotFound(new { Message = "Professeur non trouvé." });

                var targetDate = DateTime.TryParse(date, out var parsedDate) ? DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                var dayCode = DisponibiliteHelper.GetDayCode(targetDate);

                var disponibilites = (prof.Disponibilites ?? Enumerable.Empty<DisponibiliteProf>())
                    .Where(d => DisponibiliteHelper.IsActiveForDate(d, targetDate))
                    .OrderBy(d => d.HeureDebut)
                    .Select(d => new
                    {
                        d.HeureDebut,
                        d.HeureFin,
                        Type = d.TypeDisponibilite.ToString(),
                        Jour = d.TypeDisponibilite == TypeDisponibilite.Ponctuelle ? targetDate.ToString("yyyy-MM-dd") : dayCode
                    })
                    .ToList();

                var seances = await db.Seances
                    .Include(s => s.Cours)
                        .ThenInclude(c => c.Matiere)
                    .Include(s => s.Salle)
                    .Where(s => s.Cours.IdProfesseur == idProfesseur && s.Statut != "Annulee" && s.Date.Date == targetDate.Date)
                    .OrderBy(s => s.StartTime)
                    .Select(s => new
                    {
                        s.StartTime,
                        s.EndTime,
                        MatiereNom = s.Cours.Matiere != null ? s.Cours.Matiere.NomMatiere : "—",
                        Statut = s.Statut,
                        SalleNom = s.Salle != null ? s.Salle.NomSalle : "À définir"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Date = targetDate.ToString("yyyy-MM-dd"),
                    Disponibilites = disponibilites,
                    Seances = seances
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
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
        [Route("SallesDisponiblesPourCreneau")]
        public IActionResult GetSallesDisponiblesPourCreneau([FromBody] SallesDisponiblesRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Requête invalide.");

                var day = ParseDayOfWeek(request.JourSemaine);
                if (day == null)
                    return BadRequest("Jour de semaine non reconnu.");

                var sallesOccupeesIds = db.Creneaux
                    .Include(c => c.Cours)
                    .Where(c => c.JourSemaine == day &&
                               ((request.HeureDebut >= c.HeureDebut && request.HeureDebut < c.HeureFin) ||
                                (request.HeureFin > c.HeureDebut && request.HeureFin <= c.HeureFin) ||
                                (request.HeureDebut <= c.HeureDebut && request.HeureFin >= c.HeureFin)))
                    .Where(c => c.Cours.IdSalle != null)
                    .Select(c => c.Cours.IdSalle)
                    .ToList();

                var candidates = db.Salles
                    .Where(s => !sallesOccupeesIds.Contains(s.IdSalle))
                    .Select(s => new
                    {
                        s.IdSalle,
                        s.NomSalle,
                        s.Capacite,
                        s.TypeSalle,
                        Relevance = Math.Abs(s.Capacite - request.EffectifRequis) + (IsTypeCompatible(s.TypeSalle, request.TypeCoursRequis) ? 0 : 1000)
                    })
                    .OrderBy(s => s.Relevance)
                    .ThenBy(s => s.Capacite)
                    .ToList();

                return Ok(candidates.Select(s => new
                {
                    s.IdSalle,
                    s.NomSalle,
                    s.Capacite,
                    s.TypeSalle
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        private static string? ParseDayOfWeek(string jour)
        {
            if (string.IsNullOrWhiteSpace(jour)) return null;

            jour = jour.Trim().ToLowerInvariant();
            return jour switch
            {
                "lundi" or "mon" or "monday" => "lundi",
                "mardi" or "tue" or "tuesday" => "mardi",
                "mercredi" or "wed" or "wednesday" => "mercredi",
                "jeudi" or "thu" or "thursday" => "jeudi",
                "vendredi" or "fri" or "friday" => "vendredi",
                "samedi" or "sat" or "saturday" => "samedi",
                "dimanche" or "sun" or "sunday" => "dimanche",
                _ => jour
            };
        }

        private static bool IsTypeCompatible(string typeSalle, string typeCours)
        {
            if (string.IsNullOrWhiteSpace(typeSalle) || string.IsNullOrWhiteSpace(typeCours))
                return true;

            typeSalle = typeSalle.ToLowerInvariant();
            typeCours = typeCours.ToUpperInvariant();

            return typeCours switch
            {
                "TP" => typeSalle.Contains("tp"),
                "CM" => typeSalle.Contains("amphi") || typeSalle.Contains("cours") || typeSalle.Contains("cm"),
                "TD" => typeSalle.Contains("td") || typeSalle.Contains("cours") || typeSalle.Contains("tp"),
                _ => true,
            };
        }

        public class SallesDisponiblesRequest
        {
            public string? JourSemaine { get; set; }
            public TimeSpan HeureDebut { get; set; }
            public TimeSpan HeureFin { get; set; }
            public int EffectifRequis { get; set; }
            public string? TypeCoursRequis { get; set; }
        }

        [HttpPost]
        [Route("PlanifierAuto")]
        public async Task<IActionResult> PlanifierAuto([FromBody] Cours cours, [FromQuery] DateTime debut, [FromQuery] DateTime fin)
        {
            try
            {
                var result = await _planningService.PlanifierCoursAsync(cours);
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


        [HttpPost]
        [Route("VerifierConflit")]
        public async Task<IActionResult> VerifierConflit([FromBody] Cours cours)
        {
            if (cours == null)
                return BadRequest("Cours is required.");

            if (cours.Seances == null || !cours.Seances.Any())
                return Ok(new { HasConflict = false });

            foreach (var seance in cours.Seances)
            {
                var resultat = await _conflitService.VerifierAsync(
                    date: seance.Date,
                    heureDebut: seance.StartTime,
                    heureFin: seance.EndTime,
                    salleId: cours.IdSalle,
                    classeId: cours.IdClasse,
                    profId: cours.IdProfesseur,
                    excludeCoursId: cours.IdCours);

                if (resultat.HasConflict)
                {
                    return Ok(new
                    {
                        HasConflict = true,
                        Message = resultat.Message,
                        ConflictType = resultat.ConflictType?.ToString()
                    });
                }
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