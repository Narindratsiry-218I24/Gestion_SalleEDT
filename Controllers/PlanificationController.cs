using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("[controller]")]
    public class PlanificationController : Controller
    {
        private readonly EMITDbContext _context;
        private readonly IPlanningService _planningService;
        private readonly ILogger<PlanificationController> _logger;

        public PlanificationController(
            EMITDbContext context,
            IPlanningService planningService,
            ILogger<PlanificationController> logger)
        {
            _context = context;
            _planningService = planningService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index([FromQuery] int? courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpGet("Selection")]
        public IActionResult Selection(int? anneeId)
        {
            return View("Index", new { anneeId });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PlanificationApiController : ControllerBase
    {
        private readonly EMITDbContext _context;
        private readonly IPlanningService _planningService;
        private readonly ILogger<PlanificationApiController> _logger;

        public PlanificationApiController(
            EMITDbContext context,
            IPlanningService planningService,
            ILogger<PlanificationApiController> logger)
        {
            _context = context;
            _planningService = planningService;
            _logger = logger;
        }

        [HttpGet("Overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int anneeId)
        {
            try
            {
                var annee = await _context.AnneesAcademiques.FindAsync(anneeId);
                if (annee == null)
                    return NotFound(new { Message = "Année académique non trouvée." });

                var courses = await _context.Cours
                    .Include(c => c.Classe)
                    .Include(c => c.Matiere)
                    .ThenInclude(m => m.Filiere)
                    .Include(c => c.Seances)
                    .Where(c => c.Classe != null && c.Classe.IdAnneeAcademique == anneeId)
                    .ToListAsync();

                var totalHours = courses.Sum(c => c.VolumeHours);
                var plannedHours = 0;
                var coursesToPlan = 0;
                var coursesPlanned = 0;
                var coursesWithConflicts = 0;

                foreach (var cours in courses)
                {
                    var seances = cours.Seances?.Where(s => s.Statut != "Annulee").ToList() ?? new List<Seance>();
                    var hoursPlanned = seances.Sum(s => (int)(s.EndTime - s.StartTime).TotalHours);
                    plannedHours += hoursPlanned;

                    if (hoursPlanned == 0)
                    {
                        coursesToPlan++;
                    }
                    else if (hoursPlanned >= cours.VolumeHours)
                    {
                        coursesPlanned++;
                    }
                }

                // Group by filiere
                var filiereStats = courses
                    .GroupBy(c => c.Matiere?.Filiere?.NomFiliere ?? "Sans filière")
                    .Select(g => new
                    {
                        Filiere = g.Key,
                        TotalHours = g.Sum(c => c.VolumeHours),
                        PlannedHours = g.Sum(c => c.Seances?.Where(s => s.Statut != "Annulee").Sum(s => (int)(s.EndTime - s.StartTime).TotalHours) ?? 0),
                        CoursCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalHours)
                    .Take(3)
                    .ToList();

                return Ok(new
                {
                    AnneeId = anneeId,
                    AnneeNom = annee.Libelle,
                    TotalHours = totalHours,
                    PlannedHours = plannedHours,
                    RemainingHours = totalHours - plannedHours,
                    CoursesToPlan = coursesToPlan,
                    CoursesPlanned = coursesPlanned,
                    CoursesWithConflicts = coursesWithConflicts,
                    TotalCourses = courses.Count,
                    FiliereStats = filiereStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul de l'aperçu de planification.");
                return StatusCode(500, new { Message = "Erreur serveur lors du calcul de l'aperçu." });
            }
        }

        /// <summary>
        /// Get courses to plan, filtered by various criteria (Step 2)
        /// </summary>
        [HttpGet("CoursesToPlan")]
        public async Task<IActionResult> GetCoursesToPlan(
            [FromQuery] int anneeId,
            [FromQuery] int? filiereId = null,
            [FromQuery] int? classeId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? statut = null)
        {
            try
            {
                var query = _context.Cours
                    .Include(c => c.Classe)
                    .Include(c => c.Matiere)
                    .ThenInclude(m => m.Filiere)
                    .Include(c => c.Professeur)
                    .Include(c => c.Seances)
                    .Where(c => c.Classe != null && c.Classe.IdAnneeAcademique == anneeId)
                    .AsQueryable();

                // Filter by filiere
                if (filiereId.HasValue)
                {
                    query = query.Where(c => c.Matiere != null && c.Matiere.IdFiliere == filiereId.Value);
                }

                // Filter by classe
                if (classeId.HasValue)
                {
                    query = query.Where(c => c.IdClasse == classeId.Value);
                }

                // Filter by search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(c =>
                        (c.Matiere != null && c.Matiere.NomMatiere.ToLower().Contains(searchLower)) ||
                        (c.Professeur != null && (c.Professeur.Nom.ToLower().Contains(searchLower) || c.Professeur.Prenom.ToLower().Contains(searchLower))) ||
                        (c.Classe != null && c.Classe.NomClasse.ToLower().Contains(searchLower))
                    );
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(statut))
                {
                    query = query.Where(c => c.Statut == statut);
                }

                var courses = await query
                    .OrderBy(c => c.Matiere != null ? c.Matiere.NomMatiere : "")
                    .ToListAsync();

                var result = courses.Select(c =>
                {
                    var hoursPlanned = c.Seances?.Where(s => s.Statut != "Annulee").Sum(s => (int)(s.EndTime - s.StartTime).TotalHours) ?? 0;
                    return new
                    {
                        c.IdCours,
                        MatiereNom = c.Matiere?.NomMatiere ?? "N/A",
                        MatiereCode = c.Matiere?.CodeMatiere ?? "",
                        ClasseNom = c.Classe?.NomClasse ?? "N/A",
                        TypeCours = c.TypeCours,
                        VolumeHours = c.VolumeHours,
                        PlannedHours = hoursPlanned,
                        RemainingHours = c.VolumeHours - hoursPlanned,
                        ProfesseurNom = c.Professeur != null ? c.Professeur.Prenom + " " + c.Professeur.Nom : "Auto-assigné",
                        c.IdProfesseur,
                        c.Statut,
                        Priority = hoursPlanned == 0 ? 1 : (hoursPlanned < c.VolumeHours ? 2 : 3)
                    };
                }).OrderBy(x => x.Priority).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des cours à planifier.");
                return StatusCode(500, new { Message = "Erreur serveur." });
            }
        }

        /// <summary>
        /// Get session assignment options (Step 3) - professor and room suggestions
        /// </summary>
        [HttpPost("SuggestSession")]
        public async Task<IActionResult> SuggestSession([FromBody] SuggestSessionRequest request)
        {
            if (request == null || request.IdCours <= 0)
                return BadRequest("ID cours requis.");

            try
            {
                var cours = await _context.Cours
                    .Include(c => c.Matiere)
                    .Include(c => c.Classe)
                    .Include(c => c.Professeur)
                    .Include(c => c.Seances)
                    .FirstOrDefaultAsync(c => c.IdCours == request.IdCours);

                if (cours == null)
                    return NotFound("Cours non trouvé.");

                // Validate date and time
                if (request.DateSeance == default || request.HeureDebut >= request.HeureFin)
                    return BadRequest("Date et heure invalides.");

                var dayOfWeek = request.DateSeance.ToString("dddd");
                var jour = NormalizeDayOfWeek(dayOfWeek);

                // Check for conflicts
                var conflictCheck = await _planningService.VerifierPlanificationAsync(
                    cours.IdCours,
                    DateTime.SpecifyKind(request.DateSeance, DateTimeKind.Utc),
                    request.HeureDebut,
                    request.HeureFin,
                    request.IdSalle
                );

                if (conflictCheck.HasConflict)
                {
                    return BadRequest(new
                    {
                        Message = "Conflit détecté.",
                        Conflicts = new[] { conflictCheck.Message }
                    });
                }

                // Get professor suggestions
                var professorSuggestions = await GetProfessorSuggestions(cours);

                // Get room suggestions
                var effectif = cours.Classe?.Effectif ?? 30;
                var roomSuggestions = await GetRoomSuggestions(jour, request.HeureDebut, request.HeureFin, effectif, request.TypeCoursRequis);

                return Ok(new
                {
                    CoursInfo = new
                    {
                        cours.IdCours,
                        MatiereNom = cours.Matiere?.NomMatiere,
                        ClasseNom = cours.Classe?.NomClasse,
                        cours.VolumeHours,
                        cours.TypeCours
                    },
                    DateSeance = request.DateSeance,
                    HeureDebut = request.HeureDebut,
                    HeureFin = request.HeureFin,
                    ProfessorSuggestions = professorSuggestions,
                    RoomSuggestions = roomSuggestions,
                    ValidationResult = new
                    {
                        IsConflictFree = true,
                        DaysConstraintOk = !IsWeekendOrAfterHours(jour, request.HeureDebut, request.HeureFin),
                        Message = "Créneaux disponibles pour cette séance."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suggestion de séance.");
                return StatusCode(500, new { Message = "Erreur serveur." });
            }
        }

        /// <summary>
        /// Confirm and create a session (final step)
        /// </summary>
        [HttpPost("ConfirmSession")]
        public async Task<IActionResult> ConfirmSession([FromBody] ConfirmSessionRequest request)
        {
            if (request == null || request.IdCours <= 0)
                return BadRequest("Données invalides.");

            try
            {
                var cours = await _context.Cours
                    .Include(c => c.Classe)
                    .Include(c => c.Seances)
                    .FirstOrDefaultAsync(c => c.IdCours == request.IdCours);

                if (cours == null)
                    return NotFound("Cours non trouvé.");

                // Validate final session
                var jour = NormalizeDayOfWeek(request.DateSeance.ToString("dddd"));
                
                var validationResult = await _planningService.VerifierPlanificationAsync(
                    cours.IdCours,
                    DateTime.SpecifyKind(request.DateSeance, DateTimeKind.Utc),
                    request.HeureDebut,
                    request.HeureFin,
                    request.IdSalle
                );

                if (validationResult.HasConflict)
                {
                    return BadRequest(new
                    {
                        Message = "Conflit détecté lors de la confirmation.",
                        Conflicts = new[] { validationResult.Message }
                    });
                }

                // Create seance
                var seance = new Seance
                {
                    IdCours = cours.IdCours,
                    Date = request.DateSeance,
                    StartTime = request.HeureDebut,
                    EndTime = request.HeureFin,
                    IdSalle = request.IdSalle,
                    Statut = "Planifiee"
                };

                _context.Seances.Add(seance);

                // Update course professor if provided
                if (request.IdProfesseur.HasValue)
                {
                    cours.IdProfesseur = request.IdProfesseur.Value;
                }

                // Update course status
                cours.Statut = "Planifie";

                _context.Cours.Update(cours);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Séance confirmée avec succès.",
                    SeanceId = seance.IdSeance,
                    CoursId = cours.IdCours,
                    Seance = new
                    {
                        seance.IdSeance,
                        seance.Date,
                        seance.StartTime,
                        seance.EndTime,
                        seance.IdSalle,
                        seance.Statut
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation de la séance.");
                return StatusCode(500, new { Message = "Erreur serveur lors de la confirmation." });
            }
        }

        private async Task<List<dynamic>> GetProfessorSuggestions(Cours cours)
        {
            var suggestions = new List<dynamic>();

            if (cours.IdProfesseur.HasValue)
            {
                var currentProf = await _context.Professeurs.FindAsync(cours.IdProfesseur.Value);
                if (currentProf != null)
                {
                    suggestions.Add(new
                    {
                        currentProf.IdProfesseur,
                        Nom = currentProf.Prenom + " " + currentProf.Nom,
                        CapaciteHoraireMax = currentProf.CapaciteHoraireMax,
                        HeuresEffectuees = currentProf.HeuresEffectuees,
                        RemainingCapacity = currentProf.CapaciteHoraireMax - currentProf.HeuresEffectuees,
                        Type = "Actuel",
                        Priority = 1
                    });
                }
            }

            var affectations = await _context.AffectationsMatieres
                .Include(a => a.Professeur)
                .Where(a => a.EstActif && a.IdMatiere == cours.IdMatiere && a.IdClasse == cours.IdClasse)
                .ToListAsync();

            var professorsAdded = new HashSet<int>();
            if (cours.IdProfesseur.HasValue)
                professorsAdded.Add(cours.IdProfesseur.Value);

            foreach (var aff in affectations)
            {
                if (aff.Professeur != null && professorsAdded.Add(aff.Professeur.IdProfesseur))
                {
                    suggestions.Add(new
                    {
                        aff.Professeur.IdProfesseur,
                        Nom = aff.Professeur.Prenom + " " + aff.Professeur.Nom,
                        aff.Professeur.CapaciteHoraireMax,
                        aff.Professeur.HeuresEffectuees,
                        RemainingCapacity = aff.Professeur.CapaciteHoraireMax - aff.Professeur.HeuresEffectuees,
                        Type = "Affecté",
                        Priority = 2
                    });
                }
            }

            return suggestions.OrderBy(s => ((dynamic)s).Priority).ThenByDescending(s => ((dynamic)s).RemainingCapacity).ToList();
        }

        private async Task<List<dynamic>> GetRoomSuggestions(string jour, TimeSpan heureDebut, TimeSpan heureFin, int effectif, string? typeCours)
        {
            var occupiedRoomIds = await _context.Creneaux
                .Include(c => c.Cours)
                .Where(c => c.JourSemaine == jour &&
                           ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin) ||
                            (heureFin > c.HeureDebut && heureFin <= c.HeureFin) ||
                            (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)) &&
                           c.Cours.IdSalle != null)
                .Select(c => c.Cours.IdSalle)
                .ToListAsync();

            var available = await _context.Salles
                .Where(s => !occupiedRoomIds.Contains(s.IdSalle))
                .Select(s => new
                {
                    s.IdSalle,
                    s.NomSalle,
                    s.Capacite,
                    s.TypeSalle,
                    Relevance = Math.Abs(s.Capacite - effectif) + (IsTypeCompatible(s.TypeSalle, typeCours) ? 0 : 1000)
                })
                .OrderBy(s => s.Relevance)
                .ThenBy(s => s.Capacite)
                .Take(5)
                .ToListAsync();

            return available.Cast<dynamic>().ToList();
        }

        private static string NormalizeDayOfWeek(string jour)
        {
            if (string.IsNullOrWhiteSpace(jour)) return jour;
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

        private static bool IsTypeCompatible(string? typeSalle, string? typeCours)
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
                _ => true
            };
        }

        private static bool IsWeekendOrAfterHours(string jour, TimeSpan debut, TimeSpan fin)
        {
            if (jour == "dimanche" || jour == "samedi")
                return true;

            if (debut < new TimeSpan(7, 0, 0) || fin > new TimeSpan(18, 0, 0))
                return true;

            return false;
        }
    }

    public class SuggestSessionRequest
    {
        public int IdCours { get; set; }
        public DateTime DateSeance { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public int? IdSalle { get; set; }
        public string? TypeCoursRequis { get; set; }
    }

    public class ConfirmSessionRequest
    {
        public int IdCours { get; set; }
        public DateTime DateSeance { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public int? IdSalle { get; set; }
        public int? IdProfesseur { get; set; }
    }
}
