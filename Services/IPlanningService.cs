using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IPlanningService
    {
        Task<Cours> PlanifierCoursAsync(Cours cours, DateTime debut, DateTime fin);
        Task<IEnumerable<Creneau>> ObtenirEmploisDuTempsAsync();
    }
}