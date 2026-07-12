using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    /// <summary>
    /// Implémentation de IAffectationService.
    /// Extrait de AffectationMatiereController.GenererCours pour permettre la réutilisation
    /// depuis MatiereController (et tout autre point d'entrée futur).
    /// </summary>
    public class AffectationService : IAffectationService
    {
        private readonly EMITDbContext _db;

        public AffectationService(EMITDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<GenererCoursResult> GenererCoursDepuisAffectationAsync(int idAffectation)
        {
            var affectation = await _db.AffectationsMatieres
                .Include(a => a.Matiere)
                .FirstOrDefaultAsync(a => a.IdAffectation == idAffectation);

            if (affectation == null)
                return new GenererCoursResult { Success = false, Message = "Affectation introuvable." };

            if (!affectation.EstActif)
                return new GenererCoursResult { Success = false, Message = "L'affectation est inactive." };

            // Idempotence : retourner les cours existants sans dupliquer
            var coursExistants = await _db.Cours
                .Where(c => c.IdAffectation == idAffectation)
                .ToListAsync();

            if (coursExistants.Any())
            {
                return new GenererCoursResult
                {
                    Success = true,
                    Message = "Cours déjà générés (existants retournés).",
                    CoursIds = coursExistants.Select(c => c.IdCours).ToList()
                };
            }

            // Générer un Cours par type d'heure non nul (CM / TD / TP)
            var typesACreer = new[]
            {
                new { Type = "CM", Heures = affectation.HeuresCm },
                new { Type = "TD", Heures = affectation.HeuresTd },
                new { Type = "TP", Heures = affectation.HeuresTp }
            }.Where(x => x.Heures > 0).ToList();

            // Fallback : si aucun type spécifique, créer un CM générique avec le volume total
            if (!typesACreer.Any())
            {
                typesACreer = new[]
                {
                    new { Type = "CM", Heures = affectation.VolumeHoraireTotal }
                }.ToList();
            }

            var nouveauxCours = typesACreer.Select(x => new Cours
            {
                IdMatiere    = affectation.IdMatiere,
                IdProfesseur = affectation.IdProfesseur,
                IdClasse     = affectation.IdClasse,
                IdSemestre   = affectation.IdSemestre,
                IdAffectation = affectation.IdAffectation,
                TypeCours    = x.Type,
                VolumeHours  = x.Heures,
                Statut       = "Cree"
            }).ToList();

            _db.Cours.AddRange(nouveauxCours);
            await _db.SaveChangesAsync();

            return new GenererCoursResult
            {
                Success = true,
                Message = $"{nouveauxCours.Count} cours générés avec succès.",
                CoursIds = nouveauxCours.Select(c => c.IdCours).ToList()
            };
        }
    }
}
