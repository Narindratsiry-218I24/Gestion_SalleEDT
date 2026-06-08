using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("salle")]
public class Salle
{
    [Key]
    [Column("id_salle")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdSalle { get; set; }

    [StringLength(1)]
    [Column("code_batiment", TypeName = "bpchar")]
    public string CodeBatiment { get; set; }

    [Column("etage")]
    public int Etage { get; set; }

    [StringLength(10)]
    [Column("numero_porte")]
    public string NumeroPorte { get; set; }

    [StringLength(100)]
    [Column("nom_salle")]
    public string NomSalle { get; set; }

    [Column("capacite")]
    public int Capacite { get; set; }

    [StringLength(20)]
    [Column("type_salle")]
    public string TypeSalle { get; set; }

    // Navigation properties
    public virtual ICollection<Cours> Cours { get; set; }
    public virtual ICollection<DemandeEdt> DemandesEdt { get; set; }
}
}