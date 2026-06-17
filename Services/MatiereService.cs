using System;
using System.Collections.Generic;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public class ValidationResult
    {
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class MatiereService
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CM", "TD", "TP" };

        public ValidationResult ValidateMatiere(Matiere matiere)
        {
            var result = new ValidationResult();

            if (matiere == null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Matière est nulle.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(matiere.CodeMatiere))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Le code de la matière est requis.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(matiere.NomMatiere))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Le libellé de la matière est requis.";
                return result;
            }

            if (matiere.Credit <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Le nombre de crédits doit être positif.";
                return result;
            }

            if (matiere.VolumeHoraire <= 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Le volume horaire doit être positif.";
                return result;
            }

            // Vérifier le type de cours s'il existe (CM, TD, TP) – si la colonne n'est pas présente, ignore.
            // Ici nous supposons qu'un champ TypeMatiere pourrait être ajouté ultérieurement.
            // On démontre la logique de validation générique.
            // if (!string.IsNullOrWhiteSpace(matiere.Type) && !AllowedTypes.Contains(matiere.Type))
            // {
            //     result.IsSuccess = false;
            //     result.ErrorMessage = "Le type de matière doit être CM, TD ou TP.";
            //     return result;
            // }

            // Autres règles métiers peuvent être ajoutées ici, comme la cohérence avec Niveau, Mention, Parcours, Semestre.

            return result;
        }
    }
}
