using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    /// <summary>
    /// Type de disponibilité professeur.
    /// Décision 1.3 : JourSemaine portait deux sémantiques incompatibles (code jour vs date ISO).
    /// Séparation explicite via ce type.
    /// </summary>
    public enum TypeDisponibilite
    {
        /// <summary>Disponibilité récurrente chaque semaine sur un jour donné (MON, TUE...).</summary>
        Recurrente = 0,

        /// <summary>Disponibilité ponctuelle sur une date exacte.</summary>
        Ponctuelle = 1
    }

    /// <summary>
    /// Disponibilité d'un professeur.
    /// - TypeDisponibilite = Recurrente → JourSemaineCode est renseigné (MON/TUE/WED/THU/FRI/SAT)
    /// - TypeDisponibilite = Ponctuelle → DateSpecifique est renseignée
    /// - IdAnnee lie les disponibilités à une année académique précise.
    /// Colonne legacy "jour_semaine" renommée "jour_semaine_legacy" après migration des données.
    /// </summary>
    [Table("disponibilite_prof")]
    public class DisponibiliteProf
    {
        [Key]
        [Column("id_dispo")]
        public int IdDispo { get; set; }

        [Column("id_professeur")]
        public int IdProfesseur { get; set; }

        // ── Nouvelle structure ──────────────────────────────────────────────
        /// <summary>Recurrente ou Ponctuelle.</summary>
        [Column("type_disponibilite")]
        public TypeDisponibilite TypeDisponibilite { get; set; } = TypeDisponibilite.Recurrente;

        /// <summary>Code du jour de la semaine (MON/TUE/WED/THU/FRI/SAT). Null si Ponctuelle.</summary>
        [StringLength(5)]
        [Column("jour_semaine_code")]
        public string? JourSemaineCode { get; set; }

        /// <summary>Date exacte. Null si Recurrente.</summary>
        [Column("date_specifique")]
        public DateTime? DateSpecifique { get; set; }

        /// <summary>
        /// Année académique à laquelle cette disponibilité est rattachée.
        /// Null = disponibilité héritée (avant migration) non liée à une année précise.
        /// </summary>
        [Column("id_annee")]
        public int? IdAnnee { get; set; }

        // ── Colonne legacy (conservée pour compatibilité, ne plus écrire dedans) ──
        /// <summary>
        /// LEGACY — ancienne colonne ambiguë. Ne plus écrire dedans, lire uniquement pour
        /// migration. Sera supprimée dans une prochaine migration après vérification des données.
        /// </summary>
        [StringLength(15)]
        [Column("jour_semaine")]
        public string? JourSemaine { get; set; }

        // ── Horaires ────────────────────────────────────────────────────────
        [Required]
        [Column("heure_debut")]
        public TimeSpan HeureDebut { get; set; }

        [Required]
        [Column("heure_fin")]
        public TimeSpan HeureFin { get; set; }

        [Column("semaine_type", TypeName = "bpchar")]
        [StringLength(1)]
        public string? SemaineType { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey("IdProfesseur")]
        public virtual Professeur Professeur { get; set; } = null!;

        [ForeignKey("IdAnnee")]
        public virtual AnneeAcademique? AnneeAcademique { get; set; }
    }
}
