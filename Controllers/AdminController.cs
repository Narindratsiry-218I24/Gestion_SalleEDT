using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    public class AdminController : Controller
    {
        private readonly EMITDbContext _db;

        public AdminController(EMITDbContext context)
        {
            _db = context;
        }

        [HttpGet]
        public IActionResult AnneeAcademique()
        {
            var model = new AdminAnneeViewModel
            {
                Create = new CreateAnneeVm { DateDebut = DateTime.Today, DateFin = DateTime.Today.AddMonths(9), EstActive = false },
                Annees = _db.AnneesAcademiques
                    .Include(a => a.Semestres).ThenInclude(s => s.RefSemestre)
                    .Include(a => a.Classes)
                    .OrderByDescending(a => a.DateDebutAnnee)
                    .ToList()
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Classes(int? anneeId)
        {
            var annees = _db.AnneesAcademiques.OrderByDescending(a => a.DateDebutAnnee).ToList();
            var filieres = _db.Filieres.OrderBy(f => f.NomFiliere).ToList();
            var classesQuery = _db.Classes.Include(c => c.Filiere).Include(c => c.AnneeAcademique).AsQueryable();
            if (anneeId.HasValue) classesQuery = classesQuery.Where(c => c.IdAnneeAcademique == anneeId.Value);

            var model = new AdminAnneeViewModel
            {
                Annees = annees,
                Filieres = filieres,
                Classes = classesQuery.OrderBy(c => c.NomClasse).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreerClasse(int idFiliere, int idAnnee, string nomClasse, string codeClasse)
        {
            if (string.IsNullOrWhiteSpace(nomClasse) || string.IsNullOrWhiteSpace(codeClasse))
            {
                TempData["Error"] = "Nom et code requis.";
                return RedirectToAction(nameof(Classes));
            }

            var existe = _db.Classes.Any(c => c.IdAnneeAcademique == idAnnee && c.CodeClasse == codeClasse);
            if (existe)
            {
                TempData["Error"] = "Cette classe existe déjà pour cette année.";
                return RedirectToAction(nameof(Classes));
            }

            var classe = new Classe
            {
                IdFiliere = idFiliere,
                IdAnneeAcademique = idAnnee,
                NomClasse = nomClasse,
                CodeClasse = codeClasse,
                EstArchivee = false
            };
            _db.Classes.Add(classe);
            _db.SaveChanges();
            TempData["Success"] = "Classe créée.";
            return RedirectToAction(nameof(Classes), new { anneeId = idAnnee });
        }

        [HttpGet]
        public IActionResult Affectations(int? anneeId)
        {
            var annees = _db.AnneesAcademiques.OrderByDescending(a => a.DateDebutAnnee).ToList();
            var profs = _db.Professeurs.OrderBy(p => p.Nom).ThenBy(p => p.Prenom).ToList();
            var matieres = _db.Matieres.OrderBy(m => m.NomMatiere).ToList();
            var semestres = _db.Semestres.Include(s => s.RefSemestre).ToList();
            var classes = _db.Classes.Include(c => c.Filiere).Include(c => c.AnneeAcademique).ToList();

            var affectationsQuery = _db.AffectationsMatieres
                .Include(a => a.Classe).ThenInclude(c => c.Filiere)
                .Include(a => a.Matiere)
                .Include(a => a.Professeur)
                .Include(a => a.Semestre).ThenInclude(s => s.RefSemestre)
                .AsQueryable();
            if (anneeId.HasValue)
                affectationsQuery = affectationsQuery.Where(a => a.Classe.IdAnneeAcademique == anneeId.Value);

            var model = new AdminAnneeViewModel
            {
                Annees = annees,
                Professeurs = profs,
                Matieres = matieres,
                Semestres = semestres,
                Classes = classes,
            };

            ViewBag.Affectations = affectationsQuery.OrderBy(a => a.Classe.NomClasse).ThenBy(a => a.Semestre.IdSemestre).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreerAffectation(int idClasse, int idMatiere, int idProfesseur, int idSemestre, int volumeHoraireTotal, int heuresCm = 0, int heuresTd = 0, int heuresTp = 0, string commentaire = null)
        {
            if (volumeHoraireTotal <= 0)
            {
                TempData["Error"] = "Volume horaire total doit être positif.";
                return RedirectToAction(nameof(Affectations));
            }

            var existe = _db.AffectationsMatieres.Any(a => a.IdClasse == idClasse && a.IdMatiere == idMatiere && a.IdProfesseur == idProfesseur && a.IdSemestre == idSemestre);
            if (existe)
            {
                TempData["Error"] = "Cette affectation existe déjà.";
                return RedirectToAction(nameof(Affectations));
            }

            var affectation = new AffectationMatiere
            {
                IdClasse = idClasse,
                IdMatiere = idMatiere,
                IdProfesseur = idProfesseur,
                IdSemestre = idSemestre,
                VolumeHoraireTotal = volumeHoraireTotal,
                HeuresCm = heuresCm,
                HeuresTd = heuresTd,
                HeuresTp = heuresTp,
                DateDebut = DateTime.UtcNow,
                DateFin = DateTime.UtcNow.AddMonths(1),
                EstActif = true,
                Commentaire = commentaire
            };
            _db.AffectationsMatieres.Add(affectation);
            _db.SaveChanges();
            TempData["Success"] = "Affectation créée.";
            return RedirectToAction(nameof(Affectations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreerAnnee(CreateAnneeVm dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Libelle))
            {
                TempData["Error"] = "Libellé requis.";
                return RedirectToAction(nameof(AnneeAcademique));
            }

            var annee = new AnneeAcademique
            {
                Libelle = dto.Libelle,
                DateDebutAnnee = dto.DateDebut,
                DateFinAnnee = dto.DateFin,
                EstActive = dto.EstActive,
                EstArchivee = false
            };

            _db.AnneesAcademiques.Add(annee);
            _db.SaveChanges();

            var refSemestres = _db.RefSemestres.OrderBy(r => r.Ordre).ToList();
            if (!refSemestres.Any())
            {
                var s1 = new RefSemestre { CodeSemestre = "S1", Ordre = 1, IdNiveau = 1 };
                var s2 = new RefSemestre { CodeSemestre = "S2", Ordre = 2, IdNiveau = 1 };
                _db.RefSemestres.AddRange(s1, s2);
                _db.SaveChanges();
                refSemestres = _db.RefSemestres.OrderBy(r => r.Ordre).ToList();
            }

            foreach (var refSemestre in refSemestres)
            {
                var semestre = new Semestre
                {
                    IdRefSemestre = refSemestre.IdRefSemestre,
                    IdAnnee = annee.IdAnnee,
                    DateDebut = dto.DateDebut,
                    DateFin = dto.DateFin,
                    EstArchivee = false
                };
                _db.Semestres.Add(semestre);
            }
            _db.SaveChanges();

            TempData["Success"] = "Année créée avec ses semestres.";
            return RedirectToAction(nameof(AnneeAcademique));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Archiver(int id)
        {
            var annee = _db.AnneesAcademiques.Find(id);
            if (annee == null) return NotFound();
            annee.EstArchivee = true;
            annee.DateArchivage = DateTime.UtcNow;
            _db.SaveChanges();
            TempData["Success"] = "Année archivée.";
            return RedirectToAction(nameof(AnneeAcademique));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restaurer(int id)
        {
            var annee = _db.AnneesAcademiques.Find(id);
            if (annee == null) return NotFound();
            annee.EstArchivee = false;
            annee.DateArchivage = null;
            _db.SaveChanges();
            TempData["Success"] = "Année restaurée.";
            return RedirectToAction(nameof(AnneeAcademique));
        }

        public class CreateAnneeVm
        {
            public string Libelle { get; set; }
            public DateTime DateDebut { get; set; }
            public DateTime DateFin { get; set; }
            public bool EstActive { get; set; }
        }

        public class AdminAnneeViewModel
        {
            public CreateAnneeVm Create { get; set; }
            public System.Collections.Generic.List<AnneeAcademique> Annees { get; set; }
            public System.Collections.Generic.List<Filiere> Filieres { get; set; }
            public System.Collections.Generic.List<Classe> Classes { get; set; }
            public System.Collections.Generic.List<Professeur> Professeurs { get; set; }
            public System.Collections.Generic.List<Matiere> Matieres { get; set; }
            public System.Collections.Generic.List<Semestre> Semestres { get; set; }
        }
    }
}
