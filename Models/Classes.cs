using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("classe")]
public class Classe
{
    [Key]
    [Column("id_classe")]
    public int IdClasse { get; set; }

    [Column("id_filiere")]
    public int IdFiliere { get; set; }

    [Column("id_annee_academique")]
    public int IdAnneeAcademique { get; set; }

    [Required]
    [StringLength(100)]
    [Column("nom_classe")]
    public string NomClasse { get; set; }

    [StringLength(20)]
    [Column("code_classe")]
    public string? CodeClasse { get; set; }

    [Column("id_niveau")]
    public int? IdNiveau { get; set; }

    [Column("est_archivee")]
    public bool EstArchivee { get; set; }

    // Navigation properties
    [ForeignKey("IdFiliere")]
    public virtual Filiere Filiere { get; set; }

    [ForeignKey("IdAnneeAcademique")]
    public virtual AnneeAcademique AnneeAcademique { get; set; }

    [ForeignKey("IdNiveau")]
    public virtual Niveau? Niveau { get; set; }

    // Ajouté pour compatibilité avec Semestre
    [NotMapped]
    public virtual Semestre? Semestre { get; set; }

    public virtual ICollection<AffectationMatiere> AffectationsMatieres { get; set; }
    public virtual ICollection<Cours> Cours { get; set; }
}
}
