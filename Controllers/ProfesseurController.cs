using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Professeur")]
    [ApiController]
    public class ProfesseurController : ControllerBase
    {
        private readonly EMITDbContext db;

        public ProfesseurController(EMITDbContext context)
        {
            db = context;
        }

        // GET: api/Professeur
        [HttpGet]
        [Route("")]
        public IActionResult GetProfesseurs()
        {
            return Ok(db.Professeurs.ToList());
        }

        // GET: api/Professeur/5
        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetProfesseur(int id)
        {
            var professeur = db.Professeurs.Find(id);
            if (professeur == null) return NotFound();
            return Ok(professeur);
        }

        // POST: api/Professeur
        [HttpPost]
        [Route("")]
        public IActionResult CreateProfesseur([FromBody] Professeur professeur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Professeurs.Add(professeur);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetProfesseur), new { id = professeur.IdProfesseur }, professeur);
        }

        // PUT: api/Professeur/5
        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateProfesseur(int id, [FromBody] Professeur professeur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != professeur.IdProfesseur) return BadRequest();

            db.Entry(professeur).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        // DELETE: api/Professeur/5
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteProfesseur(int id)
        {
            var professeur = db.Professeurs.Find(id);
            if (professeur == null) return NotFound();

            db.Professeurs.Remove(professeur);
            db.SaveChanges();
            return Ok(professeur);
        }

        // GET: api/Professeur/5/Cours
        [HttpGet]
        [Route("{id:int}/Cours")]
        public IActionResult GetCoursDuProfesseur(int id)
        {
            var cours = db.Cours.Where(c => c.IdProfesseur == id)
                        .Include(c => c.Matiere)
                        .Include(c => c.Classe)
                        .ToList();
            return Ok(cours);
        }
    }
}
