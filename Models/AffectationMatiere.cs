using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("affectation_matiere")]
    public class AffectationMatiere
    {
        [Key]
        [Column("id_affectation")]
        public int IdAffectation { get; set; }

        [Column("id_classe")]
        public int IdClasse { get; set; }

        [Column("id_matiere")]
        public int IdMatiere { get; set; }

        [Column("id_professeur")]
        public int IdProfesseur { get; set; }

        [Column("id_semestre")]
        public int IdSemestre { get; set; }

        [Column("volume_horaire_total")]
        public int VolumeHoraireTotal { get; set; }

        [Column("heures_cm")]
        public int HeuresCm { get; set; }

        [Column("heures_td")]
        public int HeuresTd { get; set; }

        [Column("heures_tp")]
        public int HeuresTp { get; set; }

        [Column("est_actif")]
        public bool EstActif { get; set; } = true;

        [Required]
        [Column("date_debut")]
        public DateTime DateDebut { get; set; }

        [Required]
        [Column("date_fin")]
        public DateTime DateFin { get; set; }

        [Column("commentaire", TypeName = "text")]
        public string? Commentaire { get; set; }

        [ForeignKey("IdClasse")]
        public virtual Classe Classe { get; set; }

        [ForeignKey("IdMatiere")]
        public virtual Matiere Matiere { get; set; }

        [ForeignKey("IdProfesseur")]
        public virtual Professeur Professeur { get; set; }

        [ForeignKey("IdSemestre")]
        public virtual Semestre Semestre { get; set; }

        public virtual ICollection<Cours> Cours { get; set; }
    }
}
