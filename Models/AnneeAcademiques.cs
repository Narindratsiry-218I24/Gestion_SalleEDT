using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("annee_academique")]
public class AnneeAcademique
{
    [Key]
    [Column("id_annee")]
    public int IdAnnee { get; set; }

    [Required]
    [StringLength(20)]
    [Column("libelle")]
    public string Libelle { get; set; }

    [Required]
    [Column("date_debut_annee")]
    public DateTime DateDebutAnnee { get; set; }

    [Required]
    [Column("date_fin_annee")]
    public DateTime DateFinAnnee { get; set; }

    // Navigation properties
    public virtual ICollection<Semestre> Semestres { get; set; }
}
}
