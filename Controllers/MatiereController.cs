using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Matiere")]
    [ApiController]
    public class MatiereController : ControllerBase
    {
        private readonly EMITDbContext db;

        public MatiereController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetMatieres()
        {
            var matieres = db.Matieres.Include(m => m.Filiere).Include(m => m.RefSemestre).ToList();
            return Ok(matieres);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetMatiere(int id)
        {
            var matiere = db.Matieres.Include(m => m.Filiere).Include(m => m.RefSemestre)
                            .FirstOrDefault(m => m.IdMatiere == id);
            if (matiere == null) return NotFound();
            return Ok(matiere);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateMatiere([FromBody] Matiere matiere)
        {
            if (matiere == null) return BadRequest("Les données de la matière sont vides.");
            
            ModelState.Remove("Filiere");
            ModelState.Remove("RefSemestre");
            ModelState.Remove("ProfesseurResponsable");
            ModelState.Remove("Cours");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (matiere.IdMatiere == 0)
                {
                    var maxId = db.Matieres.Select(m => (int?)m.IdMatiere).Max();
                    matiere.IdMatiere = (maxId ?? 0) + 1;
                }

                db.Matieres.Add(matiere);
                db.SaveChanges();
                return CreatedAtAction(nameof(GetMatiere), new { id = matiere.IdMatiere }, matiere);
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                var innerInnerMsg = ex.InnerException?.InnerException != null ? ex.InnerException.InnerException.Message : "";
                return BadRequest($"Erreur DB: {ex.Message} | Inner: {innerMsg} | Details: {innerInnerMsg}");
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateMatiere(int id, [FromBody] Matiere matiere)
        {
            if (id != matiere.IdMatiere) return BadRequest();
            db.Entry(matiere).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteMatiere(int id)
        {
            var matiere = db.Matieres.Find(id);
            if (matiere == null) return NotFound();
            db.Matieres.Remove(matiere);
            db.SaveChanges();
            return Ok(matiere);
        }
    }
}
