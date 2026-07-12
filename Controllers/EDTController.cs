using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/EDT")]
    [ApiController]
    public class EDTController : ControllerBase
    {
        private readonly EMITDbContext db;

        public EDTController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetHebdomadaire(
            int? classeId = null, int? profId = null, int? salleId = null,
            string? cycle = null, string? niveau = null, string? semaineType = null)
        {
            var seancesQuery = db.Seances
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe).ThenInclude(cl => cl.Filiere).ThenInclude(f => f.Mention)
                .Include(s => s.Cours).ThenInclude(c => c.Classe).ThenInclude(cl => cl.Niveau)
                .Include(s => s.Cours).ThenInclude(c => c.Salle)
                .Include(s => s.Salle)
                .AsQueryable();

            if (classeId.HasValue) seancesQuery = seancesQuery.Where(s => s.Cours.IdClasse == classeId.Value);
            if (profId.HasValue)   seancesQuery = seancesQuery.Where(s => s.Cours.IdProfesseur == profId.Value);
            if (salleId.HasValue)  seancesQuery = seancesQuery.Where(s => s.SalleId == salleId.Value);

            if (!string.IsNullOrEmpty(cycle) && cycle != "all")
                seancesQuery = seancesQuery.Where(s => s.Cours.Classe.Filiere.NomFiliere.Contains(cycle));

            if (!string.IsNullOrEmpty(niveau) && niveau != "all")
                seancesQuery = seancesQuery.Where(s => s.Cours.Classe.Niveau.CodeNiveau.Contains(niveau));

            var seances = seancesQuery.ToList();

            var result = seances.Select(s => new {
                s.IdSeance,
                Date = s.Date.ToString("yyyy-MM-dd"),
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                    Classe = s.Cours.Classe != null ? new { 
                        s.Cours.Classe.IdClasse,
                        s.Cours.Classe.NomClasse,
                        Filiere = s.Cours.Classe.Filiere?.NomFiliere,
                        Mention = s.Cours.Classe.Filiere?.Mention?.NomMention,
                        Niveau = s.Cours.Classe.Niveau?.CodeNiveau
                    } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle, s.Salle.Capacite } : null
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("ParSalle/{id:int}")]
        public IActionResult GetEDTParSalle(int id)
        {
            var seances = db.Seances
                .Where(s => s.SalleId == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                    Classe = s.Cours.Classe != null ? new { s.Cours.Classe.IdClasse, s.Cours.Classe.NomClasse } : null,
                }
            });
            return Ok(result);
        }

        [HttpGet]
        [Route("ParClasse/{id:int}")]
        public IActionResult GetEDTParClasse(int id)
        {
            var seances = db.Seances
                .Where(s => s.Cours.IdClasse == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Professeur)
                .Include(s => s.Salle)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Professeur = s.Cours.Professeur != null ? new { s.Cours.Professeur.IdProfesseur, s.Cours.Professeur.Nom, s.Cours.Professeur.Prenom } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle } : null
            });
            return Ok(result);
        }

        [HttpGet]
        [Route("ParProfesseur/{id:int}")]
        public IActionResult GetEDTParProfesseur(int id)
        {
            var seances = db.Seances
                .Where(s => s.Cours.IdProfesseur == id)
                .Include(s => s.Cours).ThenInclude(c => c.Matiere)
                .Include(s => s.Cours).ThenInclude(c => c.Classe)
                .Include(s => s.Salle)
                .ToList();

            var result = seances.Select(s => new {
                s.Id,
                s.Date,
                s.StartTime,
                s.EndTime,
                Course = new {
                    s.Cours.IdCours,
                    s.Cours.TypeCours,
                    s.Cours.Statut,
                    Matiere = s.Cours.Matiere != null ? new { s.Cours.Matiere.IdMatiere, s.Cours.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                    Classe = s.Cours.Classe != null ? new { s.Cours.Classe.IdClasse, s.Cours.Classe.NomClasse } : null,
                },
                Salle = s.Salle != null ? new { s.Salle.IdSalle, s.Salle.NomSalle } : null
            });
            return Ok(result);
        }
    }
}
