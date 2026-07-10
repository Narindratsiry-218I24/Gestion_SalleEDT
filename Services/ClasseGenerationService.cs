using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IClasseGenerationService
    {
        Task<int> GenererClassesPourAnneeAsync(int anneeId);
        Task<int> GenererClassesPourMentionAsync(int anneeId, int mentionId);
        Task<List<string>> GetNomsClassesGenerees(int anneeId);

        /// <summary>
        /// Retourne les codes de niveaux autorisés pour une filière donnée.
        /// </summary>
        List<string> GetNiveauxAutorisesPourFiliere(string codeFiliere, int idMention);

        /// <summary>
        /// Supprime les classes invalides (mauvaises combinaisons Filière/Niveau) pour une année.
        /// </summary>
        Task<int> SupprimerClassesInvalidesAsync(int anneeId);
    }

    public class ClasseGenerationService : IClasseGenerationService
    {
        private readonly EMITDbContext _db;
        private readonly ILogger<ClasseGenerationService> _logger;

        // -------------------------------------------------------------------------
        // RÈGLES MÉTIER : codes de niveaux autorisés par filière
        // -------------------------------------------------------------------------
        // Mention INFORMATIQUE (id_mention = 1)
        //   Licence : DA2I  → L1, L2, L3
        //   Master  : M2I, SDIA, SIGD → M1, M2
        //
        // Mention MANAGEMENT (id_mention = 2)
        //   Licence : AES → L1, L2, L3
        //   Master  : AEE, ASE, RPO, RPCO, PCO → M1, M2
        //
        // Mention MULTI-MEDIA (id_mention = 3)
        //   ICM → L1, L2, L3, M1, M2 (tous les niveaux)
        // -------------------------------------------------------------------------

        private static readonly HashSet<string> FilieresLicence = new HashSet<string>
            { "DA2I", "AES", "ICM" };

        private static readonly HashSet<string> FilieresAllNiveaux = new HashSet<string>
            { "ICM" };

        public ClasseGenerationService(EMITDbContext db, ILogger<ClasseGenerationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =========================================================================
        // MÉTHODE PUBLIQUE : GetNiveauxAutorisesPourFiliere
        // =========================================================================
        /// <summary>
        /// Retourne la liste des <c>code_niveau</c> autorisés pour une filière.
        /// </summary>
        public List<string> GetNiveauxAutorisesPourFiliere(string codeFiliere, int idMention)
        {
            // ICM (Multi-Media) → tous les niveaux
            if (FilieresAllNiveaux.Contains(codeFiliere))
                return new List<string> { "L1", "L2", "L3", "M1", "M2" };

            // Filières de Licence → L1, L2, L3
            if (FilieresLicence.Contains(codeFiliere))
                return new List<string> { "L1", "L2", "L3" };

            // Toutes les autres filières sont de Master → M1, M2
            return new List<string> { "M1", "M2" };
        }

        // =========================================================================
        // MÉTHODE PUBLIQUE : GenererClassesPourAnneeAsync
        // =========================================================================
        public async Task<int> GenererClassesPourAnneeAsync(int anneeId)
        {
            var classesCrees = 0;

            // 1. Vérifier que l'année existe
            var annee = await _db.AnneesAcademiques
                .FirstOrDefaultAsync(a => a.IdAnnee == anneeId);

            if (annee == null)
            {
                _logger.LogWarning("Génération annulée : année académique {anneeId} introuvable.", anneeId);
                return 0;
            }

            // 2. Charger toutes les filières avec leur mention
            var filieres = await _db.Filieres
                .Include(f => f.Mention)
                .ToListAsync();

            // 3. Charger tous les niveaux et les indexer par code ET par mention
            //    Structure : { (idMention, codeNiveau) → Niveau }
            var niveaux = await _db.Niveaux.ToListAsync();
            var niveauIndex = niveaux
                .GroupBy(n => n.IdMention)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(n => n.CodeNiveau, n => n)
                );

            // 4. Pour chaque filière, déterminer les niveaux autorisés et créer les classes
            foreach (var filiere in filieres)
            {
                var codesAutorises = GetNiveauxAutorisesPourFiliere(
                    filiere.CodeFiliere, filiere.IdMention);

                // Récupérer les objets Niveau correspondants dans la mention de la filière
                if (!niveauIndex.TryGetValue(filiere.IdMention, out var niveauxDeMention))
                {
                    _logger.LogWarning(
                        "Aucun niveau trouvé pour la mention {idMention} (filière {code}).",
                        filiere.IdMention, filiere.CodeFiliere);
                    continue;
                }

                foreach (var codeNiveau in codesAutorises)
                {
                    if (!niveauxDeMention.TryGetValue(codeNiveau, out var niveau))
                    {
                        _logger.LogWarning(
                            "Niveau '{codeNiveau}' introuvable pour la mention {idMention}.",
                            codeNiveau, filiere.IdMention);
                        continue;
                    }

                    // Vérifier si la classe existe déjà
                    var existe = await _db.Classes.AnyAsync(c =>
                        c.IdAnneeAcademique == anneeId &&
                        c.IdFiliere == filiere.IdFiliere &&
                        c.IdNiveau == niveau.IdNiveau);

                    if (existe) continue;

                    // Créer la classe
                    var classe = new Classe
                    {
                        IdFiliere         = filiere.IdFiliere,
                        IdNiveau          = niveau.IdNiveau,
                        IdAnneeAcademique = anneeId,
                        NomClasse         = $"{codeNiveau} {filiere.CodeFiliere}",   // ex: "L1 DA2I"
                        CodeClasse        = $"{codeNiveau}-{filiere.CodeFiliere}",   // ex: "L1-DA2I"
                        EstArchivee       = false
                    };

                    _db.Classes.Add(classe);
                    classesCrees++;

                    _logger.LogDebug(
                        "Classe créée : {code} (filière={idF}, niveau={idN})",
                        classe.CodeClasse, filiere.IdFiliere, niveau.IdNiveau);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "{count} classe(s) générée(s) pour l'année '{libelle}'.",
                classesCrees, annee.Libelle);

            return classesCrees;
        }

        // =========================================================================
        // MÉTHODE PUBLIQUE : GenererClassesPourMentionAsync
        // =========================================================================
        public async Task<int> GenererClassesPourMentionAsync(int anneeId, int mentionId)
        {
            var classesCrees = 0;

            var annee = await _db.AnneesAcademiques
                .FirstOrDefaultAsync(a => a.IdAnnee == anneeId);
            if (annee == null) return 0;

            var filieres = await _db.Filieres
                .Where(f => f.IdMention == mentionId)
                .ToListAsync();

            var niveaux = await _db.Niveaux
                .Where(n => n.IdMention == mentionId)
                .ToDictionaryAsync(n => n.CodeNiveau, n => n);

            foreach (var filiere in filieres)
            {
                var codesAutorises = GetNiveauxAutorisesPourFiliere(
                    filiere.CodeFiliere, filiere.IdMention);

                foreach (var codeNiveau in codesAutorises)
                {
                    if (!niveaux.TryGetValue(codeNiveau, out var niveau)) continue;

                    var existe = await _db.Classes.AnyAsync(c =>
                        c.IdAnneeAcademique == anneeId &&
                        c.IdFiliere == filiere.IdFiliere &&
                        c.IdNiveau == niveau.IdNiveau);

                    if (existe) continue;

                    _db.Classes.Add(new Classe
                    {
                        IdFiliere         = filiere.IdFiliere,
                        IdNiveau          = niveau.IdNiveau,
                        IdAnneeAcademique = anneeId,
                        NomClasse         = $"{codeNiveau} {filiere.CodeFiliere}",
                        CodeClasse        = $"{codeNiveau}-{filiere.CodeFiliere}",
                        EstArchivee       = false
                    });
                    classesCrees++;
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "{count} classe(s) générée(s) pour la mention {mentionId}, année {anneeId}.",
                classesCrees, mentionId, anneeId);

            return classesCrees;
        }

        // =========================================================================
        // MÉTHODE PUBLIQUE : SupprimerClassesInvalidesAsync
        // =========================================================================
        /// <summary>
        /// Détecte et supprime les classes dont la combinaison Filière/Niveau
        /// ne respecte pas les règles métier (ex: DA2I-M1 est invalide).
        /// </summary>
        public async Task<int> SupprimerClassesInvalidesAsync(int anneeId)
        {
            var suppressions = 0;

            // Charger les classes avec leurs filières et niveaux associés
            var classes = await _db.Classes
                .Include(c => c.Filiere)
                .Include(c => c.Niveau)
                .Where(c => c.IdAnneeAcademique == anneeId)
                .ToListAsync();

            foreach (var classe in classes)
            {
                if (classe.Filiere == null || classe.Niveau == null) continue;

                var codesAutorises = GetNiveauxAutorisesPourFiliere(
                    classe.Filiere.CodeFiliere, classe.Filiere.IdMention);

                if (!codesAutorises.Contains(classe.Niveau.CodeNiveau))
                {
                    _logger.LogWarning(
                        "Suppression classe invalide : {code} (filière={f}, niveau={n})",
                        classe.CodeClasse, classe.Filiere.CodeFiliere, classe.Niveau.CodeNiveau);

                    _db.Classes.Remove(classe);
                    suppressions++;
                }
            }

            if (suppressions > 0)
                await _db.SaveChangesAsync();

            _logger.LogInformation(
                "{count} classe(s) invalide(s) supprimée(s) pour l'année {anneeId}.",
                suppressions, anneeId);

            return suppressions;
        }

        // =========================================================================
        // MÉTHODE PUBLIQUE : GetNomsClassesGenerees
        // =========================================================================
        public async Task<List<string>> GetNomsClassesGenerees(int anneeId)
        {
            return await _db.Classes
                .Where(c => c.IdAnneeAcademique == anneeId)
                .OrderBy(c => c.CodeClasse)
                .Select(c => c.CodeClasse)
                .ToListAsync();
        }
    }
}