using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Mention")]
    [ApiController]
    public class MentionController : ControllerBase
    {
        private readonly EMITDbContext db;

        public MentionController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetMentions()
        {
            var mentions = db.Mentions.ToList();
            return Ok(mentions);
        }

        // GET endpoint for specific Mention details
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetMention(int id)
        {
            var mention = db.Mentions
                .Select(m => new {
                    m.IdMention,
                    m.CodeMention,
                    m.NomMention,
                    FiliereCount = db.Filieres.Count(f => f.IdMention == m.IdMention),
                    NiveauCount = db.Niveaux.Count(n => n.IdMention == m.IdMention)
                })
                .FirstOrDefault(m => m.IdMention == id);
            
            if (mention == null)
                return NotFound();
            
            return Ok(mention);
        }

        // PUT endpoint to update a Mention
        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateMention(int id, [FromBody] Mention mentionUpdate)
        {
            if (mentionUpdate == null || id != mentionUpdate.IdMention)
                return BadRequest("Invalid payload.");

            var mention = db.Mentions.Find(id);
            if (mention == null)
                return NotFound();

            mention.CodeMention = mentionUpdate.CodeMention;
            mention.NomMention = mentionUpdate.NomMention;

            db.SaveChanges();
            return NoContent();
        }

        // DELETE endpoint to remove a Mention
        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteMention(int id)
        {
            var mention = db.Mentions.Find(id);
            if (mention == null)
                return NotFound();
            
            if (db.Filieres.Any(f => f.IdMention == id) || db.Niveaux.Any(n => n.IdMention == id))
            {
                return BadRequest(new { message = "Impossible de supprimer cette mention car elle contient des filières ou des niveaux." });
            }

            db.Mentions.Remove(mention);
            db.SaveChanges();
            return NoContent();
        }
        [HttpPost]
        [Route("")]
        public IActionResult CreateMention([FromBody] Mention mention)
        {
            if (mention == null)
                return BadRequest("Mention payload is null.");

            // Remove navigation properties from validation if present
            ModelState.Remove("Filieres");
            ModelState.Remove("Niveaux");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (mention.IdMention == 0)
            {
                var maxId = db.Mentions.Select(m => (int?)m.IdMention).Max() ?? 0;
                mention.IdMention = maxId + 1;
            }

            db.Mentions.Add(mention);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetMentions), new { id = mention.IdMention }, mention);
        }
    }
}
