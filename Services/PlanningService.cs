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

        public PlanningService(EMITDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Cours> PlanifierCourseAsync(Cours course)
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
                var course = await _context.Cours.Include(c => c.Matiere).FirstOrDefaultAsync(c => c.IdCours == courseId);
                if (course == null) throw new PlanningException("Cours non trouvé.");

                // 1. Conflit Professeur
                if (course.IdProfesseur.HasValue)
                {
                    var conflitProf = await _context.Seances
                        .Include(s => s.Cours)
                        .AnyAsync(s => s.Cours.IdProfesseur == course.IdProfesseur
                            && s.Date == date.Date
                            && ((startTime >= s.StartTime && startTime < s.EndTime)
                             || (endTime > s.StartTime && endTime <= s.EndTime)
                             || (startTime <= s.StartTime && endTime >= s.EndTime)));
                             
                    if (conflitProf) throw new PlanningException("Le professeur a déjà une séance à cet horaire.");
                }

                // 2. Conflit Classe
                if (course.IdClasse.HasValue)
                {
                    var conflitClasse = await _context.Seances
                        .Include(s => s.Cours)
                        .AnyAsync(s => s.Cours.IdClasse == course.IdClasse
                            && s.Date == date.Date
                            && ((startTime >= s.StartTime && startTime < s.EndTime)
                             || (endTime > s.StartTime && endTime <= s.EndTime)
                             || (startTime <= s.StartTime && endTime >= s.EndTime)));

                    if (conflitClasse) throw new PlanningException("La classe a déjà une séance à cet horaire.");
                }

                // 3. Conflit Salle & Capacité
                if (salleId.HasValue)
                {
                    var salle = await _context.Salles.FindAsync(salleId.Value);
                    if (salle == null) throw new PlanningException("Salle non trouvée.");

                    if (salle.Capacite < course.Capacity)
                        throw new PlanningException($"La capacité de la salle ({salle.Capacite}) est insuffisante pour ce cours ({course.Capacity}).");

                    var conflitSalle = await _context.Seances
                        .Include(s => s.Cours)
                        .AnyAsync(s => s.SalleId == salleId.Value
                            && s.Date == date.Date
                            && ((startTime >= s.StartTime && startTime < s.EndTime)
                             || (endTime > s.StartTime && endTime <= s.EndTime)
                             || (startTime <= s.StartTime && endTime >= s.EndTime)));

                    if (conflitSalle) 
                    {
                        var conflictingSeance = await _context.Seances.Include(s => s.Cours).ThenInclude(c => c.Matiere).FirstAsync(s => s.SalleId == salleId.Value && s.Date == date.Date && ((startTime >= s.StartTime && startTime < s.EndTime) || (endTime > s.StartTime && endTime <= s.EndTime) || (startTime <= s.StartTime && endTime >= s.EndTime)));
                        throw new PlanningException($"Conflit avec le cours {conflictingSeance.Cours.Matiere?.NomMatiere} en Salle {salle.NomSalle} ({conflictingSeance.StartTime} - {conflictingSeance.EndTime})");
                    }
                }

                // 4. Conflit Groupe
                if (groupeId.HasValue)
                {
                    var conflitGroupe = await _context.Seances
                        .AnyAsync(s => s.GroupeId == groupeId.Value
                            && s.Date == date.Date
                            && ((startTime >= s.StartTime && startTime < s.EndTime)
                             || (endTime > s.StartTime && endTime <= s.EndTime)
                             || (startTime <= s.StartTime && endTime >= s.EndTime)));

                    if (conflitGroupe) throw new PlanningException("Le groupe a déjà une séance à cet horaire.");
                }

                var realizedHours = (int)(endTime - startTime).TotalHours;

                var seance = new Seance
                {
                    CourseId = courseId,
                    Date = date.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    SalleId = salleId,
                    GroupeId = groupeId,
                    RealizedHours = realizedHours
                };

                _context.Seances.Add(seance);
                
                if (course.Statut == CourseStatus.Cree.ToString() || course.Statut == CourseStatus.EnAttente.ToString())
                {
                    course.Statut = CourseStatus.Planifie.ToString();
                    _context.Cours.Update(course);
                }

                await _context.SaveChangesAsync();
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

        public async Task<IEnumerable<Seance>> ObtenirEmploisDuTempsAsync()
        {
            return await _context.Seances
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .Include(s => s.Salle)
                .ToListAsync();
        }
    }
}