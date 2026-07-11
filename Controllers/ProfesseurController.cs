using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        // GET: api/Professeur/Agenda
        [HttpGet]
        [Route("Agenda")]
        public async Task<IActionResult> GetAgenda(string email, int semaineOffset = 0)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");

            var prof = await db.Professeurs
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Creneaux)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.AffectationMatiere)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Matiere)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Classe)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Salle)
                .Include(p => p.Disponibilites)
                .FirstOrDefaultAsync(p => p.Email == email);

            if (prof == null) return NotFound("Professeur non trouvé.");

            var today = DateTime.Now.Date;
            var daysToSubtract = (int)today.DayOfWeek - 1;
            if (daysToSubtract < 0) daysToSubtract = 6; // Sunday is 0, we want Monday as start

            var startOfCurrentWeek = today.AddDays(-daysToSubtract);
            var startOfTargetWeek = startOfCurrentWeek.AddDays(7 * semaineOffset);
            var endOfTargetWeek = startOfTargetWeek.AddDays(7);

            // Fetching creneaux. Since creneaux don't have explicit dates (they are weekly recurrence),
            // if we need accurate per-week data (like specific exams or canceled courses),
            // we would check exceptions. For now, assuming they apply to every week.
            var creneaux = prof.Cours?
                .Where(c => c.AffectationMatiere == null || 
                           (startOfTargetWeek.Date <= c.AffectationMatiere.DateFin.Date && endOfTargetWeek.Date >= c.AffectationMatiere.DateDebut.Date))
                .SelectMany(c => c.Creneaux?.Select(cr => new
                {
                    IdCours = c.IdCours,
                    NomMatiere = c.Matiere?.NomMatiere ?? "—",
                    NomClasse = c.Classe?.NomClasse ?? "—",
                    NomSalle = c.Salle?.NomSalle ?? "—",
                    TypeCours = (c.TypeCours != null && c.TypeCours.ToLower() == "examen") ? "examen" : "cours",
                    JourSemaine = cr.JourSemaine,
                    HeureDebut = cr.HeureDebut,
                    HeureFin = cr.HeureFin,
                    Duree = (cr.HeureFin - cr.HeureDebut).TotalHours
                }) ?? Enumerable.Empty<dynamic>())
                .Where(c => c.TypeCours == "cours" || c.TypeCours == "examen")
                .ToList() ?? new List<dynamic>();

            var stats = new
            {
                NbCours = creneaux.Count,
                HeuresTotales = creneaux.Sum(c => (int)c.Duree)
            };

            return Ok(new
            {
                SemaineDebut = startOfTargetWeek,
                SemaineFin = endOfTargetWeek,
                Disponibilites = prof.Disponibilites?.Select(d => new { d.JourSemaine, d.HeureDebut, d.HeureFin }),
                Stats = stats,
                Creneaux = creneaux
            });
        }

        [HttpGet]
        [Route("MesMatieres")]
        public async Task<IActionResult> GetMesMatieres(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");

            var prof = await db.Professeurs
                .Include(p => p.AffectationsMatieres)
                    .ThenInclude(a => a.Matiere)
                .Include(p => p.AffectationsMatieres)
                    .ThenInclude(a => a.Classe)
                .FirstOrDefaultAsync(p => p.Email == email);

            if (prof == null) return NotFound("Professeur non trouvé.");

            var result = new List<object>();

            foreach (var aff in prof.AffectationsMatieres ?? Enumerable.Empty<AffectationMatiere>())
            {
                if (aff.Matiere == null || aff.Classe == null) continue;

                // Let's count hours already planned in actual Cours/Creneaux
                // and proposed in DemandeEdt (excluding refused ones)
                // CM, TD, TP hours
                var coursList = await db.Cours
                    .Include(c => c.Creneaux)
                    .Where(c => c.IdMatiere == aff.IdMatiere && c.IdProfesseur == prof.IdProfesseur && c.IdClasse == aff.IdClasse)
                    .ToListAsync();

                double cmPlanned = coursList.Where(c => c.TypeCours == "CM").Sum(c => c.Creneaux?.Sum(cr => (cr.HeureFin - cr.HeureDebut).TotalHours) ?? 0);
                double tdPlanned = coursList.Where(c => c.TypeCours == "TD").Sum(c => c.Creneaux?.Sum(cr => (cr.HeureFin - cr.HeureDebut).TotalHours) ?? 0);
                double tpPlanned = coursList.Where(c => c.TypeCours == "TP").Sum(c => c.Creneaux?.Sum(cr => (cr.HeureFin - cr.HeureDebut).TotalHours) ?? 0);

                // Add proposed DemandesEdt (status: brouillon or en_attente_admin or acceptee)
                var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);
                int userId = user?.IdUtilisateur ?? 0;
                var demandesList = await db.DemandesEdt
                    .Where(d => d.IdDemandeur == userId && d.IdMatiere == aff.IdMatiere && d.IdClasse == aff.IdClasse && d.Statut != "refusee")
                    .ToListAsync();

                foreach (var d in demandesList)
                {
                    if (d.HeureDebutSouhaitee.HasValue && d.HeureFinSouhaitee.HasValue)
                    {
                        var duration = (d.HeureFinSouhaitee.Value - d.HeureDebutSouhaitee.Value).TotalHours;
                        var type = d.Justification ?? "CM"; // We will store course type (CM/TD/TP) in Justification
                        if (type == "CM") cmPlanned += duration;
                        else if (type == "TD") tdPlanned += duration;
                        else if (type == "TP") tpPlanned += duration;
                    }
                }

                result.Add(new
                {
                    IdMatiere = aff.IdMatiere,
                    NomMatiere = aff.Matiere.NomMatiere,
                    CodeMatiere = aff.Matiere.CodeMatiere,
                    IdClasse = aff.IdClasse,
                    NomClasse = aff.Classe.NomClasse,
                    HeuresCm = aff.HeuresCm,
                    HeuresTd = aff.HeuresTd,
                    HeuresTp = aff.HeuresTp,
                    VolumeHoraireTotal = aff.VolumeHoraireTotal,
                    CmPlanned = cmPlanned,
                    TdPlanned = tdPlanned,
                    TpPlanned = tpPlanned,
                    TotalPlanned = cmPlanned + tdPlanned + tpPlanned
                });
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("CreneauxProposes")]
        public async Task<IActionResult> GetCreneauxProposes(string email, int? matiereId)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");

            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("Utilisateur non trouvé.");

            var query = db.DemandesEdt
                .Include(d => d.Matiere)
                .Include(d => d.Classe)
                .Include(d => d.Salle)
                .Where(d => d.IdDemandeur == user.IdUtilisateur)
                .AsQueryable();

            if (matiereId.HasValue)
            {
                query = query.Where(d => d.IdMatiere == matiereId.Value);
            }

            var list = await query.ToListAsync();
            var result = list.Select(d => new
            {
                IdDemande = d.IdDemande,
                IdMatiere = d.IdMatiere,
                NomMatiere = d.Matiere?.NomMatiere,
                IdClasse = d.IdClasse,
                NomClasse = d.Classe?.NomClasse,
                IdSalle = d.IdSalle,
                NomSalle = d.Salle?.NomSalle,
                DateSouhaitee = d.DateSouhaitee?.ToString("yyyy-MM-dd"),
                HeureDebut = d.HeureDebutSouhaitee?.ToString(@"hh\:mm"),
                HeureFin = d.HeureFinSouhaitee?.ToString(@"hh\:mm"),
                TypeCours = d.Justification ?? "CM", // We store CM/TD/TP in Justification
                Statut = d.Statut // brouillon, en_attente_admin, acceptee, refusee
            });

            return Ok(result);
        }

        public class ProposerCreneauDto
        {
            public string Email { get; set; }
            public int IdMatiere { get; set; }
            public int IdClasse { get; set; }
            public int? IdSalle { get; set; }
            public string DateStr { get; set; }
            public string HeureDebut { get; set; }
            public string HeureFin { get; set; }
            public string TypeCours { get; set; } // CM, TD, TP
        }

        [HttpPost]
        [Route("ProposerCreneau")]
        public async Task<IActionResult> ProposerCreneau([FromBody] ProposerCreneauDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email)) return BadRequest("Invalid request.");

            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Utilisateur non trouvé.");

            if (!DateTime.TryParse(dto.DateStr, out var date))
                return BadRequest("Invalid date format.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var hd) || !TimeSpan.TryParse(dto.HeureFin, out var hf))
                return BadRequest("Invalid time format.");

            // Validation: Check for overlaps in existing actual recurring Creneaux
            var jourSemaine = date.DayOfWeek switch
            {
                DayOfWeek.Monday => "MON",
                DayOfWeek.Tuesday => "TUE",
                DayOfWeek.Wednesday => "WED",
                DayOfWeek.Thursday => "THU",
                DayOfWeek.Friday => "FRI",
                DayOfWeek.Saturday => "SAT",
                DayOfWeek.Sunday => "SUN",
                _ => ""
            };

            var prof = await db.Professeurs.FirstOrDefaultAsync(p => p.Email == dto.Email);
            int profId = prof?.IdProfesseur ?? 0;

            var conflictCreneau = await db.Creneaux
                .Include(c => c.Cours)
                .FirstOrDefaultAsync(c =>
                    c.JourSemaine == jourSemaine &&
                    (c.Cours.IdProfesseur == profId || c.Cours.IdClasse == dto.IdClasse || (dto.IdSalle.HasValue && c.Cours.IdSalle == dto.IdSalle.Value)) &&
                    ((hd >= c.HeureDebut && hd < c.HeureFin) ||
                     (hf > c.HeureDebut && hf <= c.HeureFin) ||
                     (hd <= c.HeureDebut && hf >= c.HeureFin))
                );

            if (conflictCreneau != null)
            {
                if (conflictCreneau.Cours.IdProfesseur == profId)
                    return BadRequest("Conflit d'emploi du temps : Vous dispensez déjà un autre cours à cette heure sur ce jour de la semaine.");
                if (conflictCreneau.Cours.IdClasse == dto.IdClasse)
                    return BadRequest("Conflit d'emploi du temps : La classe a déjà un cours prévu à cette heure sur ce jour de la semaine.");
                if (dto.IdSalle.HasValue && conflictCreneau.Cours.IdSalle == dto.IdSalle.Value)
                    return BadRequest("Conflit d'emploi du temps : La salle est occupée à cette heure sur ce jour de la semaine.");
            }

            // Validation: Check for overlaps in other proposed/active DemandesEdt on the exact date
            var conflictDemande = await db.DemandesEdt
                .FirstOrDefaultAsync(d =>
                    d.DateSouhaitee.HasValue && d.DateSouhaitee.Value.Date == date.Date &&
                    d.Statut != "refusee" &&
                    (d.IdDemandeur == user.IdUtilisateur || d.IdClasse == dto.IdClasse || (dto.IdSalle.HasValue && d.IdSalle == dto.IdSalle.Value)) &&
                    d.HeureDebutSouhaitee.HasValue && d.HeureFinSouhaitee.HasValue &&
                    ((hd >= d.HeureDebutSouhaitee.Value && hd < d.HeureFinSouhaitee.Value) ||
                     (hf > d.HeureDebutSouhaitee.Value && hf <= d.HeureFinSouhaitee.Value) ||
                     (hd <= d.HeureDebutSouhaitee.Value && hf >= d.HeureFinSouhaitee.Value))
                );

            if (conflictDemande != null)
            {
                if (conflictDemande.IdDemandeur == user.IdUtilisateur)
                    return BadRequest("Conflit de planification : Vous avez déjà proposé un autre cours à ce même moment sur cette date.");
                if (conflictDemande.IdClasse == dto.IdClasse)
                    return BadRequest("Conflit de planification : La classe est déjà ciblée par une autre proposition à cette heure.");
                if (dto.IdSalle.HasValue && conflictDemande.IdSalle == dto.IdSalle.Value)
                    return BadRequest("Conflit de planification : La salle est déjà demandée par une autre proposition sur cette plage horaire.");
            }

            // Create a draft (brouillon) DemandeEdt
            var demande = new DemandeEdt
            {
                IdDemandeur = user.IdUtilisateur,
                TypeDemande = "planification",
                Statut = "brouillon",
                IdMatiere = dto.IdMatiere,
                IdClasse = dto.IdClasse,
                IdSalle = dto.IdSalle,
                DateSouhaitee = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
                HeureDebutSouhaitee = hd,
                HeureFinSouhaitee = hf,
                Justification = dto.TypeCours // Store course type in justification
            };

            db.DemandesEdt.Add(demande);
            await db.SaveChangesAsync();

            return Ok(new { success = true, idDemande = demande.IdDemande, message = "Créneau ajouté au brouillon." });
        }

        [HttpDelete]
        [Route("SupprimerCreneau/{id:int}")]
        public async Task<IActionResult> SupprimerCreneau(int id)
        {
            var demande = await db.DemandesEdt.FindAsync(id);
            if (demande == null) return NotFound("Créneau non trouvé.");

            // Only allow deleting drafts or pending requests
            if (demande.Statut != "brouillon" && demande.Statut != "en_attente_admin")
                return BadRequest("Impossible de supprimer un créneau déjà traité.");

            db.DemandesEdt.Remove(demande);
            await db.SaveChangesAsync();

            return Ok(new { success = true, message = "Créneau supprimé." });
        }

        public class SoumettrePropositionDto
        {
            public string Email { get; set; }
            public int? IdMatiere { get; set; } // Optional: submit only for specific subject
        }

        [HttpPost]
        [Route("SoumettreProposition")]
        public async Task<IActionResult> SoumettreProposition([FromBody] SoumettrePropositionDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email)) return BadRequest("Invalid request.");

            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Utilisateur non trouvé.");

            var query = db.DemandesEdt
                .Where(d => d.IdDemandeur == user.IdUtilisateur && d.Statut == "brouillon");

            if (dto.IdMatiere.HasValue)
            {
                query = query.Where(d => d.IdMatiere == dto.IdMatiere.Value);
            }

            var drafts = await query.ToListAsync();
            if (!drafts.Any())
            {
                return BadRequest("Aucun créneau en brouillon à soumettre.");
            }

            foreach (var d in drafts)
            {
                d.Statut = "en_attente_admin";
            }

            await db.SaveChangesAsync();

            return Ok(new { success = true, message = $"{drafts.Count} créneaux soumis à l'administration." });
        }

        [HttpPost]
        [Route("RetablirPropositions")]
        public async Task<IActionResult> RetablirPropositions([FromBody] SoumettrePropositionDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email)) return BadRequest("Invalid request.");

            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound("Utilisateur non trouvé.");

            var query = db.DemandesEdt
                .Where(d => d.IdDemandeur == user.IdUtilisateur && d.Statut == "brouillon");

            if (dto.IdMatiere.HasValue)
            {
                query = query.Where(d => d.IdMatiere == dto.IdMatiere.Value);
            }

            var drafts = await query.ToListAsync();
            db.DemandesEdt.RemoveRange(drafts);
            await db.SaveChangesAsync();

            return Ok(new { success = true, message = "Brouillons réinitialisés." });
        }
    }
}
