using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("reglage_disponibilite")]
    public class ReglageDisponibilite
    {
        [Key]
        [Column("id_reglage")]
        public int IdReglage { get; set; }

        [Column("id_annee")]
        public int IdAnnee { get; set; }

        [Column("dimanche_autorise")]
        public bool DimancheAutorise { get; set; } = false;

        [Column("samedi_autorise")]
        public bool SamediAutorise { get; set; } = true;

        [Column("heure_ouverture")]
        public TimeSpan HeureOuverture { get; set; } = new TimeSpan(7, 0, 0);

        [Column("heure_fermeture")]
        public TimeSpan HeureFermeture { get; set; } = new TimeSpan(18, 0, 0);

        [Column("duree_min_creneau_minutes")]
        public int DureeMinCreneauMinutes { get; set; } = 60;

        [ForeignKey("IdAnnee")]
        public virtual AnneeAcademique? AnneeAcademique { get; set; }
    }
}
