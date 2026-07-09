using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Filiere")]
    [ApiController]
    public class FiliereController : ControllerBase
    {
        private readonly EMITDbContext db;

        public FiliereController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetFilieres()
        {
            var filieres = db.Filieres.Include(f => f.Mention).ToList();
            return Ok(filieres);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetFiliere(int id)
        {
            var filiere = db.Filieres.Include(f => f.Mention).FirstOrDefault(f => f.IdFiliere == id);
            if (filiere == null) return NotFound();
            return Ok(filiere);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateFiliere([FromBody] Filiere filiere)
        {
            ModelState.Remove("Mention");
            ModelState.Remove("Classes");
            ModelState.Remove("Matieres");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (filiere.IdFiliere <= 0)
            {
                filiere.IdFiliere = db.Filieres.Any() ? db.Filieres.Max(f => f.IdFiliere) + 1 : 1;
            }
            db.Filieres.Add(filiere);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetFiliere), new { id = filiere.IdFiliere }, filiere);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateFiliere(int id, [FromBody] Filiere filiere)
        {
            if (id != filiere.IdFiliere) return BadRequest();
            ModelState.Remove("Mention");
            ModelState.Remove("Classes");
            ModelState.Remove("Matieres");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.Entry(filiere).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteFiliere(int id)
        {
            var filiere = db.Filieres.Find(id);
            if (filiere == null) return NotFound();

            db.Filieres.Remove(filiere);
            db.SaveChanges();
            return Ok(filiere);
        }
    }
}