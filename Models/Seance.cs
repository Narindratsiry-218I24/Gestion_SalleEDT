using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("seances")]
    public class Seance
    {
        [Key]
        [Column("id_seance")]
        public int Id { get; set; }

        [Column("id_cours")]
        public int CourseId { get; set; }

        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("id_salle")]
        public int? SalleId { get; set; }

        [Column("id_groupe")]
        public int? GroupeId { get; set; }

        [Column("realized_hours")]
        public int RealizedHours { get; set; }

        [Column("attendance")]
        public double? Attendance { get; set; }

        [ForeignKey("CourseId")]
        public virtual Cours Cours { get; set; }

        [ForeignKey("SalleId")]
        public virtual Salle Salle { get; set; }

        [ForeignKey("GroupeId")]
        public virtual Groupe Groupe { get; set; }
    }
}