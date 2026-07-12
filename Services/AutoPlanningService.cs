using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    [Obsolete("Utiliser PlanningService avec le modèle Seance à la place.")]
    public interface IAutoPlanningService
    {
        Task<bool> PlanifierAnneeAsync(int anneeId);
        Task<bool> PlanifierClasseAsync(int classeId);
        Task<List<Cours>> ResoudreConflitsAsync(List<Cours> coursNonPlanifies);
        Task<bool> OptimiserRepartitionAsync();
    }

    [Obsolete("Utiliser PlanningService avec le modèle Seance à la place.")]
    public class AutoPlanningService : IAutoPlanningService
    {
        private readonly EMITDbContext _db;
        private readonly string[] _jours = { "MON", "TUE", "WED", "THU", "FRI", "SAT" };
        private readonly TimeSpan _heureMin = TimeSpan.FromHours(7);
        private readonly TimeSpan _heureMax = TimeSpan.FromHours(20);

        public AutoPlanningService(EMITDbContext db)
        {
            _db = db;
        }

        public async Task<bool> PlanifierAnneeAsync(int anneeId)
        {
            var classes = await _db.Classes.Where(c => c.IdAnneeAcademique == anneeId).ToListAsync();
            bool allSuccess = true;

            foreach (var cls in classes)
            {
                var result = await PlanifierClasseAsync(cls.IdClasse);
                if (!result) allSuccess = false;
            }

            return allSuccess;
        }

        public async Task<bool> PlanifierClasseAsync(int classeId)
        {
            // 1. Load affectations for the class that have no associated Cours or Cours without Creneaux
            var affectations = await _db.AffectationsMatieres
                .Include(a => a.Professeur).ThenInclude(p => p.Disponibilites)
                .Include(a => a.Cours).ThenInclude(c => c.Creneaux)
                .Where(a => a.IdClasse == classeId && a.EstActif == true)
                .OrderByDescending(a => a.VolumeHoraireTotal) // Heuristique: plus grande charge d'abord
                .ToListAsync();

            if (!affectations.Any()) return true;

            // Load existing context to check for constraints
            var allCreneaux = await _db.Creneaux.Include(c => c.Cours).ToListAsync();
            var toutesSalles = await _db.Salles.ToListAsync();

            foreach (var aff in affectations)
            {
                var cours = aff.Cours?.FirstOrDefault();
                if (cours == null)
                {
                    cours = new Cours
                    {
                        IdMatiere = aff.IdMatiere,
                        IdProfesseur = aff.IdProfesseur,
                        IdClasse = aff.IdClasse,
                        IdSemestre = aff.IdSemestre,
                        IdAffectation = aff.IdAffectation,
                        VolumeHours = aff.VolumeHoraireTotal,
                        Statut = "Automatique"
                    };
                    _db.Cours.Add(cours);
                    await _db.SaveChangesAsync();
                }

                if (cours.Creneaux == null || !cours.Creneaux.Any())
                {
                    // Besoin de planifier ce cours (exemple 2h par semaine, ou découper)
                    // Simplification: on tente de trouver 1 créneau de 2h
                    TimeSpan dureeCible = TimeSpan.FromHours(2);
                    
                    var newCreneau = FindBestCreneau(cours, dureeCible, toutesSalles, allCreneaux, aff.Professeur);
                    
                    if (newCreneau != null)
                    {
                        // Save creneau
                        var lastId = await _db.Creneaux.MaxAsync(c => (int?)c.IdCreneau) ?? 0;
                        newCreneau.IdCreneau = lastId + 1;
                        
                        _db.Creneaux.Add(newCreneau);
                        
                        // Update in-memory context for subsequent affectations
                        allCreneaux.Add(newCreneau);
                        
                        cours.IdSalle = newCreneau.Cours.IdSalle;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        // Conflit irrésoluble ou pas de place
                        return false; 
                    }
                }
            }

            return true;
        }

        public async Task<List<Cours>> ResoudreConflitsAsync(List<Cours> coursNonPlanifies)
        {
            var coursResolus = new List<Cours>();
            var allCreneaux = await _db.Creneaux.Include(c => c.Cours).ToListAsync();
            var toutesSalles = await _db.Salles.ToListAsync();

            foreach (var cours in coursNonPlanifies.OrderByDescending(c => c.VolumeHours))
            {
                var prof = await _db.Professeurs.Include(p => p.Disponibilites).FirstOrDefaultAsync(p => p.IdProfesseur == cours.IdProfesseur);
                var newCreneau = FindBestCreneau(cours, TimeSpan.FromHours(2), toutesSalles, allCreneaux, prof);

                if (newCreneau != null)
                {
                    var lastId = await _db.Creneaux.MaxAsync(c => (int?)c.IdCreneau) ?? 0;
                    newCreneau.IdCreneau = lastId + 1;
                    
                    _db.Creneaux.Add(newCreneau);
                    allCreneaux.Add(newCreneau);
                    coursResolus.Add(cours);
                }
            }
            
            await _db.SaveChangesAsync();
            return coursResolus;
        }

        public async Task<bool> OptimiserRepartitionAsync()
        {
            // L'optimisation consiste à redistribuer les salles pour maximiser la capacité
            var creneaux = await _db.Creneaux.Include(c => c.Cours).ThenInclude(c => c.Classe).ToListAsync();
            var salles = await _db.Salles.OrderBy(s => s.Capacite).ToListAsync();

            bool modifie = false;
            foreach (var creneau in creneaux)
            {
                // Trouver la salle la plus adaptée (plus petite salle pouvant accueillir la classe)
                int tailleClasse = 40; // Valeur par défaut, en réalité c.Classe.Effectif si existant
                
                var salleOpt = salles.FirstOrDefault(s => s.Capacite >= tailleClasse && IsSalleDisponible(s.IdSalle, creneau.JourSemaine, creneau.HeureDebut, creneau.HeureFin, creneaux, creneau.IdCreneau));

                if (salleOpt != null && creneau.Cours.IdSalle != salleOpt.IdSalle)
                {
                    creneau.Cours.IdSalle = salleOpt.IdSalle;
                    modifie = true;
                }
            }

            if (modifie) await _db.SaveChangesAsync();
            return true;
        }

        // --- Algorithme de recherche (Backtracking & Contraintes) ---
        private Creneau FindBestCreneau(Cours cours, TimeSpan duree, List<Salle> salles, List<Creneau> existants, Professeur prof)
        {
            foreach (var jour in _jours)
            {
                for (var heure = _heureMin; heure + duree <= _heureMax; heure += TimeSpan.FromHours(1))
                {
                    var heureFin = heure + duree;

                    // 1. Respect des disponibilités du prof (s'il a précisé des dispos)
                    if (prof != null && prof.Disponibilites != null && prof.Disponibilites.Any())
                    {
                        bool isDispo = prof.Disponibilites.Any(d => d.JourSemaine == jour && d.HeureDebut <= heure && d.HeureFin >= heureFin);
                        if (!isDispo) continue;
                    }

                    // 2. Pas de conflit pour le prof
                    if (cours.IdProfesseur.HasValue && !IsProfDisponible(cours.IdProfesseur.Value, jour, heure, heureFin, existants))
                        continue;

                    // 3. Pas de conflit pour la classe
                    if (cours.IdClasse.HasValue && !IsClasseDisponible(cours.IdClasse.Value, jour, heure, heureFin, existants))
                        continue;

                    // 4. Trouver une salle dispo
                    var salleLibre = salles.FirstOrDefault(s => IsSalleDisponible(s.IdSalle, jour, heure, heureFin, existants, null));

                    if (salleLibre != null)
                    {
                        cours.IdSalle = salleLibre.IdSalle;
                        return new Creneau
                        {
                            IdCours = cours.IdCours,
                            Cours = cours,
                            JourSemaine = jour,
                            HeureDebut = heure,
                            HeureFin = heureFin,
                            SemaineType = "A" // Par défaut
                        };
                    }
                }
            }
            return null; // Échec
        }

        private bool IsProfDisponible(int idProf, string jour, TimeSpan debut, TimeSpan fin, List<Creneau> existants)
        {
            return !existants.Any(c => c.Cours.IdProfesseur == idProf && c.JourSemaine == jour && 
                                      HasTimeOverlap(c.HeureDebut, c.HeureFin, debut, fin));
        }

        private bool IsClasseDisponible(int idClasse, string jour, TimeSpan debut, TimeSpan fin, List<Creneau> existants)
        {
            return !existants.Any(c => c.Cours.IdClasse == idClasse && c.JourSemaine == jour && 
                                      HasTimeOverlap(c.HeureDebut, c.HeureFin, debut, fin));
        }

        private bool IsSalleDisponible(int idSalle, string jour, TimeSpan debut, TimeSpan fin, List<Creneau> existants, int? ignoreCreneauId)
        {
            return !existants.Any(c => c.Cours.IdSalle == idSalle && c.JourSemaine == jour && 
                                      (!ignoreCreneauId.HasValue || c.IdCreneau != ignoreCreneauId.Value) &&
                                      HasTimeOverlap(c.HeureDebut, c.HeureFin, debut, fin));
        }

        private bool HasTimeOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }
    }
}
