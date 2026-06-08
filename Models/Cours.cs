using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("cours")]
public class Cours
{
    [Key]
    [Column("id_cours")]
    public int IdCours { get; set; }

    [Column("id_matiere")]
    public int IdMatiere { get; set; }

    [Column("id_professeur")]
    public int IdProfesseur { get; set; }

    [Column("id_classe")]
    public int IdClasse { get; set; }

    [Column("id_salle")]
    public int IdSalle { get; set; }

    [Column("id_semestre")]
    public int IdSemestre { get; set; }

    [Required]
    [StringLength(10)]
    [Column("type_cours")]
    public string TypeCours { get; set; }

    [StringLength(20)]
    [Column("statut")]
    public string Statut { get; set; }

    // Navigation properties
    [ForeignKey("IdMatiere")]
    public virtual Matiere Matiere { get; set; }

    [ForeignKey("IdProfesseur")]
    public virtual Professeur Professeur { get; set; }

    [ForeignKey("IdClasse")]
    public virtual Classe Classe { get; set; }

    [ForeignKey("IdSalle")]
    public virtual Salle Salle { get; set; }

    [ForeignKey("IdSemestre")]
    public virtual Semestre Semestre { get; set; }

    public virtual ICollection<Creneau> Creneaux { get; set; }
    public virtual ICollection<DemandeEdt> DemandesEdt { get; set; }
}
}
