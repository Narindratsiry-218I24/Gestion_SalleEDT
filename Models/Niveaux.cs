using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("niveau")]
public class Niveau
{
    [Key]
    [Column("id_niveau")]
    public int IdNiveau { get; set; }

    [Required]
    [StringLength(10)]
    [Column("code_niveau")]
    public string CodeNiveau { get; set; }

    [Required]
    [Column("ordre")]
    public int Ordre { get; set; }

    [Column("id_mention")]
    public int IdMention { get; set; }

    // Navigation properties
    [ForeignKey("IdMention")]
    public virtual Mention Mention { get; set; }

    public virtual ICollection<RefSemestre> RefSemestres { get; set; }
}
}
