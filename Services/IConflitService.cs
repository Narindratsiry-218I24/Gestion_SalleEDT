using System;
using System.Threading.Tasks;

namespace Gestion_SalleClasseEDT.Services
{
    /// <summary>
    /// Type de ressource en conflit.
    /// </summary>
    public enum ConflitType
    {
        Salle,
        Classe,
        Professeur
    }

    /// <summary>
    /// Résultat d'une vérification de conflit d'horaire.
    /// </summary>
    public class ConflitResult
    {
        public bool HasConflict { get; set; }

        /// <summary>Null si pas de conflit.</summary>
        public ConflitType? ConflictType { get; set; }

        /// <summary>Message lisible décrivant le conflit.</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service centralisé de vérification de conflits d'horaires.
    /// Toute logique de chevauchement doit passer par ce service — jamais dupliquée dans les contrôleurs.
    /// Source de vérité : table <c>Seance</c> (dates exactes), pas <c>Creneau</c>.
    /// </summary>
    public interface IConflitService
    {
        /// <summary>
        /// Vérifie si le créneau (date + heureDebut..heureFin) est libre pour la salle, la classe et le professeur indiqués.
        /// Les paramètres nullables sont ignorés dans la vérification.
        /// </summary>
        /// <param name="date">Date exacte de la séance.</param>
        /// <param name="heureDebut">Heure de début.</param>
        /// <param name="heureFin">Heure de fin.</param>
        /// <param name="salleId">ID salle à vérifier (null = pas de vérif. salle).</param>
        /// <param name="classeId">ID classe à vérifier (null = pas de vérif. classe).</param>
        /// <param name="profId">ID professeur à vérifier (null = pas de vérif. prof).</param>
        /// <param name="excludeCoursId">ID cours à exclure des conflits (utile lors d'une mise à jour).</param>
        /// <param name="excludeSeanceId">ID séance à exclure (utile lors d'une mise à jour d'une séance existante).</param>
        /// <returns>ConflitResult : HasConflict=false si créneau libre.</returns>
        Task<ConflitResult> VerifierAsync(
            DateTime date,
            TimeSpan heureDebut,
            TimeSpan heureFin,
            int? salleId = null,
            int? classeId = null,
            int? profId = null,
            int? excludeCoursId = null,
            int? excludeSeanceId = null);
    }
}
