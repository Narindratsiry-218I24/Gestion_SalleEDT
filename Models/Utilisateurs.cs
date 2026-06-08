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

}
}
