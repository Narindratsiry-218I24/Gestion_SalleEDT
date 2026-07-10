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
    }
}