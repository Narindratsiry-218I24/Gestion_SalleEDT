using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IPlanningService
    {
        Task<Cours> PlanifierCoursAsync(Cours course);
        Task<Seance> PlanifierSeanceAsync(int courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? salleId, int? groupeId);
        Task<IEnumerable<Seance>> ObtenirEmploisDuTempsAsync();
        Task RecalculerStatutCoursAsync(int coursId);
        Task<ConflitPlanificationResult> VerifierPlanificationAsync(int coursId, DateTime date, TimeSpan debut, TimeSpan fin, int? salleId);
    }

    public class ConflitPlanificationResult
    {
        public bool HasConflict { get; set; }
        public string? ConflictType { get; set; } // "Professeur" | "Salle" | "Classe" | "CapaciteSalle" | "DisponibiliteProf" | "VolumeDepasse"
        public string Message { get; set; } = "";
        public bool IsWarningOnly { get; set; }
    }
}