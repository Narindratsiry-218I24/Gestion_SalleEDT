using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/AnneeAcademique")]
    [ApiController]
    public class AnneeAcademiqueController : ControllerBase
    {
        private readonly EMITDbContext _db;
        private readonly ILogger<AnneeAcademiqueController> _logger;
        private readonly IClasseGenerationService _classeGenerationService;

        public AnneeAcademiqueController(EMITDbContext context, ILogger<AnneeAcademiqueController> logger, IClasseGenerationService classeGenerationService)
        {
            _db = context;
            _logger = logger;
            _classeGenerationService = classeGenerationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAnnees(int page = 1, int pageSize = 6, string? statut = null, string? search = null)
        {
            var query = _db.AnneesAcademiques
                .Include(a => a.Semestres).ThenInclude(s => s.RefSemestre)
                .Include(a => a.Classes).ThenInclude(c => c.Filiere)
                .Include(a => a.Classes).ThenInclude(c => c.Niveau)
                .Include(a => a.Professeurs)
                .OrderByDescending(a => a.DateDebutAnnee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(a => a.Libelle.Contains(search));

            if (statut == "active")
                query = query.Where(a => a.EstActive && !a.EstArchivee);
            else if (statut == "archivee")
                query = query.Where(a => a.EstArchivee);
            else if (statut == "avenir")
                query = query.Where(a => !a.EstActive && !a.EstArchivee);

            var total = await query.CountAsync();
            var totalClasses = await query.SelectMany(a => a.Classes).CountAsync();
            var totalProfesseurs = await query.SelectMany(a => a.Professeurs).CountAsync();
            var totalActive = await query.CountAsync(a => a.EstActive && !a.EstArchivee);
            var totalArchivee = await query.CountAsync(a => a.EstArchivee);
            var totalAvenir = await query.CountAsync(a => !a.EstActive && !a.EstArchivee);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = items.Select(a => new
            {
                a.IdAnnee,
                a.Libelle,
                a.DateDebutAnnee,
                a.DateFinAnnee,
                a.EstActive,
                a.EstArchivee,
                a.DateArchivage,
                Semestres = a.Semestres.Select(s => new
                {
                    s.IdSemestre,
                    CodeSemestre = s.RefSemestre != null ? s.RefSemestre.CodeSemestre : null,
                    DateDebut = s.DateDebut,
                    DateFin = s.DateFin
                }).ToList(),
                Classes = a.Classes.Select(c => new
                {
                    c.IdClasse,
                    c.NomClasse,
                    c.CodeClasse,
                    Filiere = c.Filiere != null ? c.Filiere.NomFiliere : null,
                    Niveau = c.Niveau != null ? c.Niveau.CodeNiveau : null,
                    c.EstArchivee
                }).ToList(),
                ProfesseursCount = a.Professeurs?.Count ?? 0
            }).ToList();

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Stats = new
                {
                    TotalAnnees = total,
                    AnneesActives = totalActive,
                    AnneesArchivees = totalArchivee,
                    AnneesAvenir = totalAvenir,
                    TotalClasses = totalClasses,
                    TotalProfesseurs = totalProfesseurs
                },
                Items = result
            });
        }

        [HttpGet]
        [Route("{id:int}/Details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var annee = await _db.AnneesAcademiques
                .Include(a => a.Semestres).ThenInclude(s => s.RefSemestre)
                .Include(a => a.Classes).ThenInclude(c => c.Filiere)
                .Include(a => a.Classes).ThenInclude(c => c.Niveau)
                .Include(a => a.Professeurs)
                .FirstOrDefaultAsync(a => a.IdAnnee == id);

            if (annee == null) return NotFound();

            var stats = new
            {
                TotalSemestres = annee.Semestres.Count,
                TotalClasses = annee.Classes.Count,
                TotalProfesseurs = annee.Professeurs?.Count ?? 0,
                ClassesParMention = annee.Classes
                    .GroupBy(c => c.Filiere != null ? c.Filiere.NomFiliere : "N/A")
                    .Select(g => new { Mention = g.Key, Count = g.Count() })
                    .ToList()
            };

            return Ok(new { Annee = annee, Statistiques = stats });
        }

        [HttpPost]
        [Route("")]
        public IActionResult CreateAnnee([FromBody] AnneeAcademique annee)
        {
            ModelState.Remove("Semestres");
            ModelState.Remove("Classes");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _db.AnneesAcademiques.Add(annee);
            _db.SaveChanges();
            return CreatedAtAction(nameof(GetDetails), new { id = annee.IdAnnee }, annee);
        }

        [HttpPost]
        [Route("CreerAvecSemestres")]
        public async Task<IActionResult> CreerAnneeAvecSemestres([FromBody] CreateAnneeDto dto)
        {
            if (dto == null) return BadRequest("Données manquantes.");
            if (string.IsNullOrWhiteSpace(dto.Libelle)) return BadRequest("Libellé requis.");

            var annee = new AnneeAcademique
            {
                Libelle = dto.Libelle,
                DateDebutAnnee = dto.DateDebut,
                DateFinAnnee = dto.DateFin,
                EstActive = dto.EstActive,
                EstArchivee = false
            };

            _db.AnneesAcademiques.Add(annee);
            await _db.SaveChangesAsync();

            var refSemestres = _db.RefSemestres.OrderBy(r => r.Ordre).ToList();
            if (!refSemestres.Any())
            {
                var s1 = new RefSemestre { CodeSemestre = "S1", Ordre = 1, IdNiveau = 1 };
                var s2 = new RefSemestre { CodeSemestre = "S2", Ordre = 2, IdNiveau = 1 };
                _db.RefSemestres.AddRange(s1, s2);
                await _db.SaveChangesAsync();
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
            await _db.SaveChangesAsync();

            var classesCreated = await _classeGenerationService.GenererClassesPourAnneeAsync(annee.IdAnnee);

            return Ok(new { Annee = annee, Message = "Année créée avec ses semestres", ClassesGenerated = classesCreated });
        }

        [HttpPost]
        [Route("{id:int}/GenererClasses")]
        public async Task<IActionResult> GenererClasses(int id)
        {
            var annee = await _db.AnneesAcademiques.FindAsync(id);
            if (annee == null) return NotFound("Année introuvable");

            var nbClasses = await _classeGenerationService.GenererClassesPourAnneeAsync(id);
            
            return Ok(new { 
                Annee = annee.Libelle, 
                ClassesGenerees = nbClasses,
                Message = $"{nbClasses} classes générées pour l'année {annee.Libelle}"
            });
        }

        [HttpPost]
        [Route("CreerComplete")]
        public async Task<IActionResult> CreerAnneeComplete([FromBody] CreateAnneeDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Créer l'année
                var annee = new AnneeAcademique
                {
                    Libelle = dto.Libelle,
                    DateDebutAnnee = dto.DateDebut,
                    DateFinAnnee = dto.DateFin,
                    EstActive = dto.EstActive,
                    EstArchivee = false
                };
                _db.AnneesAcademiques.Add(annee);
                await _db.SaveChangesAsync();

                // 2. Créer les semestres pour cette année
                var refSemestres = await _db.RefSemestres.ToListAsync();
                foreach (var refSem in refSemestres)
                {
                    var semestre = new Semestre
                    {
                        IdRefSemestre = refSem.IdRefSemestre,
                        IdAnnee = annee.IdAnnee,
                        DateDebut = dto.DateDebut,
                        DateFin = dto.DateFin,
                        EstArchivee = false
                    };
                    _db.Semestres.Add(semestre);
                }
                await _db.SaveChangesAsync();

                // 3. Générer automatiquement les classes
                var nbClasses = await _classeGenerationService.GenererClassesPourAnneeAsync(annee.IdAnnee);

                await transaction.CommitAsync();

                return Ok(new
                {
                    Annee = annee,
                    SemestresCount = refSemestres.Count,
                    ClassesCount = nbClasses,
                    Message = $"Année {annee.Libelle} créée avec {refSemestres.Count} semestres et {nbClasses} classes"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Erreur: {ex.Message}");
            }
        }

        public class CreateAnneeDto
        {
            public string Libelle { get; set; }
            public DateTime DateDebut { get; set; }
            public DateTime DateFin { get; set; }
            public bool EstActive { get; set; } = true;
        }

        [HttpPost]
        [Route("{id:int}/Archiver")]
        public async Task<IActionResult> ArchiverAnnee(int id)
        {
            var annee = await _db.AnneesAcademiques.FindAsync(id);
            if (annee == null) return NotFound();
            annee.EstArchivee = true;
            annee.DateArchivage = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { Message = "Année archivée." });
        }

        [HttpPost]
        [Route("{id:int}/Restaurer")]
        public async Task<IActionResult> RestaurerAnnee(int id)
        {
            var annee = await _db.AnneesAcademiques.FindAsync(id);
            if (annee == null) return NotFound();
            annee.EstArchivee = false;
            annee.DateArchivage = null;
            await _db.SaveChangesAsync();
            return Ok(new { Message = "Année restaurée." });
        }

        [HttpPost]
        [Route("{id:int}/GenererEdt")]
        public async Task<IActionResult> GenererEdt(int id)
        {
            var classeIds = await _db.Classes
                .Where(c => c.IdAnneeAcademique == id)
                .Select(c => c.IdClasse)
                .ToListAsync();

            if (!classeIds.Any())
                return BadRequest("Aucune classe trouvée pour cette année.");

            var affectations = await _db.AffectationsMatieres
                .Where(a => classeIds.Contains(a.IdClasse))
                .ToListAsync();

            if (!affectations.Any())
                return BadRequest("Aucune affectation trouvée pour cette année.");

            var generated = 0;
            foreach (var affectation in affectations)
            {
                var already = await _db.Cours.AnyAsync(c => c.IdAffectation == affectation.IdAffectation);
                if (already) continue;

                if (affectation.HeuresCm > 0)
                {
                    _db.Cours.Add(new Cours
                    {
                        IdMatiere = affectation.IdMatiere,
                        IdProfesseur = affectation.IdProfesseur,
                        IdClasse = affectation.IdClasse,
                        IdSemestre = affectation.IdSemestre,
                        IdAffectation = affectation.IdAffectation,
                        TypeCours = "CM",
                        Statut = "a_planifier"
                    });
                    generated++;
                }
                if (affectation.HeuresTd > 0)
                {
                    _db.Cours.Add(new Cours
                    {
                        IdMatiere = affectation.IdMatiere,
                        IdProfesseur = affectation.IdProfesseur,
                        IdClasse = affectation.IdClasse,
                        IdSemestre = affectation.IdSemestre,
                        IdAffectation = affectation.IdAffectation,
                        TypeCours = "TD",
                        Statut = "a_planifier"
                    });
                    generated++;
                }
                if (affectation.HeuresTp > 0)
                {
                    _db.Cours.Add(new Cours
                    {
                        IdMatiere = affectation.IdMatiere,
                        IdProfesseur = affectation.IdProfesseur,
                        IdClasse = affectation.IdClasse,
                        IdSemestre = affectation.IdSemestre,
                        IdAffectation = affectation.IdAffectation,
                        TypeCours = "TP",
                        Statut = "a_planifier"
                    });
                    generated++;
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { Message = "EDT généraux générés.", GeneratedCourses = generated });
        }

        [HttpGet]
        [Route("ClassesPreview")]
        public async Task<IActionResult> GetClassesPreview()
        {
            var filieres = await _db.Filieres.Include(f => f.Mention).ToListAsync();
            var niveaux = await _db.Niveaux.OrderBy(n => n.Ordre).ToListAsync();

            var preview = filieres.SelectMany(f => niveaux, (f, n) => new
            {
                NomClasse = $"{n.CodeNiveau}-{f.CodeFiliere}",
                Filiere = f.NomFiliere,
                Niveau = n.CodeNiveau,
                Mention = f.Mention != null ? f.Mention.NomMention : null
            }).ToList();

            return Ok(new { Total = preview.Count, Items = preview });
        }

        [HttpPost]
        [Route("{id:int}/CopierReglages/{sourceId:int}")]
        public IActionResult CopierReglages(int id, int sourceId)
        {
            if (id == sourceId) return BadRequest("L'annee source et l'annee cible doivent etre differentes.");

            var cible = _db.AnneesAcademiques.Include(a => a.Semestres).FirstOrDefault(a => a.IdAnnee == id);
            var source = _db.AnneesAcademiques
                .Include(a => a.Classes)
                .Include(a => a.Semestres)
                .FirstOrDefault(a => a.IdAnnee == sourceId);

            if (cible == null || source == null) return NotFound();

            foreach (var classeSource in source.Classes)
            {
                var existe = _db.Classes.Any(c =>
                    c.IdAnneeAcademique == cible.IdAnnee &&
                    c.CodeClasse == classeSource.CodeClasse);

                if (!existe)
                {
                    _db.Classes.Add(new Classe
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
                    _db.Semestres.Add(new Semestre
                    {
                        IdAnnee = cible.IdAnnee,
                        IdRefSemestre = semestreSource.IdRefSemestre,
                        DateDebut = cible.DateDebutAnnee,
                        DateFin = cible.DateFinAnnee
                    });
                }
            }

            _db.SaveChanges();
            return Ok(new { Message = "Reglages copies sans recopier les cours.", IdAnnee = id, SourceId = sourceId });
        }
    }
}
