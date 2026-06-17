using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("schedules")]
    public class Schedule
    {
        [Key]
        [Column("id_schedule")]
        public int Id { get; set; }

        [Column("id_subject")]
        public int? SubjectId { get; set; }

        [Column("id_professeur")]
        public int? ProfesseurId { get; set; }

        [Column("id_salle")]
        public int? SalleId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("heure_debut")]
        public TimeSpan HeureDebut { get; set; }

        [Column("heure_fin")]
        public TimeSpan HeureFin { get; set; }

        [StringLength(10)]
        [Column("session_type")]
        public string SessionType { get; set; }

        // Navigation
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        [ForeignKey("ProfesseurId")]
        public virtual Professeur Professeur { get; set; }

        [ForeignKey("SalleId")]
        public virtual Salle Salle { get; set; }
    }
}
