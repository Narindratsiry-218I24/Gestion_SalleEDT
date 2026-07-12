using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfesseurApiController : ControllerBase
    {
        private readonly EMITDbContext _db;

        public ProfesseurApiController(EMITDbContext context)
        {
            _db = context;
        }

        public class PlanifierCreneauxRequest
        {
            public string Email { get; set; }
            public List<CreneauDto> Creneaux { get; set; }
        }

        public class CreneauDto
        {
            public int? IdMatiere { get; set; }
            public int? IdClasse { get; set; }
            public int? IdSalle { get; set; }
            public string JourSemaine { get; set; }
            public string HeureDebut { get; set; }
            public string HeureFin { get; set; }
            public string TypeCours { get; set; } // CM, TD, TP
        }

        [HttpPost("sauvegarder-creneaux")]
        public async Task<IActionResult> SauvegarderCreneaux([FromBody] PlanifierCreneauxRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email requis." });
            }

            var prof = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.Email == request.Email);

            if (prof == null)
            {
                return NotFound(new { success = false, message = "Professeur non trouvé." });
            }

            if (prof.Utilisateur == null)
            {
                return BadRequest(new { success = false, message = "Aucun compte utilisateur lié à ce professeur." });
            }

            // Pour cet exemple de planification par le professeur,
            // on génère des "DemandeEdt" en lot ou des propositions pour validation admin.
            // On va créer des DemandesEdt de type "nouvelle_planification" 
            // pour chaque créneau pour que l'admin puisse les valider.
            
            var demandes = new List<DemandeEdt>();

            foreach (var dto in request.Creneaux)
            {
                if (!TimeSpan.TryParse(dto.HeureDebut, out var hd) || !TimeSpan.TryParse(dto.HeureFin, out var hf))
                {
                    continue;
                }

                // Pour simplifier on crée une demande avec une date souhaitée qui correspond 
                // au prochain jour de la semaine demandé ou on laisse sans date exacte, 
                // mais la demandeEdt nécessite de stocker l'info du jour.
                // On peut utiliser "Justification" pour stocker des métadonnées comme JourSemaine et TypeCours
                // Ou si on veut créer un vrai Creneau en attente, l'architecture EDT est complexe.
                // Disons qu'on crée des DemandeEdt.
                
                demandes.Add(new DemandeEdt
                {
                    IdDemandeur = prof.Utilisateur.IdUtilisateur,
                    TypeDemande = "nouvelle_planification",
                    Statut = "en_attente_admin",
                    IdMatiere = dto.IdMatiere,
                    IdClasse = dto.IdClasse,
                    IdSalle = dto.IdSalle,
                    HeureDebutSouhaitee = hd,
                    HeureFinSouhaitee = hf,
                    Justification = $"Planification souhaitée : {dto.JourSemaine} - {dto.TypeCours}"
                });
            }

            if (demandes.Any())
            {
                _db.DemandesEdt.AddRange(demandes);
                await _db.SaveChangesAsync();
            }

            return Ok(new { success = true, message = $"{demandes.Count} créneaux proposés pour validation." });
        }
    }
}
