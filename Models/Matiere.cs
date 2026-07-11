using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("matiere")]
public class Matiere
{
    [Key]
    [Column("id_matiere")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdMatiere { get; set; }

    [Column("id_filiere")]
    public int IdFiliere { get; set; }

    [StringLength(20)]
    [Column("code_matiere")]
    public string CodeMatiere { get; set; }

    [StringLength(100)]
    [Column("nom_matiere")]
    public string NomMatiere { get; set; }

    [Column("id_ref_semestre")]
    public int IdRefSemestre { get; set; }

    [Column("credit")]
    public int Credit { get; set; }

    [Column("volume_horaire")]
    public int VolumeHoraire { get; set; }

    [Column("id_professeur_responsable")]
    public int? IdProfesseurResponsable { get; set; }

    // Navigation properties
    [ForeignKey("IdFiliere")]
    public virtual Filiere? Filiere { get; set; }

    [ForeignKey("IdRefSemestre")]
    public virtual RefSemestre? RefSemestre { get; set; }

    [ForeignKey("IdProfesseurResponsable")]
    public virtual Professeur? ProfesseurResponsable { get; set; }

    [InverseProperty("Matiere")]
    public virtual ICollection<Prerequisite>? Prerequisites { get; set; }

    [InverseProperty("PrerequisiteMatiere")]
    public virtual ICollection<Prerequisite>? IsPrerequisiteFor { get; set; }

    public virtual ICollection<Cours>? Cours { get; set; } = new List<Cours>();
    public virtual ICollection<AffectationMatiere>? AffectationsMatieres { get; set; }
}
}