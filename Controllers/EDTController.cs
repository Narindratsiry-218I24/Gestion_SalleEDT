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
            var query = db.Cours
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Classe).ThenInclude(cl => cl.Filiere).ThenInclude(f => f.Mention)
                .Include(c => c.Classe).ThenInclude(cl => cl.Niveau)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(c => c.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(c => c.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(c => c.IdSalle == salleId);

            if (!string.IsNullOrEmpty(cycle) && cycle != "all")
                query = query.Where(c => c.Classe.Filiere.NomFiliere.Contains(cycle));

            if (!string.IsNullOrEmpty(niveau) && niveau != "all")
                query = query.Where(c => c.Classe.Niveau.CodeNiveau.Contains(niveau));

            if (!string.IsNullOrEmpty(semaineType) && semaineType != "all")
                query = query.Where(c => c.Creneaux.Any(cr => cr.SemaineType == semaineType));

            var coursList = query.ToList();

            var result = coursList.Select(c => new {
                c.IdCours,
                c.TypeCours,
                c.Statut,
                Matiere = c.Matiere != null ? new { c.Matiere.IdMatiere, c.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                Professeur = c.Professeur != null ? new { c.Professeur.IdProfesseur, c.Professeur.Nom, c.Professeur.Prenom } : null,
                Classe = c.Classe != null ? new { 
                    c.Classe.IdClasse, 
                    c.Classe.NomClasse,
                    Filiere = c.Classe.Filiere?.NomFiliere,
                    Mention = c.Classe.Filiere?.Mention?.NomMention,
                    Niveau = c.Classe.Niveau?.CodeNiveau
                } : null,
                Salle = c.Salle != null ? new { c.Salle.IdSalle, c.Salle.NomSalle, c.Salle.Capacite } : null,
                Creneaux = c.Creneaux?.Select(cr => new {
                    cr.IdCreneau,
                    cr.JourSemaine,
                    cr.HeureDebut,
                    cr.HeureFin,
                    cr.SemaineType
                })
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
