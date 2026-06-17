using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("subjects")]
    public class Subject
    {
        [Key]
        [Column("id_subject")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        [Column("label")]
        public string Label { get; set; }

        [Column("credits")]
        public int Credits { get; set; }

        [Column("hours")]
        public int Hours { get; set; }

        [StringLength(50)]
        [Column("type")]
        public string Type { get; set; }

        [Column("id_niveau")]
        public int? NiveauId { get; set; }

        [Column("id_mention")]
        public int? MentionId { get; set; }

        [Column("id_filiere")]
        public int? FiliereId { get; set; }

        [Column("id_semestre")]
        public int? SemestreId { get; set; }

        // Navigation properties (optional)
        [ForeignKey("NiveauId")]
        public virtual Niveau Niveau { get; set; }

        [ForeignKey("MentionId")]
        public virtual Mention Mention { get; set; }

        [ForeignKey("FiliereId")]
        public virtual Filiere Filiere { get; set; }

        [ForeignKey("SemestreId")]
        public virtual Semestre Semestre { get; set; }
    }
}
