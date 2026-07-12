using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion_SalleClasseEDT.Services
{
    /// <summary>
    /// Résultat de la génération de cours depuis une affectation.
    /// </summary>
    public class GenererCoursResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>IDs des Cours créés (CM, TD, TP selon volumes non nuls).</summary>
        public List<int> CoursIds { get; set; } = new();
    }

    /// <summary>
    /// Service métier pour les affectations de matières.
    /// Centralise la logique de génération des Cours (CM/TD/TP) depuis une AffectationMatiere
    /// pour éviter la duplication entre MatiereController et AffectationMatiereController.
    /// </summary>
    public interface IAffectationService
    {
        /// <summary>
        /// Génère les entités Cours (une par type CM/TD/TP non nul) pour une AffectationMatiere existante.
        /// Si des Cours existent déjà pour cette affectation, retourne ceux existants sans doublon.
        /// </summary>
        /// <param name="idAffectation">ID de l'AffectationMatiere source.</param>
        /// <returns>Résultat avec la liste des IDs de Cours créés ou existants.</returns>
        Task<GenererCoursResult> GenererCoursDepuisAffectationAsync(int idAffectation);
    }
}
