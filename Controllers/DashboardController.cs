using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Models.ViewModels;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly EMITDbContext db;

        public DashboardController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("Stats")]
        public IActionResult GetStats()
        {
            var dayOfWeek = DateTime.UtcNow.Date.DayOfWeek;
            string today = dayOfWeek switch
            {
                DayOfWeek.Monday    => "MON",
                DayOfWeek.Tuesday   => "TUE",
                DayOfWeek.Wednesday => "WED",
                DayOfWeek.Thursday  => "THU",
                DayOfWeek.Friday    => "FRI",
                _                   => "MON"
            };

            var stats = new
            {
                TotalSalles       = db.Salles.Count(),
                TotalProfesseurs  = db.Professeurs.Count(),
                TotalClasses      = db.Classes.Count(),
                TotalCours        = db.Cours.Count(),
                DemandesAttente   = db.DemandesEdt.Count(d =>
                    d.Statut == "en_attente" || d.Statut == "en attente" || d.Statut == "pending"),
                OccupancyRate     = db.Salles.Any()
                    ? (double)db.Seances
                        .Where(s => s.Date.Date == DateTime.UtcNow.Date && s.IdSalle != null)
                        .Select(s => s.IdSalle)
                        .Distinct()
                        .Count() / db.Salles.Count() * 100
                    : 0
            };

            return Ok(stats);
        }

        [HttpGet]
        [Route("Heures")]
        public IActionResult GetHeuresStats(int? idAnnee = null)
        {
            var annee = idAnnee.HasValue 
                ? db.AnneesAcademiques.Find(idAnnee.Value)
                : db.AnneesAcademiques.FirstOrDefault(a => a.EstActive);

            if (annee == null) return NotFound("Aucune année académique trouvée.");

            var result = new StatsHeuresViewModel
            {
                IdAnnee = annee.IdAnnee,
                AnneeLibelle = annee.Libelle
            };

            var professeurs = db.Professeurs
                .Include(p => p.AffectationsMatieres)
                .ThenInclude(a => a.Classe)
                .ToList();

            foreach (var p in professeurs)
            {
                var affectationsDeLAnnee = p.AffectationsMatieres
                    .Where(a => a.Classe != null && a.Classe.IdAnneeAcademique == annee.IdAnnee && a.EstActif)
                    .ToList();

                var seancesDeLAnnee = db.Seances
                    .Include(s => s.Cours)
                    .ThenInclude(c => c.Classe)
                    .Where(s => s.Cours != null && s.Cours.IdProfesseur == p.IdProfesseur && s.Cours.Classe != null && s.Cours.Classe.IdAnneeAcademique == annee.IdAnnee)
                    .ToList();

                var stats = new ProfesseurStats
                {
                    IdProfesseur = p.IdProfesseur,
                    Nom = p.Nom,
                    Prenom = p.Prenom,
                    VolumeAssigneCm = affectationsDeLAnnee.Sum(a => a.HeuresCm),
                    VolumeAssigneTd = affectationsDeLAnnee.Sum(a => a.HeuresTd),
                    VolumeAssigneTp = affectationsDeLAnnee.Sum(a => a.HeuresTp),

                    VolumePlanifieCm = seancesDeLAnnee.Where(s => s.Statut != "Annulee" && s.Cours?.TypeCours == "CM").Sum(s => (s.EndTime - s.StartTime).TotalHours),
                    VolumePlanifieTd = seancesDeLAnnee.Where(s => s.Statut != "Annulee" && s.Cours?.TypeCours == "TD").Sum(s => (s.EndTime - s.StartTime).TotalHours),
                    VolumePlanifieTp = seancesDeLAnnee.Where(s => s.Statut != "Annulee" && s.Cours?.TypeCours == "TP").Sum(s => (s.EndTime - s.StartTime).TotalHours),

                    VolumeRealiseCm = seancesDeLAnnee.Where(s => s.Statut == "Realisee" && s.Cours?.TypeCours == "CM").Sum(s => (s.EndTime - s.StartTime).TotalHours),
                    VolumeRealiseTd = seancesDeLAnnee.Where(s => s.Statut == "Realisee" && s.Cours?.TypeCours == "TD").Sum(s => (s.EndTime - s.StartTime).TotalHours),
                    VolumeRealiseTp = seancesDeLAnnee.Where(s => s.Statut == "Realisee" && s.Cours?.TypeCours == "TP").Sum(s => (s.EndTime - s.StartTime).TotalHours)
                };

                result.Professeurs.Add(stats);
            }

            return Ok(result);
        }
    }
}
