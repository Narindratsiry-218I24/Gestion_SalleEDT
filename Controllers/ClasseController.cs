using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Classe")]
    [ApiController]
    public class ClasseController : ControllerBase
    {
        private readonly EMITDbContext db;

        public ClasseController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetClasses()
        {
            var query = db.Classes
                .Include(c => c.Filiere)
                .Include(c => c.AnneeAcademique)
                .AsQueryable();

            if (int.TryParse(Request.Query["anneeId"], out var anneeId))
                query = query.Where(c => c.IdAnneeAcademique == anneeId);

            var classes = query
                .OrderBy(c => c.AnneeAcademique.Libelle)
                .ThenBy(c => c.NomClasse)
                .ToList();
            return Ok(classes);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetClasse(int id)
        {
            var classe = db.Classes
                .Include(c => c.Filiere)
                .Include(c => c.AnneeAcademique)
                .Include(c => c.AffectationsMatieres)
                    .ThenInclude(a => a.Matiere)
                .Include(c => c.AffectationsMatieres)
                    .ThenInclude(a => a.Professeur)
                .Include(c => c.AffectationsMatieres)
                    .ThenInclude(a => a.Semestre)
                        .ThenInclude(s => s.RefSemestre)
                .FirstOrDefault(c => c.IdClasse == id);
            if (classe == null) return NotFound();
            return Ok(classe);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateClasse([FromBody] Classe classe)
        {
            ModelState.Remove("Filiere");
            ModelState.Remove("AnneeAcademique");
            ModelState.Remove("AffectationsMatieres");
            ModelState.Remove("Cours");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Classes.Add(classe);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetClasse), new { id = classe.IdClasse }, classe);
        }

        [HttpPost]
        [Route("CreerPourAnnee")]
        public IActionResult CreerPourAnnee([FromBody] CreateClasseDto dto)
        {
            var annee = db.AnneesAcademiques.Find(dto.IdAnneeAcademique);
            if (annee == null) return NotFound("Année académique introuvable");

            var filiere = db.Filieres.Find(dto.IdFiliere);
            if (filiere == null) return NotFound("Filière introuvable");

            var existe = db.Classes.Any(c => c.IdAnneeAcademique == dto.IdAnneeAcademique && c.CodeClasse == dto.CodeClasse);
            if (existe) return BadRequest("Cette classe existe déjà pour cette année");

            var classe = new Classe
            {
                IdFiliere = dto.IdFiliere,
                IdNiveau = dto.IdNiveau,
                IdAnneeAcademique = dto.IdAnneeAcademique,
                NomClasse = dto.NomClasse,
                CodeClasse = dto.CodeClasse,
                EstArchivee = false
            };

            db.Classes.Add(classe);
            db.SaveChanges();
            return Ok(classe);
        }

        public class CreateClasseDto
        {
            public int IdFiliere { get; set; }
            public int IdNiveau { get; set; }
            public int IdAnneeAcademique { get; set; }
            public string NomClasse { get; set; }
            public string CodeClasse { get; set; }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateClasse(int id, [FromBody] Classe classe)
        {
            if (id != classe.IdClasse) return BadRequest();
            ModelState.Remove("Filiere");
            ModelState.Remove("AnneeAcademique");
            ModelState.Remove("AffectationsMatieres");
            ModelState.Remove("Cours");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            db.Entry(classe).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteClasse(int id)
        {
            var classe = db.Classes.Find(id);
            if (classe == null) return NotFound();
            db.Classes.Remove(classe);
            db.SaveChanges();
            return Ok(classe);
        }

        [HttpGet]
        [Route("{id:int}/EmploiDuTemps")]
        public IActionResult GetEmploiDuTemps(int id)
        {
            var query = db.Cours
                .Where(c => c.IdClasse == id)
                .Include(c => c.Matiere)
                .Include(c => c.Professeur)
                .Include(c => c.Salle)
                .Include(c => c.Semestre)
                    .ThenInclude(s => s.RefSemestre)
                .Include(c => c.AffectationMatiere)
                .Include(c => c.Creneaux)
                .AsQueryable();

            if (int.TryParse(Request.Query["semestreId"], out var semestreId))
                query = query.Where(c => c.IdSemestre == semestreId);

            var cours = query.ToList();
            return Ok(cours);
        }

        [HttpGet]
        [Route("{id:int}/Statistiques")]
        public IActionResult GetStatistiques(int id)
        {
            var stats = db.AffectationsMatieres
                .Where(a => a.IdClasse == id)
                .Include(a => a.Semestre).ThenInclude(s => s.RefSemestre)
                .GroupBy(a => new
                {
                    a.IdSemestre,
                    CodeSemestre = a.Semestre.RefSemestre.CodeSemestre
                })
                .Select(g => new
                {
                    g.Key.IdSemestre,
                    g.Key.CodeSemestre,
                    Affectations = g.Count(),
                    VolumeTotal = g.Sum(a => a.VolumeHoraireTotal),
                    HeuresCm = g.Sum(a => a.HeuresCm),
                    HeuresTd = g.Sum(a => a.HeuresTd),
                    HeuresTp = g.Sum(a => a.HeuresTp)
                })
                .OrderBy(s => s.CodeSemestre)
                .ToList();

            return Ok(stats);
        }
    }
}
