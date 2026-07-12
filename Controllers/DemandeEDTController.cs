using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Helpers;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/DemandeEDT")]
    [ApiController]
    public class DemandeEDTController : ControllerBase
    {
        private readonly EMITDbContext db;
        private readonly IConflitService _conflitService;

        public DemandeEDTController(EMITDbContext context, IConflitService conflitService)
        {
            db = context;
            _conflitService = conflitService;
        }

        private static bool IsPending(string statut)
        {
            if (string.IsNullOrEmpty(statut)) return false;
            var s = statut.Replace(" ", "_").ToLowerInvariant();
            return s == "en_attente" || s == "pending" || s == "en_attente_admin"
                || s == "proposee_prof" || s == "modif_acceptee_prof" || s == "contre_proposition_prof";
        }

        private static string NormalizeStatut(string statut)
        {
            if (string.IsNullOrEmpty(statut)) return "en_attente_admin";
            var s = statut.Replace(" ", "_").ToLowerInvariant();
            if (s == "en_attente" || s == "pending" || s == "en_attente_admin") return "en_attente_admin";
            if (s == "validée" || s == "validee" || s == "approved" || s == "acceptee") return "acceptee";
            if (s == "refusée" || s == "refusee" || s == "rejected") return "refusee";
            return s;
        }

        private static string MapDayOfWeek(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday    => "MON",
            DayOfWeek.Tuesday   => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday  => "THU",
            DayOfWeek.Friday    => "FRI",
            DayOfWeek.Saturday  => "SAT",
            DayOfWeek.Sunday    => "SUN",
            _                   => "MON"
        };

        [HttpGet]
        [Route("")]
        public IActionResult GetDemandes(
            [FromQuery] string? statut,
            [FromQuery] string? typeDemande,
            [FromQuery] int? idMention,
            [FromQuery] int? idNiveau,
            [FromQuery] int? idFiliere,
            [FromQuery] int? idClasse,
            [FromQuery] int? idMatiere,
            [FromQuery] string? search,
            [FromQuery] int? take)
        {
            var ctx = UserContextHelper.FromRequest(ControllerContext);
            var query = db.DemandesEdt
                .Include(d => d.Demandeur)
                .Include(d => d.Cours.Matiere)
                .Include(d => d.Cours.Classe)
                .Include(d => d.Salle)
                .Include(d => d.Niveau)
                .Include(d => d.Classe)
                .Include(d => d.Matiere)
                .Include("Propositions.Salle")
                .AsQueryable();

            if (ctx.IsAuthenticated && ctx.Role == "demandeur")
                query = query.Where(d => d.IdDemandeur == ctx.UserId!.Value);

            if (!string.IsNullOrWhiteSpace(statut))
            {
                var statutNormalized = NormalizeStatut(statut);
                query = query.Where(d => d.Statut == statutNormalized);
            }

            if (!string.IsNullOrWhiteSpace(typeDemande))
            {
                var typeNormalized = typeDemande.Trim().ToLower();
                query = query.Where(d => d.TypeDemande.ToLower() == typeNormalized);
            }

            if (idMention.HasValue)
            {
                query = query.Where(d =>
                    (d.Niveau != null && d.Niveau.IdMention == idMention.Value) ||
                    (d.Classe != null && d.Classe.Filiere.IdMention == idMention.Value) ||
                    (d.Matiere != null && d.Matiere.Filiere.IdMention == idMention.Value) ||
                    (d.Cours != null && d.Cours.Classe != null && d.Cours.Classe.Filiere.IdMention == idMention.Value) ||
                    (d.Cours != null && d.Cours.Matiere != null && d.Cours.Matiere.Filiere.IdMention == idMention.Value));
            }

            if (idNiveau.HasValue)
            {
                query = query.Where(d =>
                    d.IdNiveau == idNiveau.Value ||
                    (d.Classe != null && d.Classe.Semestre.RefSemestre.IdNiveau == idNiveau.Value) ||
                    (d.Matiere != null && d.Matiere.RefSemestre.IdNiveau == idNiveau.Value) ||
                    (d.Cours != null && d.Cours.Classe != null && d.Cours.Classe.Semestre.RefSemestre.IdNiveau == idNiveau.Value) ||
                    (d.Cours != null && d.Cours.Matiere != null && d.Cours.Matiere.RefSemestre.IdNiveau == idNiveau.Value));
            }

            if (idFiliere.HasValue)
            {
                query = query.Where(d =>
                    (d.Classe != null && d.Classe.IdFiliere == idFiliere.Value) ||
                    (d.Matiere != null && d.Matiere.IdFiliere == idFiliere.Value) ||
                    (d.Cours != null && d.Cours.Classe != null && d.Cours.Classe.IdFiliere == idFiliere.Value) ||
                    (d.Cours != null && d.Cours.Matiere != null && d.Cours.Matiere.IdFiliere == idFiliere.Value));
            }

            if (idClasse.HasValue)
                query = query.Where(d => d.IdClasse == idClasse.Value || (d.Cours != null && d.Cours.IdClasse == idClasse.Value));

            if (idMatiere.HasValue)
                query = query.Where(d => d.IdMatiere == idMatiere.Value || (d.Cours != null && d.Cours.IdMatiere == idMatiere.Value));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(d =>
                    d.TypeDemande.ToLower().Contains(term) ||
                    d.Statut.ToLower().Contains(term) ||
                    (d.Justification != null && d.Justification.ToLower().Contains(term)) ||
                    (d.Classe != null && d.Classe.NomClasse.ToLower().Contains(term)) ||
                    (d.Matiere != null && d.Matiere.NomMatiere.ToLower().Contains(term)) ||
                    (d.Salle != null && d.Salle.NomSalle.ToLower().Contains(term)) ||
                    (d.Cours != null && d.Cours.Classe != null && d.Cours.Classe.NomClasse.ToLower().Contains(term)) ||
                    (d.Cours != null && d.Cours.Matiere != null && d.Cours.Matiere.NomMatiere.ToLower().Contains(term)));
            }

            query = query.OrderByDescending(d => d.IdDemande);

            if (take.HasValue && take.Value > 0)
                query = query.Take(Math.Min(take.Value, 100));

            return Ok(query.ToList());
        }

        [HttpGet]
        [Route("Notifications")]
        public IActionResult GetNotifications([FromQuery] int take = 5)
        {
            var demandes = db.DemandesEdt
                .Include(d => d.Classe)
                .Include(d => d.Matiere)
                .Include(d => d.Salle)
                .OrderByDescending(d => d.IdDemande)
                .Take(Math.Clamp(take, 1, 100))
                .Select(d => new
                {
                    d.IdDemande,
                    d.TypeDemande,
                    d.Statut,
                    d.Justification,
                    Classe = d.Classe != null ? d.Classe.NomClasse : null,
                    Matiere = d.Matiere != null ? d.Matiere.NomMatiere : null,
                    Salle = d.Salle != null ? d.Salle.NomSalle : null
                })
                .ToList();

            return Ok(demandes);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetDemande(int id)
        {
            var demande = db.DemandesEdt
                .Include(d => d.Demandeur)
                .Include(d => d.Cours.Matiere)
                .Include(d => d.Salle)
                .Include(d => d.Niveau)
                .Include(d => d.Classe)
                .Include(d => d.Matiere)
                .Include("Propositions.Salle")
                .FirstOrDefault(d => d.IdDemande == id);

            if (demande == null) return NotFound();
            return Ok(demande);
        }

        [HttpPost]
        [Route("Creer")]
        public IActionResult CreerDemande([FromBody] DemandeEdt demande)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                demande.Statut = NormalizeStatut(demande.Statut);
                demande.DateCreation = DateTime.Now;

                // Determine professor ID to evaluate availability
                int? profId = null;
                if (demande.IdCours.HasValue)
                {
                    var cours = db.Cours.Find(demande.IdCours.Value);
                    profId = cours?.IdProfesseur;
                }
                else
                {
                    var demandeur = db.Utilisateurs.Find(demande.IdDemandeur);
                    if (demandeur != null)
                    {
                        var prof = db.Professeurs.FirstOrDefault(p => p.Email == demandeur.Email);
                        profId = prof?.IdProfesseur;
                    }
                }

                if (profId.HasValue && demande.DateSouhaitee.HasValue && demande.HeureDebutSouhaitee.HasValue && demande.HeureFinSouhaitee.HasValue)
                {
                    var date = demande.DateSouhaitee.Value;
                    var debut = demande.HeureDebutSouhaitee.Value;
                    var fin = demande.HeureFinSouhaitee.Value;

                    // Scoping academic year
                    int? idAnnee = null;
                    if (demande.IdClasse.HasValue)
                    {
                        var classe = db.Classes.Find(demande.IdClasse.Value);
                        idAnnee = classe?.IdAnneeAcademique;
                    }
                    else if (demande.IdCours.HasValue)
                    {
                        var cours = db.Cours.Include(c => c.Classe).FirstOrDefault(c => c.IdCours == demande.IdCours.Value);
                        idAnnee = cours?.Classe?.IdAnneeAcademique;
                    }

                    string dayCode = date.DayOfWeek switch
                    {
                        DayOfWeek.Monday => "MON",
                        DayOfWeek.Tuesday => "TUE",
                        DayOfWeek.Wednesday => "WED",
                        DayOfWeek.Thursday => "THU",
                        DayOfWeek.Friday => "FRI",
                        DayOfWeek.Saturday => "SAT",
                        _ => ""
                    };

                    // Fetch professor availabilities
                    var profDispos = db.DisponibilitesProf
                        .Where(d => d.IdProfesseur == profId.Value && (d.IdAnnee == null || d.IdAnnee == idAnnee))
                        .ToList();

                    var disposRecurrentes = profDispos.Where(d => d.TypeDisponibilite == TypeDisponibilite.Recurrente && d.JourSemaineCode == dayCode).ToList();
                    var disposPonctuelles = profDispos.Where(d => d.TypeDisponibilite == TypeDisponibilite.Ponctuelle && d.DateSpecifique.HasValue && d.DateSpecifique.Value.Date == date.Date).ToList();

                    var matchingDispos = disposPonctuelles.Any() ? disposPonctuelles : disposRecurrentes;

                    if (!matchingDispos.Any())
                    {
                        demande.HorsDisponibilite = true;
                    }
                    else
                    {
                        bool isCovered = matchingDispos.Any(d => d.HeureDebut <= debut && d.HeureFin >= fin);
                        demande.HorsDisponibilite = !isCovered;
                    }
                }

                db.DemandesEdt.Add(demande);
                db.SaveChanges();
                return Ok(demande);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message
                         ?? ex.InnerException?.Message
                         ?? ex.Message;
                return BadRequest(new { Message = "DB Error: " + inner });
            }
        }

        [HttpPost]
        [Route("{id:int}/Suspendre")]
        public IActionResult SuspendreDemande(int id, [FromBody] string justification = "")
        {
            var demande = db.DemandesEdt.Find(id);
            if (demande == null) return NotFound();

            demande.Statut = "en_attente_info";
            if (!string.IsNullOrEmpty(justification))
                demande.Justification = (demande.Justification ?? "") + "\n[Suspendu] " + justification;

            db.SaveChanges();
            return Ok(demande);
        }

        [HttpPost]
        [Route("{id:int}/ProposerAlternatives")]
        public IActionResult ProposerAlternatives(int id, [FromBody] List<PropositionAdmin> propositions)
        {
            var demande = db.DemandesEdt.Find(id);
            if (demande == null) return NotFound();

            if (propositions == null || !propositions.Any())
                return BadRequest("La liste des propositions ne peut pas être vide.");

            var oldProps = db.PropositionsAdmin.Where(p => p.IdDemande == id).ToList();
            db.PropositionsAdmin.RemoveRange(oldProps);

            foreach (var prop in propositions)
            {
                prop.IdDemande = id;
                prop.EstAcceptee = null;
                db.PropositionsAdmin.Add(prop);
            }

            demande.Statut = "proposee_prof";
            db.SaveChanges();
            return Ok(demande);
        }

        [HttpPost]
        [Route("{id:int}/Proposition/{idProposition:int}/Accepter")]
        public IActionResult AccepterProposition(int id, int idProposition)
        {
            var demande = db.DemandesEdt
                .Include(d => d.Propositions)
                .FirstOrDefault(d => d.IdDemande == id);

            if (demande == null) return NotFound();

            var propChoisie = demande.Propositions.FirstOrDefault(p => p.IdProposition == idProposition);
            if (propChoisie == null)
                return BadRequest("La proposition spécifiée n'existe pas pour cette demande.");

            foreach (var prop in demande.Propositions)
                prop.EstAcceptee = (prop.IdProposition == idProposition);

            demande.Statut = "modif_acceptee_prof";
            db.SaveChanges();
            return Ok(demande);
        }

        [HttpPost]
        [Route("{id:int}/ContreProposer")]
        public IActionResult ContreProposer(int id, [FromBody] string nouvelleProposition)
        {
            var demande = db.DemandesEdt.Find(id);
            if (demande == null) return NotFound();

            demande.Statut = "contre_proposition_prof";
            demande.Justification = (demande.Justification ?? "")
                + "\n[Contre-Proposition Enseignant] : " + nouvelleProposition;

            var props = db.PropositionsAdmin.Where(p => p.IdDemande == id).ToList();
            foreach (var p in props) p.EstAcceptee = false;

            db.SaveChanges();
            return Ok(demande);
        }

        [HttpPost]
        [Route("{id:int}/Valider")]
        public async Task<IActionResult> ValiderDemande(int id)
        {
            var demande = db.DemandesEdt
                .Include(d => d.Cours.Creneaux)
                .Include(d => d.Propositions)
                .FirstOrDefault(d => d.IdDemande == id);

            if (demande == null) return NotFound();

            int finalSalleId          = demande.IdSalle ?? 1;
            DateTime? finalDate       = demande.DateSouhaitee;
            TimeSpan? finalHeureDebut = demande.HeureDebutSouhaitee;
            TimeSpan? finalHeureFin   = demande.HeureFinSouhaitee;

            var propositionAcceptee = demande.Propositions.FirstOrDefault(p => p.EstAcceptee == true);
            if (propositionAcceptee != null)
            {
                finalSalleId    = propositionAcceptee.IdSalleProposee;
                finalDate       = propositionAcceptee.DateProposee;
                finalHeureDebut = propositionAcceptee.HeureDebutProposee;
                finalHeureFin   = propositionAcceptee.HeureFinProposee;
            }

            if (!finalDate.HasValue || !finalHeureDebut.HasValue || !finalHeureFin.HasValue)
                return BadRequest("Les informations d'horaire et de date sont requises pour planifier.");

            string jourSemaine = MapDayOfWeek(finalDate.Value.DayOfWeek);

            int finalClasseId = demande.IdClasse ?? (demande.Cours != null ? (demande.Cours.IdClasse ?? 0) : 0);

            int finalProfId = 0;
            if (demande.IdCours.HasValue && demande.Cours != null)
            {
                finalProfId = demande.Cours.IdProfesseur ?? 0;
            }
            else
            {
                var demandeur = db.Utilisateurs.Find(demande.IdDemandeur);
                var prof = db.Professeurs.FirstOrDefault(p => p.Email == demandeur!.Email)
                        ?? db.Professeurs.FirstOrDefault();
                finalProfId = prof?.IdProfesseur ?? 1;
            }

            // 1.2 — Vérification via IConflitService (source : Seance, pas Creneau)
            var conflitResult = await _conflitService.VerifierAsync(
                date: finalDate.Value,
                heureDebut: finalHeureDebut.Value,
                heureFin: finalHeureFin.Value,
                salleId: finalSalleId > 0 ? finalSalleId : null,
                classeId: finalClasseId > 0 ? finalClasseId : null,
                profId: finalProfId > 0 ? finalProfId : null,
                excludeCoursId: demande.IdCours
            );

            if (conflitResult.HasConflict)
                return BadRequest($"Conflit détecté : {conflitResult.Message}");

            if (demande.IdCours.HasValue)
            {
                var cours = db.Cours.Include(c => c.Creneaux).FirstOrDefault(c => c.IdCours == demande.IdCours.Value);
                if (cours != null)
                {
                    cours.IdSalle = finalSalleId;
                    var creneau = cours.Creneaux.FirstOrDefault();
                    if (creneau != null)
                    {
                        creneau.JourSemaine = jourSemaine;
                        creneau.HeureDebut  = finalHeureDebut.Value;
                        creneau.HeureFin    = finalHeureFin.Value;
                    }
                    else
                    {
                        db.Creneaux.Add(new Creneau
                        {
                            IdCreneau   = (db.Creneaux.Max(c => (int?)c.IdCreneau) ?? 0) + 1,
                            IdCours     = cours.IdCours,
                            JourSemaine = jourSemaine,
                            HeureDebut  = finalHeureDebut.Value,
                            HeureFin    = finalHeureFin.Value,
                            SemaineType = "A"
                        });
                    }
                }
            }
            else
            {
                if (!demande.IdMatiere.HasValue || !demande.IdClasse.HasValue)
                    return BadRequest("La matière et la classe sont requises pour planifier un nouveau cours/examen.");

                var demandeur = db.Utilisateurs.Find(demande.IdDemandeur);
                var prof = db.Professeurs.FirstOrDefault(p => p.Email == demandeur!.Email)
                        ?? db.Professeurs.FirstOrDefault();
                int idProf = prof?.IdProfesseur ?? 1;

                var classe = db.Classes.Find(demande.IdClasse.Value);
                if (classe == null) return BadRequest("Classe introuvable.");

                var matiere = db.Matieres.Find(demande.IdMatiere.Value);
                if (matiere == null) return BadRequest("Matiere introuvable.");

                var semestre = db.Semestres.FirstOrDefault(s =>
                    s.IdAnnee == classe.IdAnneeAcademique &&
                    s.IdRefSemestre == matiere.IdRefSemestre)
                    ?? db.Semestres.FirstOrDefault(s => s.IdAnnee == classe.IdAnneeAcademique);

                if (semestre == null)
                    return BadRequest("Aucun semestre n'est configure pour l'annee academique de cette classe.");

                int idSemestre = semestre.IdSemestre;
                var affectation = db.AffectationsMatieres.FirstOrDefault(a =>
                    a.IdClasse == demande.IdClasse.Value &&
                    a.IdMatiere == demande.IdMatiere.Value &&
                    a.IdSemestre == idSemestre &&
                    a.EstActif);

                int newCoursId = (db.Cours.Max(c => (int?)c.IdCours) ?? 0) + 1;
                var nouveauCours = new Cours
                {
                    IdCours      = newCoursId,
                    IdMatiere    = demande.IdMatiere.Value,
                    IdProfesseur = idProf,
                    IdClasse     = demande.IdClasse.Value,
                    IdSalle      = finalSalleId,
                    IdSemestre   = idSemestre,
                    IdAffectation = affectation?.IdAffectation,
                    TypeCours    = demande.TypeDemande == "examen" ? "examen" : "cours",
                    Statut       = "planifié"
                };
                db.Cours.Add(nouveauCours);

                int newCreneauId = (db.Creneaux.Max(c => (int?)c.IdCreneau) ?? 0) + 1;
                db.Creneaux.Add(new Creneau
                {
                    IdCreneau   = newCreneauId,
                    IdCours     = newCoursId,
                    JourSemaine = jourSemaine,
                    HeureDebut  = finalHeureDebut.Value,
                    HeureFin    = finalHeureFin.Value,
                    SemaineType = "A"
                });
            }

            var userCtx = UserContextHelper.FromRequest(ControllerContext);
            if (userCtx.IsAuthenticated && userCtx.Role == "validateur")
                demande.IdValidateur = userCtx.UserId;

            demande.Statut = "acceptee";

            try
            {
                db.SaveChanges();
                
                // --- AJOUT : Notification au professeur après validation ---
                int profIdToNotify = 0;
                if (demande.IdCours.HasValue && demande.Cours != null && demande.Cours.IdProfesseur.HasValue)
                {
                    profIdToNotify = demande.Cours.IdProfesseur.Value;
                }
                else
                {
                    var demandeurObj = db.Utilisateurs.Find(demande.IdDemandeur);
                    if (demandeurObj != null)
                    {
                        var profMatch = db.Professeurs.FirstOrDefault(p => p.Email == demandeurObj.Email);
                        if (profMatch != null) profIdToNotify = profMatch.IdProfesseur;
                    }
                }
                
                if (profIdToNotify > 0)
                {
                    db.Notifications.Add(new Notification
                    {
                        IdProfesseur = profIdToNotify,
                        Titre = "Proposition validée",
                        Message = $"Votre proposition de créneau a été validée pour la date du {finalDate:dd/MM/yyyy}.",
                        Type = "succes",
                        DateCreation = DateTime.UtcNow,
                        EstLue = false,
                        Lien = "/ProfesseurDashboard/MesDemandes"
                    });
                    db.SaveChanges();
                }
                // -----------------------------------------------------------
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message
                         ?? ex.InnerException?.Message
                         ?? ex.Message;
                return BadRequest("Erreur DB Validation: " + inner);
            }

            return Ok(demande);
        }

        [HttpPost]
        [Route("{id:int}/Refuser")]
        public IActionResult RefuserDemande(int id)
        {
            var demande = db.DemandesEdt.Include(d => d.Cours).FirstOrDefault(d => d.IdDemande == id);
            if (demande == null) return NotFound();

            var ctx = UserContextHelper.FromRequest(ControllerContext);
            if (ctx.IsAuthenticated && ctx.Role == "validateur")
                demande.IdValidateur = ctx.UserId;

            demande.Statut = "refusee";
            db.SaveChanges();
            
            // --- AJOUT : Notification au professeur après refus ---
            int profIdToNotify = 0;
            if (demande.IdCours.HasValue && demande.Cours != null && demande.Cours.IdProfesseur.HasValue)
            {
                profIdToNotify = demande.Cours.IdProfesseur.Value;
            }
            else
            {
                var demandeurObj = db.Utilisateurs.Find(demande.IdDemandeur);
                if (demandeurObj != null)
                {
                    var profMatch = db.Professeurs.FirstOrDefault(p => p.Email == demandeurObj.Email);
                    if (profMatch != null) profIdToNotify = profMatch.IdProfesseur;
                }
            }
            
            if (profIdToNotify > 0)
            {
                db.Notifications.Add(new Notification
                {
                    IdProfesseur = profIdToNotify,
                    Titre = "Proposition refusée",
                    Message = $"Votre proposition de créneau du {demande.DateSouhaitee:dd/MM/yyyy} a été refusée.",
                    Type = "erreur",
                    DateCreation = DateTime.UtcNow,
                    EstLue = false,
                    Lien = "/ProfesseurDashboard/MesDemandes"
                });
                db.SaveChanges();
            }
            // -----------------------------------------------------------
            
            return Ok(demande);
        }
    }
}
