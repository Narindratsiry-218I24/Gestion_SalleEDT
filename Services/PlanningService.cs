using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Services
{
    public class PlanningException : Exception
    {
        public PlanningException(string message) : base(message) { }
    }

    public class PlanningService : IPlanningService
    {
        private readonly EMITDbContext _context;

        public PlanningService(EMITDbContext context)
        {
            _context = context;
        }

        public async Task<Cours> PlanifierCoursAsync(Cours cours, DateTime debut, DateTime fin)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var jour = debut.ToString("dddd");
                var heureDebut = debut.TimeOfDay;
                var heureFin = fin.TimeOfDay;

                // Validation of compatibility matter/class
                var matiere = await _context.Matieres.FindAsync(cours.IdMatiere);
                var classe = await _context.Classes.FindAsync(cours.IdClasse);
                
                if (matiere != null && classe != null && matiere.IdFiliere != classe.IdFiliere)
                {
                    throw new PlanningException("La matière n'est pas compatible avec la filière de cette classe.");
                }

                // 1. Conflit Professeur
                var conflitProf = await _context.Creneaux
                    .Include(c => c.Cours)
                    .AnyAsync(c => c.Cours.IdProfesseur == cours.IdProfesseur
                        && c.JourSemaine == jour
                        && ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin)
                         || (heureFin > c.HeureDebut && heureFin <= c.HeureFin)
                         || (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)));
                         
                if (conflitProf) throw new PlanningException("Le professeur a déjà un cours à cet horaire.");

                // 2. Conflit Classe
                var conflitClasse = await _context.Creneaux
                    .Include(c => c.Cours)
                    .AnyAsync(c => c.Cours.IdClasse == cours.IdClasse
                        && c.JourSemaine == jour
                        && ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin)
                         || (heureFin > c.HeureDebut && heureFin <= c.HeureFin)
                         || (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)));

                if (conflitClasse) throw new PlanningException("La classe a déjà un cours à cet horaire.");

                // 3. Attribution de Salle
                if (cours.IdSalle == null || cours.IdSalle == 0)
                {
                    var salles = await _context.Salles.OrderByDescending(s => s.Capacite).ToListAsync();
                    Salle salleAllouee = null;

                    foreach (var salle in salles)
                    {
                        var conflit = await _context.Creneaux
                            .Include(c => c.Cours)
                            .AnyAsync(c => c.Cours.IdSalle == salle.IdSalle
                                && c.JourSemaine == jour
                                && ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin)
                                 || (heureFin > c.HeureDebut && heureFin <= c.HeureFin)
                                 || (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)));

                        if (!conflit)
                        {
                            salleAllouee = salle;
                            break;
                        }
                    }

                    if (salleAllouee == null)
                        throw new PlanningException("Aucune salle disponible pour ce créneau.");

                    cours.IdSalle = salleAllouee.IdSalle;
                }
                else
                {
                    var conflitSalle = await _context.Creneaux
                        .Include(c => c.Cours)
                        .AnyAsync(c => c.Cours.IdSalle == cours.IdSalle
                            && c.JourSemaine == jour
                            && ((heureDebut >= c.HeureDebut && heureDebut < c.HeureFin)
                             || (heureFin > c.HeureDebut && heureFin <= c.HeureFin)
                             || (heureDebut <= c.HeureDebut && heureFin >= c.HeureFin)));

                    if (conflitSalle) throw new PlanningException("La salle spécifiée est déjà occupée à cet horaire.");
                }

                // Génération automatique d'ID pour Cours
                cours.IdCours = (await _context.Cours.MaxAsync(c => (int?)c.IdCours) ?? 0) + 1;
                
                _context.Cours.Add(cours);
                await _context.SaveChangesAsync();

                // Créer le créneau
                var creneau = new Creneau
                {
                    IdCreneau = (await _context.Creneaux.MaxAsync(c => (int?)c.IdCreneau) ?? 0) + 1,
                    IdCours = cours.IdCours,
                    JourSemaine = jour,
                    HeureDebut = heureDebut,
                    HeureFin = heureFin,
                    SemaineType = "A" // Par defaut
                };

                _context.Creneaux.Add(creneau);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return cours;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Creneau>> ObtenirEmploisDuTempsAsync()
        {
            return await _context.Creneaux
                .Include(c => c.Cours)
                    .ThenInclude(c => c.Matiere)
                .Include(c => c.Cours)
                    .ThenInclude(c => c.Salle)
                .Include(c => c.Cours)
                    .ThenInclude(c => c.Professeur)
                .Include(c => c.Cours)
                    .ThenInclude(c => c.Classe)
                .ToListAsync();
        }
    }
}