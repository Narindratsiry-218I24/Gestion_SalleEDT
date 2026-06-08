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
    }
}
