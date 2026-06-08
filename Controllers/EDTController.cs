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
                .Include("Classe.Filiere")
                .Include("Classe.Semestre.RefSemestre")
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(c => c.IdClasse == classeId);
            if (profId.HasValue)   query = query.Where(c => c.IdProfesseur == profId);
            if (salleId.HasValue)  query = query.Where(c => c.IdSalle == salleId);

            if (!string.IsNullOrEmpty(cycle) && cycle != "all")
                query = query.Where(c => c.Classe.Filiere.NomFiliere == cycle);

            if (!string.IsNullOrEmpty(niveau) && niveau != "all")
                query = query.Where(c => c.Classe.Semestre.RefSemestre.CodeSemestre == niveau);

            if (!string.IsNullOrEmpty(semaineType) && semaineType != "all")
                query = query.Where(c => c.Creneaux.Any(cr => cr.SemaineType == semaineType));

            return Ok(query.ToList());
        }

        // GET: api/EDT/ParSalle/{id}
        [HttpGet]
        [Route("ParSalle/{id:int}")]
        public IActionResult GetEDTParSalle(int id)
        {
            var cours = db.Cours
                .Where(c => c.IdSalle == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Classe)
                .Include(c => c.Creneaux)
                .ToList();
            return Ok(cours);
        }

        // GET: api/EDT/ParClasse/{id}
        [HttpGet]
        [Route("ParClasse/{id:int}")]
        public IActionResult GetEDTParClasse(int id)
        {
            var cours = db.Cours
                .Where(c => c.IdClasse == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .ToList();
            return Ok(cours);
        }

        // GET: api/EDT/ParProfesseur/{id}
        [HttpGet]
        [Route("ParProfesseur/{id:int}")]
        public IActionResult GetEDTParProfesseur(int id)
        {
            var cours = db.Cours
                .Where(c => c.IdProfesseur == id)
                .Include(c => c.Matiere)
                .Include(c => c.Classe)
                .Include(c => c.Salle)
                .Include(c => c.Creneaux)
                .ToList();
            return Ok(cours);
        }
    }
}
