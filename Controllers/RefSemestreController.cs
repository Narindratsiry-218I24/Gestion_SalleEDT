using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/RefSemestre")]
    [ApiController]
    public class RefSemestreController : ControllerBase
    {
        private readonly EMITDbContext db;

        public RefSemestreController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(db.RefSemestres
                .Include(r => r.Niveau)
                .OrderBy(r => r.Niveau.Ordre)
                .ThenBy(r => r.Ordre)
                .ToList());
        }
    }
}
