using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdNotification { get; set; }

        public int IdProfesseur { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "info"; // info, success, warning, danger

        [MaxLength(500)]
        public string? Lien { get; set; }

        public bool EstLue { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateLecture { get; set; }

        [ForeignKey("IdProfesseur")]
        public virtual Professeur? Professeur { get; set; }
    }
}
