using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("professeur")]
    [Index(nameof(Email), IsUnique = true)]
    public class Professeur
    {
        [Key]
        [Column("id_professeur")]
        public int IdProfesseur { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nom")]
        public string Nom { get; set; }

        [Required]
        [StringLength(50)]
        [Column("prenom")]
        public string Prenom { get; set; }

        [Required]
        [StringLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Column("telephone")]
        public string Telephone { get; set; }

        [StringLength(20)]
        [Column("grade")]
        public string Grade {get ; set ;}

        [StringLength(50)]
        [Column("matricule")]
        public string Matricule { get; set; }

        [StringLength(10)]
        [Column("titre")]
        public string Titre { get; set; }

        [StringLength(20)]
        [Column("telephone_portable")]
        public string TelephonePortable { get; set; }

        [StringLength(100)]
        [Column("specialite")]
        public string Specialite { get; set; }

        [Column("specialites_secondaires", TypeName = "jsonb")]
        public string SpecialitesSecondaires { get; set; }

        [StringLength(100)]
        [Column("departement")]
        public string Departement { get; set; }

        [Column("date_embauche")]
        public DateTime? DateEmbauche { get; set; }

        [StringLength(50)]
        [Column("statut")]
        public string Statut { get; set; }

        [StringLength(255)]
        [Column("photo_url")]
        public string PhotoUrl { get; set; }

        [Column("biographie", TypeName = "text")]
        public string Biographie { get; set; }

        [StringLength(255)]
        [Column("cv_url")]
        public string CvUrl { get; set; }

        [Column("id_utilisateur")]
        public int? IdUtilisateur { get; set; }

        [ForeignKey("IdUtilisateur")]
        public virtual Utilisateur Utilisateur { get; set; }

        [Column("capacite_horaire_max")]
        public int CapaciteHoraireMax { get; set; } = 20;

        [Column("est_actif")]
        public bool EstActif { get; set; } = true;

        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [Column("date_modification")]
        public DateTime? DateModification { get; set; }

        [Column("heures_effectuees")]
        public int HeuresEffectuees { get; set; }

        [NotMapped]
        public IFormFile? photoFile { get; set; }

        [NotMapped]
        public IFormFile? cvFile { get; set; }

        // Navigation properties
        public virtual ICollection<DisponibiliteProf> Disponibilites { get; set; }
        public virtual ICollection<Cours> Cours { get; set; }
        public virtual ICollection<Matiere> MatieresResponsable { get; set; }
        public virtual ICollection<AffectationMatiere> AffectationsMatieres { get; set; }
    }
}
