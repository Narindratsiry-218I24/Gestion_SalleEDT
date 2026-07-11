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

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetMention(int id)
        {
            var mention = db.Mentions.Find(id);
            if (mention == null) return NotFound();
            return Ok(mention);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateMention([FromBody] Mention mention)
        {
            ModelState.Remove("Filieres");
            ModelState.Remove("Niveaux");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (mention.IdMention <= 0)
            {
                mention.IdMention = db.Mentions.Any() ? db.Mentions.Max(m => m.IdMention) + 1 : 1;
            }
            db.Mentions.Add(mention);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetMention), new { id = mention.IdMention }, mention);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateMention(int id, [FromBody] Mention mention)
        {
            if (id != mention.IdMention) return BadRequest();
            ModelState.Remove("Filieres");
            ModelState.Remove("Niveaux");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.Entry(mention).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteMention(int id)
        {
            var mention = db.Mentions.Find(id);
            if (mention == null) return NotFound();

            db.Mentions.Remove(mention);
            db.SaveChanges();
            return Ok(mention);
        }
    }
}
