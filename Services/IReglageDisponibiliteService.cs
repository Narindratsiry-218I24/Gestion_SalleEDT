using System;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IReglageDisponibiliteService
    {
        Task<ReglageDisponibilite> GetReglagePourAnneeAsync(int idAnnee);
        Task<ReglageDisponibilite> GetReglageOuDefautAsync(int idAnnee);
        Task<ReglageDisponibilite> SaveReglageAsync(ReglageDisponibilite reglage);
        Task<bool> EstHeureValideAsync(DateTime date, TimeSpan debut, TimeSpan fin, int idAnnee);
    }
}
