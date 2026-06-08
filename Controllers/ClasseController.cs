using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Classe")]
    [ApiController]
    public class ClasseController : ControllerBase
    {
        private readonly EMITDbContext db;

        public ClasseController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetClasses()
        {
            var classes = db.Classes
                .Include(c => c.Filiere)
                .Include(c => c.Semestre.RefSemestre)
                .ToList();
            return Ok(classes);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetClasse(int id)
        {
            var classe = db.Classes
                .Include(c => c.Filiere)
                .Include(c => c.Semestre)
                .FirstOrDefault(c => c.IdClasse == id);
            if (classe == null) return NotFound();
            return Ok(classe);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateClasse([FromBody] Classe classe)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Classes.Add(classe);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetClasse), new { id = classe.IdClasse }, classe);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateClasse(int id, [FromBody] Classe classe)
        {
            if (id != classe.IdClasse) return BadRequest();
            db.Entry(classe).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteClasse(int id)
        {
            var classe = db.Classes.Find(id);
            if (classe == null) return NotFound();
            db.Classes.Remove(classe);
            db.SaveChanges();
            return Ok(classe);
        }

        [HttpGet]
        [Route("{id:int}/EmploiDuTemps")]
        public IActionResult GetEmploiDuTemps(int id)
        {
            var cours = db.Cours
                .Where(c => c.IdClasse == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Salle)
                .ToList();
            return Ok(cours);
        }
    }
}