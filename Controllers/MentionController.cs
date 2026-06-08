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
    }
}
