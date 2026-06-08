using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

        // Navigation properties
        public virtual ICollection<DisponibiliteProf> Disponibilites { get; set; }
        public virtual ICollection<Cours> Cours { get; set; }
    }
}
