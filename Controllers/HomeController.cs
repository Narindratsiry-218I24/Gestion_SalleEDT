using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EMITDbContext _context;

    public HomeController(ILogger<HomeController> logger, EMITDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Connexion()
    {
        return View();
    }

    public IActionResult Inscription()
    {
        return View();
    }

    public IActionResult Parametres()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Cours(
        string? mention = null,
        string? niveau = null,
        string? parcours = null,
        string? search = null,
        string? statut = null,
        int page = 1,
        int pageSize = 10)
    {
        // Construction de la requête de base
        var query = _context.Cours
            .Include(c => c.Matiere)
                .ThenInclude(m => m.Filiere)
                .ThenInclude(f => f.Mention)
            .Include(c => c.Matiere)
                .ThenInclude(m => m.RefSemestre)
                .ThenInclude(r => r.Niveau)
            .Include(c => c.Professeur)
            .Include(c => c.Classe)
                .ThenInclude(cl => cl.Filiere)
            .Include(c => c.Seances)
                .ThenInclude(s => s.Salle)
            .AsQueryable();

        // Filtre par Mention (via la filière de la matière)
        if (!string.IsNullOrEmpty(mention))
        {
            query = query.Where(c => c.Matiere.Filiere.Mention.NomMention == mention);
        }

        // Filtre par Niveau (via le RefSemestre de la matière)
        if (!string.IsNullOrEmpty(niveau))
        {
            var niveauCode = GetNiveauCode(niveau);
            if (!string.IsNullOrEmpty(niveauCode))
            {
                query = query.Where(c => c.Matiere.RefSemestre.Niveau.CodeNiveau == niveauCode);
            }
        }

        // Filtre par Parcours (via le CodeFiliere de la matière)
        if (!string.IsNullOrEmpty(parcours))
        {
            query = query.Where(c => c.Matiere.Filiere.CodeFiliere == parcours);
        }

        // Recherche par nom/code matière ou enseignant
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c => 
                c.Matiere.NomMatiere.ToLower().Contains(searchLower) ||
                c.Matiere.CodeMatiere.ToLower().Contains(searchLower) ||
                (c.Professeur != null && c.Professeur.Nom.ToLower().Contains(searchLower)) ||
                (c.Professeur != null && c.Professeur.Prenom.ToLower().Contains(searchLower))
            );
        }

        // Filtre par statut
        if (!string.IsNullOrEmpty(statut))
        {
            query = query.Where(c => c.Statut == statut);
        }

        // Compter le total avant pagination
        var totalItems = await query.CountAsync();

        // Appliquer la pagination
        var items = await query
            .OrderByDescending(c => c.IdCours)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Charger les données pour les filtres
        var mentions = await _context.Mentions.ToListAsync();
        var niveaux = await _context.Niveaux.ToListAsync();
        var filieres = await _context.Filieres.ToListAsync();

        // Créer le ViewModel
        var viewModel = new CoursListViewModel
        {
            Cours = items,
            TotalItems = totalItems,
            CurrentPage = page,
            PageSize = pageSize,
            FilterMention = mention,
            FilterNiveau = niveau,
            FilterParcours = parcours,
            SearchTerm = search,
            FilterStatut = statut
        };

        // Passer les données pour les filtres via ViewBag
        ViewBag.Mentions = mentions;
        ViewBag.Niveaux = niveaux;
        ViewBag.Filieres = filieres;
        ViewBag.Statuts = new List<string> 
        { 
            "Cree", "EnAttente", "Planifie", "EnCours", "Suspendu", "Termine", "Archive" 
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetCoursByFilters(
        string? mention = null,
        string? niveau = null,
        string? parcours = null)
    {
        // API endpoint pour récupérer les cours filtrés (utilisé par AJAX)
        var query = _context.Cours
            .Include(c => c.Matiere)
                .ThenInclude(m => m.Filiere)
                .ThenInclude(f => f.Mention)
            .Include(c => c.Matiere)
                .ThenInclude(m => m.RefSemestre)
                .ThenInclude(r => r.Niveau)
            .Include(c => c.Professeur)
            .Include(c => c.Classe)
            .Include(c => c.Seances)
                .ThenInclude(s => s.Salle)
            .AsQueryable();

        if (!string.IsNullOrEmpty(mention))
        {
            query = query.Where(c => c.Matiere.Filiere.Mention.NomMention == mention);
        }

        if (!string.IsNullOrEmpty(niveau))
        {
            var niveauCode = GetNiveauCode(niveau);
            if (!string.IsNullOrEmpty(niveauCode))
            {
                query = query.Where(c => c.Matiere.RefSemestre.Niveau.CodeNiveau == niveauCode);
            }
        }

        if (!string.IsNullOrEmpty(parcours))
        {
            query = query.Where(c => c.Matiere.Filiere.CodeFiliere == parcours);
        }

        var result = await query
            .OrderByDescending(c => c.IdCours)
            .Select(c => new
            {
                c.IdCours,
                MatiereNom = c.Matiere.NomMatiere,
                MatiereCode = c.Matiere.CodeMatiere,
                ProfesseurNom = c.Professeur != null ? c.Professeur.Nom + " " + c.Professeur.Prenom : "Non assigné",
                ClasseNom = c.Classe != null ? c.Classe.NomClasse : "Non assignée",
                c.Statut,
                c.VolumeHours,
                SeancesCount = c.Seances.Count,
                HasSeances = c.Seances.Any(),
                Progression = c.VolumeHours > 0 ? (double)c.Seances.Where(s => s.Statut == "Realisee").Sum(s => (s.EndTime - s.StartTime).TotalHours) / c.VolumeHours * 100 : 0,
                NextSeance = c.Seances.Where(s => s.Date >= DateTime.Today).OrderBy(s => s.Date).ThenBy(s => s.StartTime).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(result);
    }

    public async Task<IActionResult> EDT()
    {
        var seances = await _context.Seances
            .Include(s => s.Cours)
                .ThenInclude(c => c.Matiere)
            .Include(s => s.Cours)
                .ThenInclude(c => c.Professeur)
            .Include(s => s.Salle)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
        
        return View(seances);
    }

    public IActionResult Professeurs()
    {
        return View();
    }

    public IActionResult Requetes()
    {
        return View();
    }

    public IActionResult Salles()
    {
        return View();
    }

    public IActionResult Structures()
    {
        return View();
    }

    public IActionResult Matieres()
    {
        return View();
    }

    public IActionResult Validation()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CourseDetails(int id)
    {
        var course = await _context.Cours
            .Include(c => c.Matiere)
                .ThenInclude(m => m.Filiere)
                .ThenInclude(f => f.Mention)
            .Include(c => c.Matiere)
                .ThenInclude(m => m.RefSemestre)
                .ThenInclude(r => r.Niveau)
            .Include(c => c.Professeur)
            .Include(c => c.Classe)
                .ThenInclude(cl => cl.Filiere)
            .Include(c => c.Seances)
                .ThenInclude(s => s.Salle)
            .Include(c => c.Groupe)
            .FirstOrDefaultAsync(c => c.IdCours == id);
        
        if (course == null) return NotFound();

        // Charger les salles disponibles pour la planification
        ViewBag.Salles = await _context.Salles.ToListAsync();
        ViewBag.Professeurs = await _context.Professeurs.ToListAsync();

        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> PlanifierSeance(int id, [FromBody] PlanificationRequest request)
    {
        try
        {
            var cours = await _context.Cours
                .Include(c => c.Seances)
                .FirstOrDefaultAsync(c => c.IdCours == id);

            if (cours == null)
                return NotFound(new { Message = "Cours non trouvé" });

            // Vérifier les conflits
            var conflit = await VerifierConflits(request, cours);
            if (conflit != null)
                return BadRequest(new { Message = conflit });

            // Créer la séance
            var seance = new Seance
            {
                IdCours = id,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IdSalle = request.SalleId,
                GroupeId = request.GroupeId,
                Statut = "Planifiee"
            };

            _context.Seances.Add(seance);
            
            // Mettre à jour le statut du cours si nécessaire
            if (cours.Statut == "Cree" || cours.Statut == "EnAttente")
            {
                cours.Statut = "Planifie";
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                Message = "Séance planifiée avec succès",
                SeanceId = seance.IdSeance
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Erreur lors de la planification: {ex.Message}" });
        }
    }

    private async Task<string?> VerifierConflits(PlanificationRequest request, Cours cours)
    {
        // Vérifier conflit professeur
        if (request.ProfesseurId.HasValue)
        {
            var conflitProf = await _context.Seances
                .Where(s => s.IdCours != cours.IdCours && 
                       s.Date == request.Date &&
                       s.StartTime < request.EndTime &&
                       s.EndTime > request.StartTime &&
                       s.Cours.IdProfesseur == request.ProfesseurId)
                .Select(s => s.Cours.Matiere.NomMatiere)
                .FirstOrDefaultAsync();

            if (conflitProf != null)
                return $"Le professeur est déjà occupé avec {conflitProf} à ce créneau.";
        }

        // Vérifier conflit salle
        if (request.SalleId.HasValue)
        {
            var conflitSalle = await _context.Seances
                .Where(s => s.IdCours != cours.IdCours && 
                       s.Date == request.Date &&
                       s.StartTime < request.EndTime &&
                       s.EndTime > request.StartTime &&
                       s.IdSalle == request.SalleId)
                .Select(s => s.Cours.Matiere.NomMatiere)
                .FirstOrDefaultAsync();

            if (conflitSalle != null)
                return $"La salle est déjà occupée avec {conflitSalle} à ce créneau.";
        }

        // Vérifier conflit classe
        if (cours.IdClasse.HasValue)
        {
            var conflitClasse = await _context.Seances
                .Where(s => s.IdCours != cours.IdCours && 
                       s.Date == request.Date &&
                       s.StartTime < request.EndTime &&
                       s.EndTime > request.StartTime &&
                       s.Cours.IdClasse == cours.IdClasse)
                .Select(s => s.Cours.Matiere.NomMatiere)
                .FirstOrDefaultAsync();

            if (conflitClasse != null)
                return $"La classe est déjà occupée avec {conflitClasse} à ce créneau.";
        }

        return null;
    }

    private string? GetNiveauCode(string niveauName)
    {
        return niveauName switch
        {
            "Licence 1" => "L1",
            "Licence 2" => "L2",
            "Licence 3" => "L3",
            "Master 1" => "M1",
            "Master 2" => "M2",
            _ => null
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class PlanificationRequest
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int? SalleId { get; set; }
    public int? GroupeId { get; set; }
    public int? ProfesseurId { get; set; }
}