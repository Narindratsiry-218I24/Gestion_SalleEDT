using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("prerequisites")]
    public class Prerequisite
    {
        [Key]
        [Column("id_prerequisite")]
        public int Id { get; set; }

        // The Matiere that has the prerequisite
        [Column("id_matiere")]
        public int MatiereId { get; set; }

        // The Matiere that is required (the prerequisite)
        [Column("id_prerequisite_matiere")]
        public int PrerequisiteMatiereId { get; set; }

        [ForeignKey("MatiereId")]
        public virtual Matiere? Matiere { get; set; }

        [ForeignKey("PrerequisiteMatiereId")]
        public virtual Matiere? PrerequisiteMatiere { get; set; }
    }
}
