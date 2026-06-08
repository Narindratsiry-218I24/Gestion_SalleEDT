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

    [Column("id_semestre")]
    public int IdSemestre { get; set; }

    [Required]
    [StringLength(100)]
    [Column("nom_classe")]
    public string NomClasse { get; set; }

    // Navigation properties
    [ForeignKey("IdFiliere")]
    public virtual Filiere Filiere { get; set; }

    [ForeignKey("IdSemestre")]
    public virtual Semestre Semestre { get; set; }

    public virtual ICollection<Cours> Cours { get; set; }
}
}
