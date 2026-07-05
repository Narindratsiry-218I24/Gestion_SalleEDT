using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Professeur")]
    [ApiController]
    public class ProfesseurController : ControllerBase
    {
        private readonly EMITDbContext db;
        private readonly IWebHostEnvironment _env;

        public ProfesseurController(EMITDbContext context, IWebHostEnvironment env)
        {
            db = context;
            _env = env;
        }

        // GET: api/Professeur
        [HttpGet]
        [Route("")]
        public IActionResult GetProfesseurs()
        {
            return Ok(db.Professeurs.ToList());
        }

        // GET: api/Professeur/5
        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetProfesseur(int id)
        {
            var professeur = db.Professeurs.Find(id);
            if (professeur == null) return NotFound();
            return Ok(professeur);
        }

        // POST: api/Professeur
        [HttpPost]
        [Route("")]
        public IActionResult CreateProfesseur([FromBody] Professeur professeur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Professeurs.Add(professeur);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetProfesseur), new { id = professeur.IdProfesseur }, professeur);
        }

        // POST: api/Professeur/upload
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> CreateProfesseurFromForm([FromForm] Professeur professeur, IFormFile photoFile, IFormFile cvFile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var allowedImageExt = new[] { ".jpg", ".jpeg", ".png" };
            var allowedCvExt = new[] { ".pdf" };
            const long maxFileBytes = 2 * 1024 * 1024;

            if (photoFile != null)
            {
                var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                if (!allowedImageExt.Contains(ext) || photoFile.Length > maxFileBytes)
                {
                    return BadRequest("Photo: types autorisés jpg,png et taille max 2MB.");
                }
            }

            if (cvFile != null)
            {
                var ext = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                if (!allowedCvExt.Contains(ext) || cvFile.Length > maxFileBytes)
                {
                    return BadRequest("CV: format PDF et taille max 2MB.");
                }
            }

            professeur.DateCreation = DateTime.UtcNow;
            db.Professeurs.Add(professeur);
            await db.SaveChangesAsync();

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "professeurs", professeur.IdProfesseur.ToString());
            Directory.CreateDirectory(uploadsRoot);

            if (photoFile != null)
            {
                var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                var fileName = "photo" + ext;
                var filePath = Path.Combine(uploadsRoot, fileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await photoFile.CopyToAsync(stream);
                }
                professeur.PhotoUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
            }

            if (cvFile != null)
            {
                var fileName = "cv" + Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                var filePath = Path.Combine(uploadsRoot, fileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await cvFile.CopyToAsync(stream);
                }
                professeur.CvUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
            }

            db.Entry(professeur).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfesseur), new { id = professeur.IdProfesseur }, professeur);
        }

        // PUT: api/Professeur/5
        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateProfesseur(int id, [FromBody] Professeur professeur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != professeur.IdProfesseur) return BadRequest();

            db.Entry(professeur).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        // DELETE: api/Professeur/5
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteProfesseur(int id)
        {
            var professeur = db.Professeurs.Find(id);
            if (professeur == null) return NotFound();

            db.Professeurs.Remove(professeur);
            db.SaveChanges();
            return Ok(professeur);
        }

        // GET: api/Professeur/5/Cours
        [HttpGet]
        [Route("{id:int}/Cours")]
        public IActionResult GetCoursDuProfesseur(int id)
        {
            var cours = db.Cours.Where(c => c.IdProfesseur == id)
                        .Include(c => c.Matiere)
                        .Include(c => c.Classe)
                        .ToList();
            return Ok(cours);
        }
    }
}
