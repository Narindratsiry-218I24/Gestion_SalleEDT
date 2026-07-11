using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("cours")]
    public class Cours
    {
        [Key]
        [Column("id_cours")]
        public int IdCours { get; set; }

        [Column("id_matiere")]
        public int IdMatiere { get; set; }

        [Column("id_professeur")]
        public int? IdProfesseur { get; set; }

        [Column("id_classe")]
        public int? IdClasse { get; set; }

        // Removing IdSalle as it shouldn't be at Cours level, it is at Seance level.
        // public int? IdSalle { get; set; }

        [Column("id_semestre")]
        public int? IdSemestre { get; set; }

        [Column("id_groupe")]
        public int? IdGroupe { get; set; }

        [Column("volume_hours")]
        public int VolumeHours { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }

        [StringLength(50)]
        [Column("type_cours")]
        public string? TypeCours { get; set; } = CourseType.CM.ToString();

        [StringLength(50)]
        [Column("teaching_mode")]
        public string? Mode { get; set; } = TeachingMode.Presentiel.ToString();

        [StringLength(50)]
        [Column("language")]
        public string? LanguageStr { get; set; } = Language.Francais.ToString();

        [StringLength(50)]
        [Column("statut")]
        public string? Statut { get; set; } = CourseStatus.Cree.ToString();

        // Pedagogical Information
        [Column("objectives")]
        public string? Objectives { get; set; }

        [Column("skills")]
        public string? Skills { get; set; }

        [Column("methods")]
        public string? Methods { get; set; }

        [Column("evaluation")]
        public string? Evaluation { get; set; }

        [Column("bibliography")]
        public string? Bibliography { get; set; }

        // Navigation properties
        [ForeignKey("IdMatiere")]
        public virtual Matiere? Matiere { get; set; }

        [ForeignKey("IdProfesseur")]
        public virtual Professeur? Professeur { get; set; }

        [ForeignKey("IdClasse")]
        public virtual Classe? Classe { get; set; }

        [ForeignKey("IdSemestre")]
        public virtual Semestre? Semestre { get; set; }

        [ForeignKey("IdGroupe")]
        public virtual Groupe? Groupe { get; set; }

        public virtual ICollection<Seance>? Seances { get; set; } = new List<Seance>();

        // Legacy properties retained for backward compatibility with older controllers
        [Column("id_salle")]
        public int? IdSalle { get; set; }

        [ForeignKey("IdSalle")]
        public virtual Salle? Salle { get; set; }

        public virtual ICollection<Creneau>? Creneaux { get; set; } = new List<Creneau>();

        // Navigation for Prerequisites (Matiere level, but could be related to Cours, let's keep it simple for now)
        // Usually prerequisites are between subjects (Matieres). The prompt said "Java II nécessite Algorithmique".
        // Let's add it to Subject/Matiere.

        [NotMapped]
        public int RealizedHours => Seances?.Where(s => s.Statut == "Realisee").Sum(s => s.DurationHours) ?? 0;

        [NotMapped]
        public int RemainingHours => Math.Max(0, VolumeHours - RealizedHours);

        [NotMapped]
        public double Progression => VolumeHours > 0 ? (double)RealizedHours / VolumeHours * 100 : 0;

        [NotMapped]
        public Seance? NextSeance => Seances?.Where(s => s.Date >= DateTime.Today).OrderBy(s => s.Date).ThenBy(s => s.StartTime).FirstOrDefault();
    }
}
