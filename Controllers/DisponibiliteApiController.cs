using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibiliteController : ControllerBase
    {
        private readonly EMITDbContext _db;
        private readonly IReglageDisponibiliteService _reglageService;

        public DisponibiliteController(EMITDbContext db, IReglageDisponibiliteService reglageService)
        {
            _db = db;
            _reglageService = reglageService;
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
                var currentDate = new DateTime(annee, mois, i, 0, 0, 0, DateTimeKind.Utc);
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

                var dayDispos = dispos.Where(d => DisponibiliteHelper.IsActiveForDate(d, currentDate)).ToList();

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
            public int? SelectedMatiereId { get; set; }
            public string SelectedMatiereNom { get; set; }
        }

        [HttpPost("MettreAJour")]
        public async Task<IActionResult> MettreAJour([FromBody] UpdateDispoDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required.");
            }

            var prof = await _db.Professeurs.Include(p => p.Disponibilites).FirstOrDefaultAsync(p => p.Email == dto.Email.Trim());
            if (prof == null) return NotFound("Professeur non trouvé.");

            var targetDateStrings = dto.Dispos?
                .Where(d => !string.IsNullOrWhiteSpace(d.DateStr))
                .Select(d => d.DateStr)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var disposToRemove = prof.Disponibilites
                .Where(d =>
                    d.TypeDisponibilite == TypeDisponibilite.Ponctuelle &&
                    (
                        (d.DateSpecifique.HasValue && targetDateStrings.Contains(d.DateSpecifique.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))) ||
                        (!string.IsNullOrWhiteSpace(d.JourSemaine) && targetDateStrings.Contains(d.JourSemaine))
                    ))
                .ToList();

            if (disposToRemove.Any())
            {
                _db.DisponibilitesProf.RemoveRange(disposToRemove);
            }

            var activeAnnee = await _db.AnneesAcademiques.FirstOrDefaultAsync(a => a.EstActive);
            int? activeAnneeId = activeAnnee?.IdAnnee;

            foreach (var d in dto.Dispos.Where(x => x.IsDefined))
            {
                DateTime? parsedDate = DateTime.TryParse(d.DateStr, out var pDate) ? DateTime.SpecifyKind(pDate.Date, DateTimeKind.Utc) : null;

                if (d.Matin)
                {
                    _db.DisponibilitesProf.Add(new DisponibiliteProf
                    {
                        IdProfesseur = prof.IdProfesseur,
                        JourSemaine = d.DateStr,
                        HeureDebut = new TimeSpan(8, 0, 0),
                        HeureFin = new TimeSpan(12, 0, 0),
                        SemaineType = "A",
                        TypeDisponibilite = TypeDisponibilite.Ponctuelle,
                        DateSpecifique = parsedDate,
                        IdAnnee = activeAnneeId
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
                        SemaineType = "A",
                        TypeDisponibilite = TypeDisponibilite.Ponctuelle,
                        DateSpecifique = parsedDate,
                        IdAnnee = activeAnneeId
                    });
                }

                if (!d.Matin && !d.ApresMidi)
                {
                    _db.DisponibilitesProf.Add(new DisponibiliteProf
                    {
                        IdProfesseur = prof.IdProfesseur,
                        JourSemaine = d.DateStr,
                        HeureDebut = TimeSpan.Zero,
                        HeureFin = TimeSpan.Zero,
                        SemaineType = "A",
                        TypeDisponibilite = TypeDisponibilite.Ponctuelle,
                        DateSpecifique = parsedDate,
                        IdAnnee = activeAnneeId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Disponibilités mises à jour avec succès." });
        }

        [HttpGet("Reglage/{idAnnee:int}")]
        public async Task<IActionResult> GetReglage(int idAnnee)
        {
            var reglage = await _reglageService.GetReglagePourAnneeAsync(idAnnee);
            return Ok(new
            {
                reglage.IdAnnee,
                reglage.DimancheAutorise,
                reglage.SamediAutorise,
                reglage.HeureOuverture,
                reglage.HeureFermeture,
                reglage.DureeMinCreneauMinutes
            });
        }

        [HttpPost("Reglage")]
        public async Task<IActionResult> SaveReglage([FromBody] ReglageDisponibilite reglage)
        {
            if (reglage == null) return BadRequest("Reglage is required.");
            if (reglage.HeureOuverture >= reglage.HeureFermeture) return BadRequest("Heure d'ouverture doit être antérieure à l'heure de fermeture.");
            if (reglage.DureeMinCreneauMinutes <= 0) return BadRequest("La durée minimale du créneau doit être supérieure à zéro.");

            var saved = await _reglageService.SaveReglageAsync(reglage);
            return Ok(saved);
        }
    }
}