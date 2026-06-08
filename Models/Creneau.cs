using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("creneau")]
public class Creneau
{
    [Key]
    [Column("id_creneau")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdCreneau { get; set; }

    [Column("id_cours")]
    public int IdCours { get; set; }

    [StringLength(10)]
    [Column("jour_semaine")]
    public string JourSemaine { get; set; }

    [Column("heure_debut")]
    public TimeSpan HeureDebut { get; set; }

    [Column("heure_fin")]
    public TimeSpan HeureFin { get; set; }

    [StringLength(1)]
    [Column("semaine_type", TypeName = "bpchar")]
    public string SemaineType { get; set; }

    // Navigation properties
    [ForeignKey("IdCours")]
    public virtual Cours Cours { get; set; }
}
}