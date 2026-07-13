using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    /// <summary>
    /// Dashboard MVC du professeur connecté.
    /// Le professeur est identifié via l'email stocké dans localStorage (passé en query string ou session).
    /// Ici on utilise une approche simple : l'email du professeur peut être passé en paramètre pour l'instant.
    /// En production, utiliser des sessions ou claims.
    /// </summary>
    public class ProfesseurDashboardController : Controller
    {
        private readonly EMITDbContext _db;

        public ProfesseurDashboardController(EMITDbContext context)
        {
            _db = context;
        }

        // ─────────────────────────────────────────────────────────────
        // Helper: récupérer le professeur depuis l'email en session/query
        // ─────────────────────────────────────────────────────────────
        private async Task<Professeur?> GetProfesseurFromRequest()
        {
            // Priorité : query param "email" → header X-User-Email → null
            var email = Request.Query["email"].ToString()
                     ?? Request.Headers["X-User-Email"].ToString();

            if (string.IsNullOrEmpty(email))
                return null;

            return await _db.Professeurs
                .Include(p => p.Utilisateur)
                .Include(p => p.AffectationsMatieres)
                    .ThenInclude(a => a.Matiere)
                .Include(p => p.AffectationsMatieres)
                    .ThenInclude(a => a.Classe)
                .Include(p => p.AffectationsMatieres)
                    .ThenInclude(a => a.Semestre)
                        .ThenInclude(s => s.RefSemestre)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Matiere)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Classe)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Salle)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.AffectationMatiere)
                .Include(p => p.Cours)
                    .ThenInclude(c => c.Creneaux)
                .Include(p => p.Disponibilites)
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        // ─────────────────────────────────────────────────────────────
        // DASHBOARD — Vue principale
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null)
                return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // AGENDA — créneaux de la semaine courante
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Agenda(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // MES COURS — liste des cours affectés
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> MesCours(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // PLANIFIER CRÉNEAUX — interface interactive
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> PlanifierCreneaux(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            
            // Charger les affectations du professeur
            var mesAffectations = prof.AffectationsMatieres?.ToList() ?? new List<AffectationMatiere>();
            var mesClassesIds = mesAffectations.Select(a => a.IdClasse).Distinct().ToList();
            var mesMatieresIds = mesAffectations.Select(a => a.IdMatiere).Distinct().ToList();

            vm.Classes = await _db.Classes
                .Include(c => c.Filiere).ThenInclude(f => f.Mention)
                .Include(c => c.Niveau)
                .Where(c => mesClassesIds.Contains(c.IdClasse))
                .OrderBy(c => c.NomClasse)
                .ToListAsync();
                
            vm.Matieres = await _db.Matieres
                .Where(m => mesMatieresIds.Contains(m.IdMatiere))
                .OrderBy(m => m.NomMatiere)
                .ToListAsync();
                
            vm.Salles = await _db.Salles.OrderBy(s => s.NomSalle).ToListAsync();

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // NOUVELLE DEMANDE — formulaire
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> NouvelleDemande(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            
            // Charger données pour le formulaire: UNIQUEMENT Celles enseignées par le prof
            var mesAffectations = prof.AffectationsMatieres?.ToList() ?? new List<AffectationMatiere>();
            
            var mesClassesIds = mesAffectations.Select(a => a.IdClasse).Distinct().ToList();
            var mesMatieresIds = mesAffectations.Select(a => a.IdMatiere).Distinct().ToList();

            var heuresPlanifieesDict = new System.Collections.Generic.Dictionary<string, int>();
            if (prof.Cours != null)
            {
                foreach (var c in prof.Cours)
                {
                    var key = $"{c.IdMatiere}|{c.IdClasse}";
                    var heures = c.Creneaux?.Sum(cr => (int)(cr.HeureFin - cr.HeureDebut).TotalHours) ?? 0;
                    
                    if (heuresPlanifieesDict.ContainsKey(key))
                        heuresPlanifieesDict[key] += heures;
                    else
                        heuresPlanifieesDict[key] = heures;
                }
            }
            ViewBag.HeuresPlanifiees = heuresPlanifieesDict;

            vm.Classes = await _db.Classes
                .Include(c => c.Filiere).ThenInclude(f => f.Mention)
                .Include(c => c.Niveau)
                .Include(c => c.AnneeAcademique)
                .Where(c => mesClassesIds.Contains(c.IdClasse))
                .OrderBy(c => c.NomClasse)
                .ToListAsync();
                
            vm.Matieres = await _db.Matieres
                .Include(m => m.RefSemestre).ThenInclude(s => s.Niveau)
                .Include(m => m.Filiere).ThenInclude(f => f.Mention)
                .Where(m => mesMatieresIds.Contains(m.IdMatiere))
                .OrderBy(m => m.NomMatiere)
                .ToListAsync();
                
            vm.Salles = await _db.Salles.OrderBy(s => s.NomSalle).ToListAsync();
            
            if (prof.Utilisateur != null)
            {
                vm.MesDemandes = await _db.DemandesEdt
                    .Where(d => d.IdDemandeur == prof.Utilisateur.IdUtilisateur)
                    .Include(d => d.Cours).ThenInclude(c => c.Matiere)
                    .Include(d => d.Matiere)
                    .Include(d => d.Classe)
                    .Include(d => d.Salle)
                    .Include(d => d.Propositions)
                    .OrderByDescending(d => d.IdDemande)
                    .Take(5) // only recent 5 for sidebar
                    .ToListAsync();
            }

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // SOUMETTRE DEMANDE — POST
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoumettreDemande(
            string email,
            string typeDemande,
            int? idCours,
            int? idMatiere,
            int? idClasse,
            int? idSalle,
            DateTime? dateSouhaitee,
            string? heureDebut,
            string? heureFin,
            string? justification)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            if (prof.Utilisateur == null)
            {
                TempData["Error"] = "Votre compte utilisateur n'est pas lié. Contactez l'administration.";
                return RedirectToAction(nameof(NouvelleDemande), new { email });
            }

            var demande = new DemandeEdt
            {
                IdDemandeur     = prof.Utilisateur.IdUtilisateur,
                TypeDemande     = typeDemande ?? "modification",
                Statut          = "en_attente_admin",
                IdCours         = idCours,
                IdMatiere       = idMatiere,
                IdClasse        = idClasse,
                IdSalle         = idSalle,
                DateSouhaitee   = dateSouhaitee.HasValue
                    ? DateTime.SpecifyKind(dateSouhaitee.Value, DateTimeKind.Utc)
                    : null,
                HeureDebutSouhaitee = ParseTime(heureDebut),
                HeureFinSouhaitee   = ParseTime(heureFin),
                Justification       = justification
            };

            _db.DemandesEdt.Add(demande);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Votre demande a été soumise avec succès et est en attente de validation.";
            return RedirectToAction(nameof(MesDemandes), new { email });
        }

        // ─────────────────────────────────────────────────────────────
        // MES DEMANDES — historique des requêtes
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> MesDemandes(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();

            if (prof.Utilisateur != null)
            {
                vm.MesDemandes = await _db.DemandesEdt
                    .Where(d => d.IdDemandeur == prof.Utilisateur.IdUtilisateur)
                    .Include(d => d.Cours).ThenInclude(c => c.Matiere)
                    .Include(d => d.Matiere)
                    .Include(d => d.Classe)
                    .Include(d => d.Salle)
                    .Include(d => d.Propositions)
                    .OrderByDescending(d => d.IdDemande)
                    .ToListAsync();
            }

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // DISPONIBILITÉS — gérer les créneaux disponibles
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Disponibilites(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            return View(vm);
        }

        public async Task<IActionResult> Notifications(string email)
        {
            var prof = await GetProfesseurFromRequest();
            if (prof == null) return View("ProfNonTrouve");

            var vm = new ProfesseurDashboardViewModel(prof, _db);
            await vm.LoadAsync();
            
            ViewBag.ToutesNotifications = await _db.Notifications
                .Where(n => n.IdProfesseur == prof.IdProfesseur)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();

            return View(vm);
        }



        // ─────────────────────────────────────────────────────────────
        // Helper: parser HH:mm → TimeSpan?
        // ─────────────────────────────────────────────────────────────
        private static TimeSpan? ParseTime(string? t)
        {
            if (string.IsNullOrWhiteSpace(t)) return null;
            if (TimeSpan.TryParse(t, out var ts)) return ts;
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // VIEW MODEL
    // ─────────────────────────────────────────────────────────────
    public class ProfesseurDashboardViewModel
    {
        private readonly EMITDbContext _db;

        public Professeur Professeur { get; set; }

        // Stats
        public int NbCoursCetteSemaine { get; set; }
        public int NbHeuresCetteSemaine { get; set; }
        public int NbDemandesEnAttente { get; set; }
        public int NbMatieres { get; set; }

        // Agenda (semaine courante)
        public System.Collections.Generic.List<CreneauAgenda> CreneauxSemaine { get; set; } = new();

        // Calendrier
        public int CalendrierMois { get; set; } = DateTime.Now.Month;
        public int CalendrierAnnee { get; set; } = DateTime.Now.Year;

        // Demandes
        public System.Collections.Generic.List<DemandeEdt> MesDemandes { get; set; } = new();

        // Pour le formulaire
        public System.Collections.Generic.List<Classe> Classes { get; set; } = new();
        public System.Collections.Generic.List<Matiere> Matieres { get; set; } = new();
        public System.Collections.Generic.List<Salle> Salles { get; set; } = new();

        public ProfesseurDashboardViewModel(Professeur prof, EMITDbContext db)
        {
            Professeur = prof;
            _db = db;
        }

        public async Task LoadAsync()
        {
            NbMatieres = Professeur.AffectationsMatieres?.Count ?? 0;

            // Cours de cette semaine (basé sur les séances réelles de la table seance)
            var today = DateTime.UtcNow.Date;
            var startOfWeek = DateTime.SpecifyKind(today.AddDays(-(int)today.DayOfWeek + 1), DateTimeKind.Utc); // Lundi
            var endOfWeek = startOfWeek.AddDays(7);

            var seances = await _db.Seances
                .Include(s => s.Cours.Matiere)
                .Include(s => s.Cours.Classe)
                .Include(s => s.Salle)
                .Where(s => s.Cours.IdProfesseur == Professeur.IdProfesseur && s.Statut != "Annulee" && s.Date >= startOfWeek && s.Date < endOfWeek)
                .ToListAsync();

            var creneaux = seances.Select(s => new CreneauAgenda
            {
                Cours         = s.Cours,
                Creneau       = new Creneau 
                { 
                    JourSemaine = s.Date.DayOfWeek switch
                    {
                        DayOfWeek.Monday => "MON",
                        DayOfWeek.Tuesday => "TUE",
                        DayOfWeek.Wednesday => "WED",
                        DayOfWeek.Thursday => "THU",
                        DayOfWeek.Friday => "FRI",
                        DayOfWeek.Saturday => "SAT",
                        DayOfWeek.Sunday => "SUN",
                        _ => ""
                    },
                    HeureDebut = s.StartTime, 
                    HeureFin = s.EndTime 
                },
                NomMatiere    = s.Cours?.Matiere?.NomMatiere ?? "—",
                NomClasse     = s.Cours?.Classe?.NomClasse ?? "—",
                NomSalle      = s.Salle?.NomSalle ?? "Non assigné",
                TypeCours     = s.Cours?.TypeCours ?? "cours",
                JourSemaine   = s.Date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "MON",
                    DayOfWeek.Tuesday => "TUE",
                    DayOfWeek.Wednesday => "WED",
                    DayOfWeek.Thursday => "THU",
                    DayOfWeek.Friday => "FRI",
                    DayOfWeek.Saturday => "SAT",
                    DayOfWeek.Sunday => "SUN",
                    _ => ""
                },
                HeureDebut    = s.StartTime,
                HeureFin      = s.EndTime
            }).ToList();

            CreneauxSemaine = creneaux;
            NbCoursCetteSemaine = creneaux.Count;
            NbHeuresCetteSemaine = creneaux.Sum(c =>
                (int)(c.HeureFin - c.HeureDebut).TotalHours);

            if (Professeur.Utilisateur != null)
            {
                NbDemandesEnAttente = await _db.DemandesEdt
                    .CountAsync(d => d.IdDemandeur == Professeur.Utilisateur.IdUtilisateur
                                  && d.Statut == "en_attente_admin");
            }
        }
    }

    public class CreneauAgenda
    {
        public Cours Cours { get; set; }
        public Creneau Creneau { get; set; }
        public string NomMatiere { get; set; }
        public string NomClasse { get; set; }
        public string NomSalle { get; set; }
        public string TypeCours { get; set; }
        public string JourSemaine { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }

        public string JourLabel => JourSemaine switch
        {
            "MON" => "Lundi",
            "TUE" => "Mardi",
            "WED" => "Mercredi",
            "THU" => "Jeudi",
            "FRI" => "Vendredi",
            "SAT" => "Samedi",
            _ => JourSemaine
        };

        public int JourOrdre => JourSemaine switch
        {
            "MON" => 1, "TUE" => 2, "WED" => 3, "THU" => 4, "FRI" => 5, "SAT" => 6, _ => 7
        };
    }
}
