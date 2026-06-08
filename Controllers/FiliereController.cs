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
    }
}