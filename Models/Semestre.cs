using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("semestre")]
public class Semestre
{
    [Key]
    [Column("id_semestre")]
    public int IdSemestre { get; set; }

    [Column("id_ref_semestre")]
    public int IdRefSemestre { get; set; }

    [Column("id_annee")]
    public int IdAnnee { get; set; }

    [Required]
    [Column("date_debut")]
    public DateTime DateDebut { get; set; }

    [Required]
    [Column("date_fin")]
    public DateTime DateFin { get; set; }

    // Navigation properties
    [ForeignKey("IdRefSemestre")]
    public virtual RefSemestre RefSemestre { get; set; }

    [ForeignKey("IdAnnee")]
    public virtual AnneeAcademique AnneeAcademique { get; set; }

    public virtual ICollection<AffectationMatiere> AffectationsMatieres { get; set; }
    public virtual ICollection<Cours> Cours { get; set; }
}
}
