using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibiliteController : ControllerBase
    {
        private readonly EMITDbContext _db;

        public DisponibiliteController(EMITDbContext db)
        {
            _db = db;
        }
        [HttpGet("Mois")]
        public async Task<IActionResult> GetMois(string email, int mois, int annee)
        {
            var prof = await _db.Professeurs.FirstOrDefaultAsync(p => p.Email == email);
            if (prof == null) return NotFound("Professeur non trouvé.");

            var startDate = new DateTime(annee, mois, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var dispos = await _db.DisponibilitesProf
                .Where(d => d.IdProfesseur == prof.IdProfesseur)
                .ToListAsync();

            var coursQuery = await _db.Cours
                .Include(c => c.Matiere)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .Include(c => c.AffectationMatiere)
                .Where(c => c.IdProfesseur == prof.IdProfesseur)
                .ToListAsync();

            var user = await _db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);
            int userId = user?.IdUtilisateur ?? 0;

            var proposedSlots = await _db.DemandesEdt
                .Where(d => d.IdDemandeur == userId && d.DateSouhaitee >= startDate && d.DateSouhaitee <= endDate)
                .ToListAsync();

            var jours = new List<object>();

            for (int i = 1; i <= endDate.Day; i++)
            {
                var currentDate = new DateTime(annee, mois, i);
                var dateStr = currentDate.ToString("yyyy-MM-dd");
                var jourSemaineEnum = currentDate.DayOfWeek;
                string jourStr = jourSemaineEnum switch
                {
                    DayOfWeek.Monday => "MON",
                    DayOfWeek.Tuesday => "TUE",
                    DayOfWeek.Wednesday => "WED",
                    DayOfWeek.Thursday => "THU",
                    DayOfWeek.Friday => "FRI",
                    DayOfWeek.Saturday => "SAT",
                    DayOfWeek.Sunday => "SUN",
                    _ => ""
                };

                // Check specific date availability (YYYY-MM-DD)
                var dayDispos = dispos.Where(d => d.JourSemaine == dateStr).ToList();

                bool hasMatin = dayDispos.Any(d => d.HeureDebut <= new TimeSpan(8, 0, 0) && d.HeureFin >= new TimeSpan(12, 0, 0));
                bool hasAprem = dayDispos.Any(d => d.HeureDebut <= new TimeSpan(14, 0, 0) && d.HeureFin >= new TimeSpan(18, 0, 0));

                bool hasProposed = proposedSlots.Any(p => p.DateSouhaitee.HasValue && p.DateSouhaitee.Value.Date == currentDate.Date);

                string statut = "blanc"; // non défini
                if (hasProposed) statut = "violet";
                else if (hasMatin && hasAprem) statut = "vert";
                else if (hasMatin || hasAprem) statut = "orange";
                else if (dayDispos.Any()) statut = "rouge"; 

                var coursDuJour = coursQuery.SelectMany(c => c.Creneaux
                    .Where(cr => cr.JourSemaine.ToUpper() == jourStr &&
                                 (c.AffectationMatiere == null || 
                                 (currentDate.Date >= c.AffectationMatiere.DateDebut.Date && currentDate.Date <= c.AffectationMatiere.DateFin.Date)))
                    .Select(cr => new {
                        heureDebut = cr.HeureDebut.ToString(@"hh\:mm"),
                        heureFin = cr.HeureFin.ToString(@"hh\:mm"),
                        matiere = c.Matiere?.NomMatiere ?? "Cours",
                        type = c.TypeCours ?? "Cours",
                        salle = c.Salle?.NomSalle ?? "À définir"
                    })).OrderBy(c => c.heureDebut).ToList();

                jours.Add(new {
                    date = dateStr,
                    jourSemaine = jourStr,
                    matin = hasMatin,
                    apresMidi = hasAprem,
                    statut = statut,
                    hasCours = coursDuJour.Any(),
                    cours = coursDuJour
                });
            }

            return Ok(new { jours = jours });
        }

        public class UpdateDispoDto
        {
            public string Email { get; set; }
            public List<DispoItem> Dispos { get; set; }
        }

        public class DispoItem
        {
            public string DateStr { get; set; } // yyyy-MM-dd
            public bool Matin { get; set; }
            public bool ApresMidi { get; set; }
            public bool IsDefined { get; set; } // Si le jour a été cliqué/défini
        }

        [HttpPost("MettreAJour")]
        public async Task<IActionResult> MettreAJour([FromBody] UpdateDispoDto dto)
        {
            var prof = await _db.Professeurs.Include(p => p.Disponibilites).FirstOrDefaultAsync(p => p.Email == dto.Email);
            if (prof == null) return NotFound("Professeur non trouvé.");

            var datesAUpdate = dto.Dispos.Select(d => d.DateStr).ToList();

            // Supprimer uniquement les disponibilités des dates reçues
            var disposToRemove = prof.Disponibilites.Where(d => datesAUpdate.Contains(d.JourSemaine)).ToList();
            _db.DisponibilitesProf.RemoveRange(disposToRemove);

            foreach (var d in dto.Dispos.Where(x => x.IsDefined))
            {
                if (d.Matin)
                {
                    _db.DisponibilitesProf.Add(new DisponibiliteProf
                    {
                        IdProfesseur = prof.IdProfesseur,
                        JourSemaine = d.DateStr,
                        HeureDebut = new TimeSpan(8, 0, 0),
                        HeureFin = new TimeSpan(12, 0, 0),
                        SemaineType = "A"
                    });
                }
                if (d.ApresMidi)
                {
                    _db.DisponibilitesProf.Add(new DisponibiliteProf
                    {
                        IdProfesseur = prof.IdProfesseur,
                        JourSemaine = d.DateStr,
                        HeureDebut = new TimeSpan(14, 0, 0),
                        HeureFin = new TimeSpan(18, 0, 0),
                        SemaineType = "A"
                    });
                }
                
                // Si IsDefined est true mais Matin=false et ApresMidi=false, on crée un enregistrement "rouge" (0h - 0h) pour marquer l'indisponibilité
                if (!d.Matin && !d.ApresMidi)
                {
                    _db.DisponibilitesProf.Add(new DisponibiliteProf
                    {
                        IdProfesseur = prof.IdProfesseur,
                        JourSemaine = d.DateStr,
                        HeureDebut = TimeSpan.Zero,
                        HeureFin = TimeSpan.Zero,
                        SemaineType = "A"
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Disponibilités mises à jour avec succès." });
        }
    }
}