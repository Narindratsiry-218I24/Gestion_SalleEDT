using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/AffectationMatiere")]
    [ApiController]
    public class AffectationMatiereController : ControllerBase
    {
        private readonly EMITDbContext db;
        private readonly IAffectationService _affectationService;

        public AffectationMatiereController(EMITDbContext context, IAffectationService affectationService)
        {
            db = context;
            _affectationService = affectationService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetAffectations(int? classeId = null, int? semestreId = null, bool actifsSeulement = true)
        {
            var query = db.AffectationsMatieres
                .Include(a => a.Classe).ThenInclude(c => c.AnneeAcademique)
                .Include(a => a.Matiere)
                .Include(a => a.Professeur)
                .Include(a => a.Semestre).ThenInclude(s => s.RefSemestre)
                .AsQueryable();

            if (classeId.HasValue) query = query.Where(a => a.IdClasse == classeId.Value);
            if (semestreId.HasValue) query = query.Where(a => a.IdSemestre == semestreId.Value);
            if (actifsSeulement) query = query.Where(a => a.EstActif);

            return Ok(query
                .OrderBy(a => a.Classe.NomClasse)
                .ThenBy(a => a.Semestre.RefSemestre.Ordre)
                .ThenBy(a => a.Matiere.NomMatiere)
                .ToList());
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetAffectation(int id)
        {
            var affectation = db.AffectationsMatieres
                .Include(a => a.Classe)
                .Include(a => a.Matiere)
                .Include(a => a.Professeur)
                .Include(a => a.Semestre).ThenInclude(s => s.RefSemestre)
                .FirstOrDefault(a => a.IdAffectation == id);

            if (affectation == null) return NotFound();
            return Ok(affectation);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateAffectation([FromBody] AffectationMatiere affectation)
        {
            if (affectation == null) return BadRequest("Les donnees de l'affectation sont vides.");

            ModelState.Remove("Classe");
            ModelState.Remove("Matiere");
            ModelState.Remove("Professeur");
            ModelState.Remove("Semestre");
            ModelState.Remove("Cours");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var classe = db.Classes.Find(affectation.IdClasse);
            var matiere = db.Matieres.Find(affectation.IdMatiere);
            var semestre = db.Semestres.Find(affectation.IdSemestre);

            if (classe == null) return BadRequest("Classe introuvable.");
            if (matiere == null) return BadRequest("Matiere introuvable.");
            if (semestre == null) return BadRequest("Semestre introuvable.");
            if (semestre.IdAnnee != classe.IdAnneeAcademique)
                return BadRequest("Le semestre doit appartenir a l'annee academique de la classe.");
            if (matiere.IdFiliere != classe.IdFiliere)
                return BadRequest("La matiere doit appartenir a la filiere de la classe.");
            if (affectation.VolumeHoraireTotal <= 0)
                return BadRequest("Le volume horaire total doit etre positif.");

            var existePourCeProf = db.AffectationsMatieres.Any(a =>
                a.IdClasse == affectation.IdClasse &&
                a.IdMatiere == affectation.IdMatiere &&
                a.IdProfesseur == affectation.IdProfesseur &&
                a.IdSemestre == affectation.IdSemestre &&
                a.EstActif);

            if (existePourCeProf) return BadRequest("Cette affectation existe déjà pour ce professeur.");

            var existePourAutreProf = db.AffectationsMatieres.Any(a =>
                a.IdClasse == affectation.IdClasse &&
                a.IdMatiere == affectation.IdMatiere &&
                a.IdSemestre == affectation.IdSemestre &&
                a.EstActif);

            if (existePourAutreProf) return BadRequest("Cette matière est déjà affectée à un autre professeur pour cette classe.");

            db.AffectationsMatieres.Add(affectation);
            db.SaveChanges();

            return CreatedAtAction(nameof(GetAffectation), new { id = affectation.IdAffectation }, affectation);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateAffectation(int id, [FromBody] AffectationMatiere affectation)
        {
            if (id != affectation.IdAffectation) return BadRequest();

            ModelState.Remove("Classe");
            ModelState.Remove("Matiere");
            ModelState.Remove("Professeur");
            ModelState.Remove("Semestre");
            ModelState.Remove("Cours");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            db.Entry(affectation).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteAffectation(int id)
        {
            var affectation = db.AffectationsMatieres.Find(id);
            if (affectation == null) return NotFound();

            affectation.EstActif = false;
            db.SaveChanges();
            return Ok(affectation);
        }

        [HttpPost]
        [Route("{id:int}/GenererCours")]
        public async Task<IActionResult> GenererCours(int id)
        {
            // Délégué à IAffectationService (Phase 1.7) pour éviter la duplication avec MatiereController
            var result = await _affectationService.GenererCoursDepuisAffectationAsync(id);

            if (!result.Success)
                return BadRequest(new { Message = result.Message });

            var cours = await db.Cours
                .Where(c => result.CoursIds.Contains(c.IdCours))
                .ToListAsync();

            return Ok(cours);
        }

        [HttpGet]
        [Route("ParProfesseur/{professeurId:int}")]
        public IActionResult GetAffectationsParProfesseur(int professeurId, int? anneeId = null)
        {
            var query = db.AffectationsMatieres
                .Include(a => a.Classe)
                    .ThenInclude(c => c.AnneeAcademique)
                .Include(a => a.Classe)
                    .ThenInclude(c => c.Filiere)
                .Include(a => a.Matiere)
                .Include(a => a.Semestre)
                    .ThenInclude(s => s.RefSemestre)
                .Where(a => a.IdProfesseur == professeurId && a.EstActif)
                .AsQueryable();

            if (anneeId.HasValue)
            {
                query = query.Where(a => a.Classe.IdAnneeAcademique == anneeId.Value);
            }

            var result = query.Select(a => new
            {
                a.IdAffectation,
                Classe = a.Classe.NomClasse,
                Filiere = a.Classe.Filiere.NomFiliere,
                Matiere = a.Matiere.NomMatiere,
                Semestre = a.Semestre.RefSemestre.CodeSemestre,
                Annee = a.Classe.AnneeAcademique.Libelle,
                a.VolumeHoraireTotal,
                a.HeuresCm,
                a.HeuresTd,
                a.HeuresTp,
                a.EstActif
            }).ToList();

            return Ok(result);
        }
    }
}
