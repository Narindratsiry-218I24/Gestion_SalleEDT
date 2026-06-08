using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("demande_edt")]
    public class DemandeEdt
    {
        [Key]
        [Column("id_demande")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDemande { get; set; }

        [Column("id_demandeur")]
        public int IdDemandeur { get; set; }

        [Column("id_validateur")]
        public int? IdValidateur { get; set; }

        [Column("id_cours")]
        public int? IdCours { get; set; }

        [Column("id_salle")]
        public int? IdSalle { get; set; }

        // Champs optionnels pour les nouvelles demandes (examen, réservation, etc.)
        [Column("id_classe")]
        public int? IdClasse { get; set; }

        [Column("id_matiere")]
        public int? IdMatiere { get; set; }

        [Column("id_niveau")]
        public int? IdNiveau { get; set; }

        [Column("date_souhaitee")]
        public DateTime? DateSouhaitee { get; set; }

        [Column("heure_debut_souhaitee")]
        public TimeSpan? HeureDebutSouhaitee { get; set; }

        [Column("heure_fin_souhaitee")]
        public TimeSpan? HeureFinSouhaitee { get; set; }

        [Required]
        [StringLength(25)]
        [Column("type_demande")]
        public string TypeDemande { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Column("statut")]
        public string Statut { get; set; } = string.Empty;

        [Column("justification")]
        public string? Justification { get; set; }

        // Navigation properties
        [ForeignKey("IdDemandeur")]
        public virtual Utilisateur? Demandeur { get; set; }

        [ForeignKey("IdValidateur")]
        public virtual Utilisateur? Validateur { get; set; }

        [ForeignKey("IdCours")]
        public virtual Cours? Cours { get; set; }

        [ForeignKey("IdSalle")]
        public virtual Salle? Salle { get; set; }

        [ForeignKey("IdClasse")]
        public virtual Classe? Classe { get; set; }

        [ForeignKey("IdMatiere")]
        public virtual Matiere? Matiere { get; set; }

        [ForeignKey("IdNiveau")]
        public virtual Niveau? Niveau { get; set; }

        // Propositions de l'admin
        public virtual ICollection<PropositionAdmin> Propositions { get; set; } = new List<PropositionAdmin>();
    }
}
