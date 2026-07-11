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

        // POST endpoint to create a Mention
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
