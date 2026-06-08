using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("proposition_admin")]
public class PropositionAdmin
{
    [Key]
    [Column("id_proposition")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdProposition { get; set; }

    [Column("id_demande")]
    public int IdDemande { get; set; }

    [Column("id_salle_proposee")]
    public int IdSalleProposee { get; set; }

    [Column("date_proposee")]
    public DateTime DateProposee { get; set; }

    [Column("heure_debut_proposee")]
    public TimeSpan HeureDebutProposee { get; set; }

    [Column("heure_fin_proposee")]
    public TimeSpan HeureFinProposee { get; set; }

    [Column("est_acceptee")]
    public bool? EstAcceptee { get; set; }

    [ForeignKey("IdDemande")]
    public virtual DemandeEdt Demande { get; set; }

    [ForeignKey("IdSalleProposee")]
    public virtual Salle Salle { get; set; }
}
}
