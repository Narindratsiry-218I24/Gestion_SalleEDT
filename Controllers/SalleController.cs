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

        [HttpGet]
        [Route("Disponibles")]
        public IActionResult GetSallesDisponibles([FromQuery] string date, [FromQuery] string startTime, [FromQuery] string endTime, [FromQuery] string? batiment = null, [FromQuery] string? type = null, [FromQuery] int? etage = null)
        {
            try
            {
                // Force parsing using ISO format
                if (!DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                    return BadRequest($"Invalid date format: {date}");
                
                if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
                    return BadRequest($"Invalid time format: {startTime} or {endTime}");

                var query = db.Salles.AsQueryable();
                if (!string.IsNullOrWhiteSpace(batiment) && batiment.ToLower() != "all")
                    query = query.Where(s => s.CodeBatiment == batiment);
                if (!string.IsNullOrWhiteSpace(type) && type.ToLower() != "all")
                    query = query.Where(s => s.TypeSalle == type);
                if (etage.HasValue)
                    query = query.Where(s => s.Etage == etage.Value);

                var allSalles = query.ToList();

                // Convert Date properly to match DB format if necessary
                var dDate = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

                // Find overlapping sessions
                var overlappingSessions = db.Seances
                    .Where(s => s.Date.Date == dDate && s.Statut != "Annulee")
                    .Where(s => (start >= s.StartTime && start < s.EndTime) || 
                                (end > s.StartTime && end <= s.EndTime) || 
                                (start <= s.StartTime && end >= s.EndTime))
                    .Select(s => s.IdSalle)
                    .Distinct()
                    .ToList();

                var availableSalles = allSalles.Where(s => !overlappingSessions.Contains(s.IdSalle)).ToList();

                var result = availableSalles.Select(s => new {
                    IdSalle = s.IdSalle,
                    NomSalle = s.NomSalle,
                    Capacite = s.Capacite,
                    CodeBatiment = s.CodeBatiment,
                    Etage = s.Etage,
                    TypeSalle = s.TypeSalle
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException != null) msg += " | Inner: " + ex.InnerException.Message;
                return StatusCode(500, msg);
            }
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

        // Old API removed to avoid conflicts

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
