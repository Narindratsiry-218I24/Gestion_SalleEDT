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

        [Column("id_salle")]
        public int? IdSalle { get; set; }

    [Column("id_semestre")]
    public int IdSemestre { get; set; }

        [Column("id_affectation")]
        public int? IdAffectation { get; set; }

        [Column("volume_hours")]
        public int VolumeHours { get; set; }

        [Column("capacity")]
        public int Capacity { get; set; }

        [StringLength(50)]
        [Column("type_cours")]
        public string? TypeCours { get; set; } = "CM";

        [StringLength(50)]
        [Column("teaching_mode")]
        public string? Mode { get; set; } = "Presentiel";

        [StringLength(50)]
        [Column("statut")]
        public string? Statut { get; set; } = "Cree";

        // Navigation properties
        [ForeignKey("IdMatiere")]
        public virtual Matiere? Matiere { get; set; }

        [ForeignKey("IdProfesseur")]
        public virtual Professeur? Professeur { get; set; }

        [ForeignKey("IdClasse")]
        public virtual Classe? Classe { get; set; }

        [ForeignKey("IdSalle")]
        public virtual Salle? Salle { get; set; }

    [ForeignKey("IdSemestre")]
    public virtual Semestre Semestre { get; set; }

        [ForeignKey("IdAffectation")]
        public virtual AffectationMatiere? AffectationMatiere { get; set; }

[Column("id_groupe")]
        public int? IdGroupe { get; set; }

        [ForeignKey("IdGroupe")]
        public virtual Groupe? Groupe { get; set; }

        [Column("objectives")]
        public string? Objectives { get; set; }
        [Column("skills")]
        public string? Skills { get; set; }
        [Column("evaluation")]
        public string? Evaluation { get; set; }

        public virtual ICollection<Creneau>? Creneaux { get; set; }
        public virtual ICollection<DemandeEdt>? DemandesEdt { get; set; }
        
        public virtual ICollection<Seance>? Seances { get; set; } = new List<Seance>();

        [NotMapped]
        public int RealizedHours => Seances?.Sum(s => s.RealizedHours) ?? 0;

        [NotMapped]
        public int RemainingHours => Math.Max(0, VolumeHours - RealizedHours);

        [NotMapped]
        public double Progression => VolumeHours > 0 ? (double)RealizedHours / VolumeHours * 100 : 0;

        [NotMapped]
        public Seance? NextSeance => Seances?.Where(s => s.Date >= DateTime.Today).OrderBy(s => s.Date).ThenBy(s => s.StartTime).FirstOrDefault();
    }
}
