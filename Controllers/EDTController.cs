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

        // GET: api/EDT
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
                .Include(c => c.Classe).ThenInclude(cl => cl.AnneeAcademique)
                .Include(c => c.Semestre).ThenInclude(s => s.RefSemestre).ThenInclude(rs => rs.Niveau)
                .Include(c => c.AffectationMatiere)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(c => c.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(c => c.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(c => c.IdSalle == salleId);

            if (!string.IsNullOrEmpty(cycle) && cycle != "all")
                query = query.Where(c => c.Classe.Filiere.NomFiliere.Contains(cycle));

            if (!string.IsNullOrEmpty(niveau) && niveau != "all")
                query = query.Where(c => c.Semestre.RefSemestre.Niveau.CodeNiveau.Contains(niveau));

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
                    Annee = c.Classe.AnneeAcademique?.Libelle,
                    Niveau = c.Semestre?.RefSemestre?.Niveau?.CodeNiveau,
                    Semestre = c.Semestre?.RefSemestre?.CodeSemestre
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

        // GET: api/EDT/ParSalle/{id}
        [HttpGet]
        [Route("ParSalle/{id:int}")]
        public IActionResult GetEDTParSalle(int id)
        {
            var coursList = db.Cours
                .Where(c => c.IdSalle == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Classe)
                .Include(c => c.Creneaux)
                .ToList();

            var result = coursList.Select(c => new {
                c.IdCours,
                c.TypeCours,
                c.Statut,
                Matiere = c.Matiere != null ? new { c.Matiere.IdMatiere, c.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                Professeur = c.Professeur != null ? new { c.Professeur.IdProfesseur, c.Professeur.Nom, c.Professeur.Prenom } : null,
                Classe = c.Classe != null ? new { c.Classe.IdClasse, c.Classe.NomClasse } : null,
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

        // GET: api/EDT/ParClasse/{id}
        [HttpGet]
        [Route("ParClasse/{id:int}")]
        public IActionResult GetEDTParClasse(int id)
        {
            var coursList = db.Cours
                .Where(c => c.IdClasse == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .ToList();

            var result = coursList.Select(c => new {
                c.IdCours,
                c.TypeCours,
                c.Statut,
                Matiere = c.Matiere != null ? new { c.Matiere.IdMatiere, c.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                Professeur = c.Professeur != null ? new { c.Professeur.IdProfesseur, c.Professeur.Nom, c.Professeur.Prenom } : null,
                Salle = c.Salle != null ? new { c.Salle.IdSalle, c.Salle.NomSalle } : null,
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

        // GET: api/EDT/ParProfesseur/{id}
        [HttpGet]
        [Route("ParProfesseur/{id:int}")]
        public IActionResult GetEDTParProfesseur(int id)
        {
            var coursList = db.Cours
                .Where(c => c.IdProfesseur == id)
                .Include(c => c.Matiere)
                .Include(c => c.Classe)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .ToList();

            var result = coursList.Select(c => new {
                c.IdCours,
                c.TypeCours,
                c.Statut,
                Matiere = c.Matiere != null ? new { c.Matiere.IdMatiere, c.Matiere.NomMatiere, Couleur = "#3b82f6" } : null,
                Classe = c.Classe != null ? new { c.Classe.IdClasse, c.Classe.NomClasse } : null,
                Salle = c.Salle != null ? new { c.Salle.IdSalle, c.Salle.NomSalle } : null,
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
    }
}
