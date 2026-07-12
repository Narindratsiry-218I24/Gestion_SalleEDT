using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

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
            var dayOfWeek = DateTime.Today.DayOfWeek;
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
                OccupancyRate = db.Salles.Any()
                    ? (double)db.Seances
                        .Where(s => s.SalleId != null && s.Date >= DateTime.Today && s.Date < DateTime.Today.AddDays(1))
                        .Select(s => s.SalleId)
                        .Distinct()
                        .Count() * 100.0 / db.Salles.Count()
                    : 0,
            };

            return Ok(stats);
        }
    }
}