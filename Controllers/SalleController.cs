using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Salle")]
    [ApiController]
    public class SalleController : ControllerBase
    {
        private readonly EMITDbContext db;

        public SalleController(EMITDbContext context)
        {
            db = context;
        }

        // GET: api/Salle?batiment=A&type=cours&etage=1
        [HttpGet]
        [Route("")]
        public IActionResult GetSalles([FromQuery] string? batiment = null, [FromQuery] string? type = null, [FromQuery] int? etage = null)
        {
            var query = db.Salles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(batiment) && batiment.ToLower() != "all")
                query = query.Where(s => s.CodeBatiment == batiment);
            if (!string.IsNullOrWhiteSpace(type) && type.ToLower() != "all")
                query = query.Where(s => s.TypeSalle == type);
            if (etage.HasValue)
                query = query.Where(s => s.Etage == etage.Value);
            return Ok(query.ToList());
        }

        // GET: api/Salle/5
        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetSalle(int id)
        {
            var salle = db.Salles.Find(id);
            if (salle == null) return NotFound();
            return Ok(salle);
        }

        // POST: api/Salle
        [HttpPost]
        [Route("")]
        public IActionResult CreateSalle([FromBody] Salle salle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (salle.IdSalle <= 0)
            {
                salle.IdSalle = db.Salles.Any() ? db.Salles.Max(s => s.IdSalle) + 1 : 1;
            }

            db.Salles.Add(salle);
            db.SaveChanges();
            return Ok(salle);
        }

        // PUT: api/Salle/5
        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateSalle(int id, [FromBody] Salle salle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != salle.IdSalle) return BadRequest();

            db.Entry(salle).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        // DELETE: api/Salle/5
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteSalle(int id)
        {
            var salle = db.Salles.Find(id);
            if (salle == null) return NotFound();

            db.Salles.Remove(salle);
            db.SaveChanges();
            return Ok(salle);
        }

        // GET: api/Salle/Disponibles
        [HttpGet]
        [Route("Disponibles")]
        public IActionResult GetSallesDisponibles()
        {
            return Ok(db.Salles.ToList());
        }

        // GET: api/Salle/ParType/{type}
        [HttpGet]
        [Route("ParType/{type}")]
        public IActionResult GetSallesParType(string type)
        {
            var salles = db.Salles.Where(s => s.TypeSalle == type).ToList();
            return Ok(salles);
        }
    }
}
