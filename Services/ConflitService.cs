using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    /// <summary>
    /// Implémentation de IConflitService.
    /// Vérifie les chevauchements sur la table <c>Seance</c> (source de vérité unique pour l'état réel).
    /// L'ordre de vérification est : Salle → Professeur → Classe (du plus bloquant au moins bloquant).
    /// </summary>
    public class ConflitService : IConflitService
    {
        private readonly EMITDbContext _db;

        public ConflitService(EMITDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<ConflitResult> VerifierAsync(
            DateTime date,
            TimeSpan heureDebut,
            TimeSpan heureFin,
            int? salleId = null,
            int? classeId = null,
            int? profId = null,
            int? excludeCoursId = null,
            int? excludeSeanceId = null)
        {
            // Base query : séances non annulées sur la même date qui se chevauchent
            var seancesQuery = _db.Seances
                .Include(s => s.Cours)
                .Where(s =>
                    s.Date == date.Date &&
                    s.Statut != "Annulee" &&
                    // Chevauchement : (début1 < fin2) ET (fin1 > début2)
                    s.StartTime < heureFin &&
                    s.EndTime > heureDebut);

            // Exclure le cours lui-même si mise à jour
            if (excludeCoursId.HasValue)
                seancesQuery = seancesQuery.Where(s => s.CourseId != excludeCoursId.Value);

            // Exclure la séance elle-même si mise à jour d'une séance
            if (excludeSeanceId.HasValue)
                seancesQuery = seancesQuery.Where(s => s.IdSeance != excludeSeanceId.Value);

            // Charger une seule fois en mémoire pour effectuer les 3 vérifications
            var seancesEnConflit = await seancesQuery.ToListAsync();

            // --- 1. Conflit de salle ---
            if (salleId.HasValue && salleId.Value > 0)
            {
                var conflitSalle = seancesEnConflit.FirstOrDefault(s => s.SalleId == salleId.Value);
                if (conflitSalle != null)
                {
                    var matiere = await _db.Matieres.FindAsync(conflitSalle.Cours?.IdMatiere);
                    return new ConflitResult
                    {
                        HasConflict = true,
                        ConflictType = ConflitType.Salle,
                        Message = $"La salle est déjà occupée à ce créneau" +
                                  (matiere != null ? $" par « {matiere.NomMatiere} »" : "") + "."
                    };
                }
            }

            // --- 2. Conflit de professeur ---
            if (profId.HasValue && profId.Value > 0)
            {
                var conflitProf = seancesEnConflit.FirstOrDefault(s => s.Cours?.IdProfesseur == profId.Value);
                if (conflitProf != null)
                {
                    var matiere = await _db.Matieres.FindAsync(conflitProf.Cours?.IdMatiere);
                    return new ConflitResult
                    {
                        HasConflict = true,
                        ConflictType = ConflitType.Professeur,
                        Message = $"Le professeur enseigne déjà" +
                                  (matiere != null ? $" « {matiere.NomMatiere} »" : " un autre cours") +
                                  " à ce créneau."
                    };
                }
            }

            // --- 3. Conflit de classe ---
            if (classeId.HasValue && classeId.Value > 0)
            {
                var conflitClasse = seancesEnConflit.FirstOrDefault(s => s.Cours?.IdClasse == classeId.Value);
                if (conflitClasse != null)
                {
                    var matiere = await _db.Matieres.FindAsync(conflitClasse.Cours?.IdMatiere);
                    return new ConflitResult
                    {
                        HasConflict = true,
                        ConflictType = ConflitType.Classe,
                        Message = $"La classe a déjà un cours planifié à ce créneau" +
                                  (matiere != null ? $" (« {matiere.NomMatiere} »)" : "") + "."
                    };
                }
            }

            return new ConflitResult { HasConflict = false };
        }
    }
}
