using System;
using System.Linq;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public class ReglageDisponibiliteService : IReglageDisponibiliteService
    {
        private readonly EMITDbContext _db;

        public ReglageDisponibiliteService(EMITDbContext db)
        {
            _db = db;
        }

        public async Task<ReglageDisponibilite> GetReglagePourAnneeAsync(int idAnnee)
        {
            var reglage = _db.ReglagesDisponibilite.FirstOrDefault(r => r.IdAnnee == idAnnee);
            return reglage ?? await GetReglageOuDefautAsync(idAnnee);
        }

        public Task<ReglageDisponibilite> GetReglageOuDefautAsync(int idAnnee)
        {
            var defaultReglage = new ReglageDisponibilite
            {
                IdAnnee = idAnnee,
                DimancheAutorise = false,
                SamediAutorise = true,
                HeureOuverture = new TimeSpan(7, 0, 0),
                HeureFermeture = new TimeSpan(18, 0, 0),
                DureeMinCreneauMinutes = 60
            };
            return Task.FromResult(defaultReglage);
        }

        public async Task<ReglageDisponibilite> SaveReglageAsync(ReglageDisponibilite reglage)
        {
            if (reglage == null) throw new ArgumentNullException(nameof(reglage));

            var existing = _db.ReglagesDisponibilite.FirstOrDefault(r => r.IdAnnee == reglage.IdAnnee);
            if (existing == null)
            {
                _db.ReglagesDisponibilite.Add(reglage);
            }
            else
            {
                existing.DimancheAutorise = reglage.DimancheAutorise;
                existing.SamediAutorise = reglage.SamediAutorise;
                existing.HeureOuverture = reglage.HeureOuverture;
                existing.HeureFermeture = reglage.HeureFermeture;
                existing.DureeMinCreneauMinutes = reglage.DureeMinCreneauMinutes;
            }

            await _db.SaveChangesAsync();
            return reglage;
        }

        public async Task<bool> EstHeureValideAsync(DateTime date, TimeSpan debut, TimeSpan fin, int idAnnee)
        {
            var reglage = await GetReglagePourAnneeAsync(idAnnee);

            if (debut >= fin) return false;
            if (debut < reglage.HeureOuverture || fin > reglage.HeureFermeture) return false;

            if (date.DayOfWeek == DayOfWeek.Sunday && !reglage.DimancheAutorise) return false;
            if (date.DayOfWeek == DayOfWeek.Saturday && !reglage.SamediAutorise) return false;

            return true;
        }
    }
}
