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

        // GET endpoint for specific Filiere details
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetFiliere(int id)
        {
            var filiere = db.Filieres
                .Include(f => f.Mention)
                .Select(f => new {
                    f.IdFiliere,
                    f.IdMention,
                    f.CodeFiliere,
                    f.NomFiliere,
                    MentionNom = f.Mention != null ? f.Mention.NomMention : null,
                    ClasseCount = db.Classes.Count(c => c.IdFiliere == f.IdFiliere),
                    MatiereCount = db.Matieres.Count(m => m.IdFiliere == f.IdFiliere)
                })
                .FirstOrDefault(f => f.IdFiliere == id);
            
            if (filiere == null)
                return NotFound();
            
            return Ok(filiere);
        }

        // PUT endpoint to update a Filiere
        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateFiliere(int id, [FromBody] Filiere filiereUpdate)
        {
            if (filiereUpdate == null || id != filiereUpdate.IdFiliere)
                return BadRequest("Invalid payload.");

            var filiere = db.Filieres.Find(id);
            if (filiere == null)
                return NotFound();

            filiere.IdMention = filiereUpdate.IdMention;
            filiere.CodeFiliere = filiereUpdate.CodeFiliere;
            filiere.NomFiliere = filiereUpdate.NomFiliere;

            db.SaveChanges();
            return NoContent();
        }

        // DELETE endpoint to remove a Filiere
        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteFiliere(int id)
        {
            var filiere = db.Filieres.Find(id);
            if (filiere == null)
                return NotFound();
            
            if (db.Classes.Any(c => c.IdFiliere == id) || db.Matieres.Any(m => m.IdFiliere == id))
            {
                return BadRequest(new { message = "Impossible de supprimer ce parcours car il contient des classes ou des matières." });
            }

            db.Filieres.Remove(filiere);
            db.SaveChanges();
            return NoContent();
        }
        [HttpPost]
        [Route("")]
        public IActionResult CreateFiliere([FromBody] Filiere filiere)
        {
            if (filiere == null)
                return BadRequest("Filiere payload is null.");

            // Remove navigation properties from validation if present
            ModelState.Remove("Mention");
            ModelState.Remove("Classes");
            ModelState.Remove("Matieres");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (filiere.IdFiliere == 0)
            {
                var maxId = db.Filieres.Select(f => (int?)f.IdFiliere).Max() ?? 0;
                filiere.IdFiliere = maxId + 1;
            }

            db.Filieres.Add(filiere);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetFilieres), new { id = filiere.IdFiliere }, filiere);
        }
    }
}