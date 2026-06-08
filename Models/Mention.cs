using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("mention")]
[Index(nameof(CodeMention), IsUnique = true)]
public class Mention
{
    [Key]
    [Column("id_mention")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdMention { get; set; }

    [Required]
    [StringLength(20)]
    [Column("code_mention")]
    public string CodeMention { get; set; }

    [StringLength(100)]
    [Column("nom_mention")]
    public string NomMention { get; set; }

    // Navigation properties
    public virtual ICollection<Filiere> Filieres { get; set; }
    public virtual ICollection<Niveau> Niveaux { get; set; }
}
}