using System;
using System.Linq;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Services
{
    public class SchedulingService
    {
        private readonly EMITDbContext _context;

        public SchedulingService(EMITDbContext context)
        {
            _context = context;
        }

        public async Task<Schedule> CreateScheduleAsync(Schedule schedule, string user = null)
        {
            // Basic business rules
            // Prevent library room C001 usage
            if (schedule.SalleId.HasValue)
            {
                var salle = await _context.Salles.FindAsync(schedule.SalleId.Value);
                if (salle != null && (salle.NomSalle == "C001" || salle.NumeroPorte == "C001"))
                {
                    throw new Exception("La salle C001 (bibliothèque) ne peut pas être utilisée pour des cours.");
                }
            }

            var jour = schedule.Date.ToString("dddd");
            var debut = schedule.HeureDebut;
            var fin = schedule.HeureFin;

            // Check teacher conflict against existing schedules
            if (schedule.ProfesseurId.HasValue)
            {
                var conflitProf = await _context.Schedules
                    .AnyAsync(s => s.ProfesseurId == schedule.ProfesseurId
                        && s.Date.Date == schedule.Date.Date
                        && (debut < s.HeureFin && fin > s.HeureDebut));

                if (conflitProf) throw new Exception("Le professeur a déjà un cours à cet horaire.");
            }

            // Check room conflict
            if (schedule.SalleId.HasValue)
            {
                var conflitSalle = await _context.Schedules
                    .AnyAsync(s => s.SalleId == schedule.SalleId
                        && s.Date.Date == schedule.Date.Date
                        && (debut < s.HeureFin && fin > s.HeureDebut));

                if (conflitSalle) throw new Exception("La salle est occupée à cet horaire.");
            }

            // Save schedule
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = nameof(Schedule),
                EntityId = schedule.Id,
                Operation = "Create",
                ChangedBy = user ?? "system",
                ChangedAt = DateTime.UtcNow,
                Details = $"SubjectId={schedule.SubjectId};ProfesseurId={schedule.ProfesseurId};SalleId={schedule.SalleId}"
            });
            await _context.SaveChangesAsync();

            return schedule;
        }
    }
}
