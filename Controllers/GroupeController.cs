using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Groupe")]
    [ApiController]
    public class GroupeController : ControllerBase
    {
        private readonly EMITDbContext db;

        public GroupeController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet("ByClasse/{classeId:int}")]
        public IActionResult ByClasse(int classeId)
        {
            return Ok(db.Groupes.Where(g => g.ClasseId == classeId).ToList());
        }
    }
}
