using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("disponibilite_prof")]
public class DisponibiliteProf
{
    [Key]
    [Column("id_dispo")]
    public int IdDispo { get; set; }

    [Column("id_professeur")]
    public int IdProfesseur { get; set; }

    [Required]
    [StringLength(15)]
    [Column("jour_semaine")]
    public string JourSemaine { get; set; }

    [Required]
    [Column("heure_debut")]
    public TimeSpan HeureDebut { get; set; }

    [Required]
    [Column("heure_fin")]
    public TimeSpan HeureFin { get; set; }

    [Column("semaine_type", TypeName = "bpchar")]
    [StringLength(1)]
    public string SemaineType { get; set; }

    // Navigation properties
    [ForeignKey("IdProfesseur")]
    public virtual Professeur Professeur { get; set; }
}
}
