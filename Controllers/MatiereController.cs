using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Matiere")]
    [ApiController]
    public class MatiereController : ControllerBase
    {
        private readonly EMITDbContext db;

        public MatiereController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetMatieres()
        {
            var matieres = db.Matieres.Include(m => m.Filiere).Include(m => m.RefSemestre).ToList();
            return Ok(matieres);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetMatiere(int id)
        {
            var matiere = db.Matieres.Include(m => m.Filiere).Include(m => m.RefSemestre)
                            .FirstOrDefault(m => m.IdMatiere == id);
            if (matiere == null) return NotFound();
            return Ok(matiere);
        }

        [HttpGet]
        [Route("Niveaux")]
        public IActionResult GetNiveaux()
        {
            var niveaux = db.Niveaux.OrderBy(n => n.Ordre).ToList();
            return Ok(niveaux);
        }

        [HttpGet]
        [Route("Filieres")]
        public IActionResult GetFilieres([FromQuery] int niveauId)
        {
            var niveau = db.Niveaux.Find(niveauId);
            if (niveau == null) return NotFound();

            var filieres = db.Filieres
                .Where(f => f.IdMention == niveau.IdMention)
                .OrderBy(f => f.NomFiliere)
                .ToList();
            return Ok(filieres);
        }

        [HttpGet]
        [Route("RefSemestres")]
        public IActionResult GetRefSemestres()
        {
            var semestres = db.RefSemestres.Include(r => r.Niveau).ToList();
            return Ok(semestres);
        }

        [HttpGet]
        [Route("NonPlanifiees")]
        public IActionResult GetMatieresNonPlanifiees([FromQuery] int filiereId, [FromQuery] int niveauId)
        {
            // Trouver les classes associées à la filière et niveau
            var classes = db.Classes
                .Where(c => c.IdFiliere == filiereId && c.IdNiveau == niveauId)
                .Select(c => c.IdClasse)
                .ToList();

            if (!classes.Any()) return Ok(new List<object>());

            // Trouver les affectations de matières pour ces classes
            var affectations = db.AffectationsMatieres
                .Include(a => a.Matiere)
                .Include(a => a.Professeur)
                .Where(a => classes.Contains(a.IdClasse) && a.EstActif)
                .ToList();

            // Trouver les cours existants
            var existingCoursAffectations = db.Cours
                .Where(c => c.IdAffectation != null)
                .Select(c => c.IdAffectation)
                .Distinct()
                .ToList();

            var results = affectations
                .Where(a => !existingCoursAffectations.Contains(a.IdAffectation))
                .Select(a => new
                {
                    a.IdAffectation,
                    a.Matiere.IdMatiere,
                    a.Matiere.NomMatiere,
                    a.Matiere.CodeMatiere,
                    ProfesseurNom = a.Professeur != null ? $"{a.Professeur.Prenom} {a.Professeur.Nom}" : "Non assigné",
                    a.IdProfesseur,
                    a.IdClasse,
                    a.IdSemestre,
                    a.HeuresCm,
                    a.HeuresTd,
                    a.HeuresTp
                })
                .ToList();

            return Ok(results);
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateMatiere([FromBody] Matiere matiere)
        {
            if (matiere == null) return BadRequest("Les données de la matière sont vides.");
            
            ModelState.Remove("Filiere");
            ModelState.Remove("RefSemestre");
            ModelState.Remove("ProfesseurResponsable");
            ModelState.Remove("Cours");
            ModelState.Remove("AffectationsMatieres");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (matiere.IdMatiere == 0)
                {
                    var maxId = db.Matieres.Select(m => (int?)m.IdMatiere).Max();
                    matiere.IdMatiere = (maxId ?? 0) + 1;
                }

                db.Matieres.Add(matiere);
                db.SaveChanges();

                // Si un enseignant est assigné lors de la création
                if (matiere.IdProfesseurResponsable.HasValue)
                {
                    var refSemestre = db.RefSemestres.Find(matiere.IdRefSemestre);
                    if (refSemestre != null)
                    {
                        var semestre = db.Semestres.FirstOrDefault(s => s.IdRefSemestre == matiere.IdRefSemestre && !s.EstArchivee);
                        var classe = db.Classes.FirstOrDefault(c => c.IdFiliere == matiere.IdFiliere && c.IdNiveau == refSemestre.IdNiveau && !c.EstArchivee);

                        if (semestre != null && classe != null)
                        {
                            var maxAffId = db.AffectationsMatieres.Select(a => (int?)a.IdAffectation).Max() ?? 0;
                            var affectation = new AffectationMatiere
                            {
                                IdAffectation = maxAffId + 1,
                                IdMatiere = matiere.IdMatiere,
                                IdProfesseur = matiere.IdProfesseurResponsable.Value,
                                IdClasse = classe.IdClasse,
                                IdSemestre = semestre.IdSemestre,
                                VolumeHoraireTotal = matiere.VolumeHoraire,
                                HeuresCm = matiere.VolumeHoraire, // Tout est simple "Cours"
                                DateDebut = semestre.DateDebut,
                                DateFin = semestre.DateFin,
                                EstActif = true
                            };
                            db.AffectationsMatieres.Add(affectation);
                            db.SaveChanges();

                            var maxCoursId = db.Cours.Select(c => (int?)c.IdCours).Max() ?? 0;
                            var cours = new Cours
                            {
                                IdCours = maxCoursId + 1,
                                IdMatiere = matiere.IdMatiere,
                                IdProfesseur = matiere.IdProfesseurResponsable.Value,
                                IdClasse = classe.IdClasse,
                                IdSemestre = semestre.IdSemestre,
                                IdAffectation = affectation.IdAffectation,
                                TypeCours = "Cours",
                                VolumeHours = matiere.VolumeHoraire,
                                Statut = "Cree"
                            };
                            db.Cours.Add(cours);
                            db.SaveChanges();
                        }
                    }
                }

                return CreatedAtAction(nameof(GetMatiere), new { id = matiere.IdMatiere }, matiere);
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                var innerInnerMsg = ex.InnerException?.InnerException != null ? ex.InnerException.InnerException.Message : "";
                return BadRequest($"Erreur DB: {ex.Message} | Inner: {innerMsg} | Details: {innerInnerMsg}");
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateMatiere(int id, [FromBody] Matiere matiere)
        {
            if (id != matiere.IdMatiere) return BadRequest();
            db.Entry(matiere).State = EntityState.Modified;
            db.SaveChanges();
            return NoContent();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteMatiere(int id)
        {
            var matiere = db.Matieres.Find(id);
            if (matiere == null) return NotFound();
            db.Matieres.Remove(matiere);
            db.SaveChanges();
            return Ok(matiere);
        }
    }
}
