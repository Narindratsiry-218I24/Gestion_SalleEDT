using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Services
{
    public class PlanningException : Exception
    {
        public PlanningException(string message) : base(message) { }
    }

    public class PlanningService : IPlanningService
    {
        private readonly EMITDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IConflitService _conflitService;
        private readonly IReglageDisponibiliteService _reglageService;

        public PlanningService(EMITDbContext context, IAuditService auditService, IConflitService conflitService, IReglageDisponibiliteService reglageService)
        {
            _context = context;
            _auditService = auditService;
            _conflitService = conflitService;
            _reglageService = reglageService;
        }

        public async Task<Cours> PlanifierCoursAsync(Cours course)
        {
            if (course.IdCours == 0)
            {
                course.IdCours = (await _context.Cours.MaxAsync(c => (int?)c.IdCours) ?? 0) + 1;
            }

            _context.Cours.Add(course);
            await _context.SaveChangesAsync();
            await _auditService.LogActionAsync("Cours", course.IdCours, "Create", $"Created course {course.IdCours}");
            return course;
        }

        public async Task<Seance> PlanifierSeanceAsync(int courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? salleId, int? groupeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cours = await _context.Cours.FindAsync(courseId);
                if (cours == null) throw new PlanningException("Cours non trouvé.");

                // Validation of compatibility matter/class
                var matiere = await _context.Matieres.FindAsync(cours.IdMatiere);
                var classe = await _context.Classes.FindAsync(cours.IdClasse);
                
                if (matiere != null && classe != null && matiere.IdFiliere != classe.IdFiliere)
                {
                    throw new PlanningException("La matière n'est pas compatible avec la filière de cette classe.");
                }

                // Call central validation
                var validation = await VerifierPlanificationAsync(courseId, date, startTime, endTime, salleId);
                if (validation.HasConflict && !validation.IsWarningOnly)
                {
                    throw new PlanningException(validation.Message);
                }

                var seance = new Seance
                {
                    IdCours = courseId,
                    Date = date.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    IdSalle = salleId,
                    GroupeId = groupeId,
                    Statut = "Planifiee"
                };

                _context.Seances.Add(seance);
                await _context.SaveChangesAsync();

                await RecalculerStatutCoursAsync(courseId);

                await _auditService.LogActionAsync("Seance", seance.Id, "Schedule", $"Scheduled session for course {courseId} on {date.ToShortDateString()}");

                await transaction.CommitAsync();

                return seance;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RecalculerStatutCoursAsync(int coursId)
        {
            var cours = await _context.Cours
                .Include(c => c.Seances)
                .FirstOrDefaultAsync(c => c.IdCours == coursId);
            if (cours == null) return;

            var realizedHours = cours.Seances.Sum(s => s.RealizedHours);
            var hasFutureSeance = cours.Seances.Any(s => s.Date >= DateTime.Today && s.Statut != "Annulee");

            cours.Statut = cours.Statut switch
            {
                _ when realizedHours >= cours.VolumeHours && cours.VolumeHours > 0 => "Termine",
                _ when cours.Seances.Any(s => s.Statut == "Realisee") => "EnCours",
                _ when hasFutureSeance => "Planifie",
                _ => cours.Statut // ne pas rétrograder un statut "Suspendu"/"Archive" manuel
            };
            await _context.SaveChangesAsync();
        }

        public async Task<ConflitPlanificationResult> VerifierPlanificationAsync(
            int coursId, DateTime date, TimeSpan debut, TimeSpan fin, int? salleId)
        {
            var cours = await _context.Cours
                .Include(c => c.Matiere)
                .Include(c => c.Classe)
                .Include(c => c.Seances)
                .FirstOrDefaultAsync(c => c.IdCours == coursId);

            if (cours == null)
            {
                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = "Cours",
                    Message = "Cours non trouvé.",
                    IsWarningOnly = false
                };
            }

            // Use availability settings for the academic year instead of hard-coded Sunday / 07:00-18:00 rules.
            int? idAnnee = cours.Classe?.IdAnneeAcademique;
            if (!idAnnee.HasValue)
            {
                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = "AnneeInconnue",
                    Message = "Impossible de trouver l'année académique pour ce cours.",
                    IsWarningOnly = false
                };
            }

            var horaireValide = await _reglageService.EstHeureValideAsync(date, debut, fin, idAnnee.Value);
            if (!horaireValide)
            {
                var reglage = await _reglageService.GetReglageOuDefautAsync(idAnnee.Value);
                var jourInterditMessage = date.DayOfWeek == DayOfWeek.Sunday && !reglage.DimancheAutorise
                    ? "La planification le dimanche est interdite." : null;
                var horaireMessage = (debut < reglage.HeureOuverture || fin > reglage.HeureFermeture)
                    ? $"Les cours doivent être planifiés dans le créneau autorisé de {reglage.HeureOuverture:hh\\:mm} à {reglage.HeureFermeture:hh\\:mm}."
                    : null;

                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = "HoraireInterdit",
                    Message = jourInterditMessage ?? horaireMessage ?? "Le créneau n'est pas autorisé par les règles de disponibilité.",
                    IsWarningOnly = false
                };
            }

            // Call central validation service
            var conflitResult = await _conflitService.VerifierAsync(
                date: date,
                heureDebut: debut,
                heureFin: fin,
                salleId: salleId,
                classeId: cours.IdClasse,
                profId: cours.IdProfesseur,
                excludeCoursId: coursId
            );

            if (conflitResult.HasConflict)
            {
                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = conflitResult.ConflictType?.ToString() ?? "Conflit",
                    Message = conflitResult.Message,
                    IsWarningOnly = false
                };
            }

            // 6. Dépassement du VolumeHours du cours (blocage dur)
            var newHours = (int)(fin - debut).TotalHours;
            var realizedHours = cours.Seances.Where(s => s.Statut != "Annulee").Sum(s => s.DurationHours);
            if (realizedHours + newHours > cours.VolumeHours && cours.VolumeHours > 0)
            {
                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = "VolumeDepasse",
                    Message = $"Dépassement du volume horaire. Planifié: {realizedHours}h, Nouveau: {newHours}h, Max: {cours.VolumeHours}h.",
                    IsWarningOnly = false
                };
            }

            // Warnings
            var warnings = new List<string>();

            // 4. Capacité salle < effectif classe (avertissement)
            if (salleId.HasValue && cours.IdClasse.HasValue)
            {
                var salle = await _context.Salles.FindAsync(salleId.Value);
                var classe = await _context.Classes.FindAsync(cours.IdClasse.Value);
                if (salle != null && classe != null && salle.Capacite < classe.Effectif)
                {
                    warnings.Add($"La capacité de la salle ({salle.Capacite}) est inférieure à l'effectif de la classe ({classe.Effectif}).");
                }
            }

            // TypeSalle vs TypeCours (avertissement)
            if (salleId.HasValue && !string.IsNullOrEmpty(cours.TypeCours))
            {
                var salle = await _context.Salles.FindAsync(salleId.Value);
                if (salle != null)
                {
                    if (cours.TypeCours.ToUpper() == "TP" && salle.TypeSalle.ToLower() != "tp")
                    {
                        warnings.Add($"Cohérence type de salle : le cours est un 'TP' mais la salle est de type '{salle.TypeSalle}'.");
                    }
                }
            }

            // 5. Hors disponibilité déclarée du prof (avertissement)
            if (cours.IdProfesseur.HasValue)
            {
                string dayCode = date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "MON",
                    DayOfWeek.Tuesday => "TUE",
                    DayOfWeek.Wednesday => "WED",
                    DayOfWeek.Thursday => "THU",
                    DayOfWeek.Friday => "FRI",
                    DayOfWeek.Saturday => "SAT",
                    _ => ""
                };

                // Find all availabilities for this professor, scoped to this academic year (or null)
                var profDispos = await _context.DisponibilitesProf
                    .Where(d => d.IdProfesseur == cours.IdProfesseur.Value && (d.IdAnnee == null || d.IdAnnee == idAnnee))
                    .ToListAsync();

                // Separate recurrent and specific
                var disposRecurrentes = profDispos.Where(d => d.TypeDisponibilite == TypeDisponibilite.Recurrente && d.JourSemaineCode == dayCode).ToList();
                var disposPonctuelles = profDispos.Where(d => d.TypeDisponibilite == TypeDisponibilite.Ponctuelle && d.DateSpecifique.HasValue && d.DateSpecifique.Value.Date == date.Date).ToList();

                var matchingDispos = disposPonctuelles.Any() ? disposPonctuelles : disposRecurrentes;

                if (!matchingDispos.Any())
                {
                    warnings.Add($"Le professeur n'a pas déclaré de disponibilité pour ce jour ({date.ToShortDateString()}).");
                }
                else
                {
                    bool isCovered = matchingDispos.Any(d => d.HeureDebut <= debut && d.HeureFin >= fin);
                    if (!isCovered)
                    {
                        warnings.Add($"Le créneau demandé ({debut:hh\\:mm}-{fin:hh\\:mm}) est en dehors des disponibilités déclarées du professeur pour ce jour.");
                    }
                }
            }

            if (warnings.Any())
            {
                return new ConflitPlanificationResult
                {
                    HasConflict = true,
                    ConflictType = "DisponibiliteProf",
                    Message = string.Join("\n", warnings),
                    IsWarningOnly = true
                };
            }

            return new ConflitPlanificationResult
            {
                HasConflict = false
            };
        }

        public async Task<IEnumerable<Seance>> ObtenirEmploisDuTempsAsync()
        {
            return await _context.Seances
                .Include(s => s.Cours)
                    .ThenInclude(c => c.Matiere)
                .Include(s => s.Cours)
                    .ThenInclude(c => c.Salle)
                .Include(s => s.Cours)
                    .ThenInclude(c => c.Professeur)
                .Include(s => s.Cours)
                    .ThenInclude(c => c.Classe)
                .ToListAsync();
        }
    }
}
