using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Semestre")]
    [ApiController]
    public class SemestreController : ControllerBase
    {
        private readonly EMITDbContext db;

        public SemestreController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetSemestres(int? anneeId = null)
        {
            var query = db.Semestres
                .Include(s => s.RefSemestre).ThenInclude(r => r.Niveau)
                .Include(s => s.AnneeAcademique)
                .AsQueryable();

            if (anneeId.HasValue) query = query.Where(s => s.IdAnnee == anneeId.Value);

            return Ok(query
                .OrderBy(s => s.AnneeAcademique.Libelle)
                .ThenBy(s => s.RefSemestre.Ordre)
                .ToList());
        }

        [HttpGet]
        [Route("RefSemestre")]
        public IActionResult GetRefSemestres()
        {
            return Ok(db.RefSemestres
                .Include(r => r.Niveau)
                .OrderBy(r => r.Niveau.Ordre)
                .ThenBy(r => r.Ordre)
                .ToList());
        }
    }
}
