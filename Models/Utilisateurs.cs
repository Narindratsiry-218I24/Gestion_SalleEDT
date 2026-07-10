using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("utilisateur")]
    [Index(nameof(Email), IsUnique = true)]
    public class Utilisateur
    {
        [Key]
        [Column("id_utilisateur")]
        public int IdUtilisateur { get; set; }

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

        [Required]
        [StringLength(20)]
        [Column("role")]
        public string Role { get; set; }

        // Sécurité — mot de passe hashé (BCrypt)
        [StringLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        // Token d'activation envoyé par email
        [StringLength(255)]
        [Column("token_activation")]
        public string? TokenActivation { get; set; }

        [Column("token_expiration")]
        public DateTime? TokenExpiration { get; set; }

        // Statut du compte : EnAttente, Actif, Suspendu, Desactive
        [StringLength(30)]
        [Column("statut_compte")]
        public string StatutCompte { get; set; } = "EnAttente";

        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [Column("date_derniere_connexion")]
        public DateTime? DateDerniereConnexion { get; set; }

        // Indique si c'est le premier login (obligation de changer mdp)
        [Column("premier_login")]
        public bool PremierLogin { get; set; } = true;

        // Navigation vers le professeur associé
        public virtual Professeur? Professeur { get; set; }
    }
}

