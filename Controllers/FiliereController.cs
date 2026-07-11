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

        // POST endpoint to create a Filiere
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