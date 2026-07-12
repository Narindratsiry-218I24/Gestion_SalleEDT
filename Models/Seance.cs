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
        public int IdSeance { get; set; }

        [NotMapped]
        public int Id { get => IdSeance; set => IdSeance = value; }

        [Column("id_cours")]
        public int IdCours { get; set; }

        // Alias for PlanningService (not mapped to DB)
        [NotMapped]
        public int CourseId { get => IdCours; set => IdCours = value; }

        [Column("id_salle")]
        public int? IdSalle { get; set; }

        // Alias for PlanningService (not mapped to DB)
        [NotMapped]
        public int? SalleId { get => IdSalle; set => IdSalle = value; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        [Column("statut")]
        public string? Statut { get; set; } = "Planifiee"; // Planifiee, Realisee, Annulee

        [ForeignKey("IdCours")]
        public virtual Cours? Cours { get; set; }

        [ForeignKey("IdSalle")]
        public virtual Salle? Salle { get; set; }

        [NotMapped]
        // Computed duration in hours

        public int DurationHours => (int)(EndTime - StartTime).TotalHours;

        // Additional fields for planning service
        [Column("id_groupe")]
        public int? GroupeId { get; set; }

        [NotMapped]
        public int RealizedHours => Statut != "Annulee" ? DurationHours : 0;
    }
}