using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Niveau")]
    [ApiController]
    public class NiveauController : ControllerBase
    {
        private readonly EMITDbContext db;

        public NiveauController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetNiveaux()
        {
            var niveaux = db.Niveaux.Include(n => n.Mention).ToList();
            return Ok(niveaux);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetNiveau(int id)
        {
            var niveau = db.Niveaux.Include(n => n.Mention).FirstOrDefault(n => n.IdNiveau == id);
            if (niveau == null) return NotFound();
            return Ok(niveau);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateNiveau([FromBody] Niveau niveau)
        {
            ModelState.Remove("Mention");
            ModelState.Remove("RefSemestres");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.Niveaux.Add(niveau);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetNiveau), new { id = niveau.IdNiveau }, niveau);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateNiveau(int id, [FromBody] Niveau niveau)
        {
            if (id != niveau.IdNiveau) return BadRequest();
            ModelState.Remove("Mention");
            ModelState.Remove("RefSemestres");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.Entry(niveau).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteNiveau(int id)
        {
            var niveau = db.Niveaux.Find(id);
            if (niveau == null) return NotFound();

            db.Niveaux.Remove(niveau);
            db.SaveChanges();
            return Ok(niveau);
        }
    }
}
