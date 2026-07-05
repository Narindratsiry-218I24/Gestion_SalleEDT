using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/AnneeAcademique")]
    [ApiController]
    public class AnneeAcademiqueController : ControllerBase
    {
        private readonly EMITDbContext db;

        public AnneeAcademiqueController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetAnnees()
        {
            return Ok(db.AnneesAcademiques
                .OrderByDescending(a => a.DateDebutAnnee)
                .ToList());
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetAnnee(int id)
        {
            var annee = db.AnneesAcademiques
                .Include(a => a.Semestres).ThenInclude(s => s.RefSemestre)
                .Include(a => a.Classes)
                .FirstOrDefault(a => a.IdAnnee == id);
            if (annee == null) return NotFound();
            return Ok(annee);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateAnnee([FromBody] AnneeAcademique annee)
        {
            ModelState.Remove("Semestres");
            ModelState.Remove("Classes");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.AnneesAcademiques.Add(annee);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetAnnee), new { id = annee.IdAnnee }, annee);
        }

        [HttpPost]
        [Route("{id:int}/CopierReglages/{sourceId:int}")]
        public IActionResult CopierReglages(int id, int sourceId)
        {
            if (id == sourceId) return BadRequest("L'annee source et l'annee cible doivent etre differentes.");

            var cible = db.AnneesAcademiques.Include(a => a.Semestres).FirstOrDefault(a => a.IdAnnee == id);
            var source = db.AnneesAcademiques
                .Include(a => a.Classes)
                .Include(a => a.Semestres)
                .FirstOrDefault(a => a.IdAnnee == sourceId);

            if (cible == null || source == null) return NotFound();

            foreach (var classeSource in source.Classes)
            {
                var existe = db.Classes.Any(c =>
                    c.IdAnneeAcademique == cible.IdAnnee &&
                    c.CodeClasse == classeSource.CodeClasse);

                if (!existe)
                {
                    db.Classes.Add(new Classe
                    {
                        IdFiliere = classeSource.IdFiliere,
                        IdAnneeAcademique = cible.IdAnnee,
                        NomClasse = classeSource.NomClasse,
                        CodeClasse = classeSource.CodeClasse
                    });
                }
            }

            foreach (var semestreSource in source.Semestres)
            {
                var existe = cible.Semestres.Any(s => s.IdRefSemestre == semestreSource.IdRefSemestre);
                if (!existe)
                {
                    db.Semestres.Add(new Semestre
                    {
                        IdAnnee = cible.IdAnnee,
                        IdRefSemestre = semestreSource.IdRefSemestre,
                        DateDebut = cible.DateDebutAnnee,
                        DateFin = cible.DateFinAnnee
                    });
                }
            }

            db.SaveChanges();
            return Ok(new { Message = "Reglages copies sans recopier les cours.", IdAnnee = id, SourceId = sourceId });
        }
    }
}
