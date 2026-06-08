using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("ref_semestre")]
public class RefSemestre
{
    [Key]
    [Column("id_ref_semestre")]
    public int IdRefSemestre { get; set; }

    [Required]
    [StringLength(5)]
    [Column("code_semestre")]
    public string CodeSemestre { get; set; }

    [Required]
    [Column("ordre")]
    public int Ordre { get; set; }

    [Column("id_niveau")]
    public int IdNiveau { get; set; }

    // Navigation properties
    [ForeignKey("IdNiveau")]
    public virtual Niveau Niveau { get; set; }

    public virtual ICollection<Semestre> Semestres { get; set; }
    public virtual ICollection<Matiere> Matieres { get; set; }
}
}
