using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("groupes")]
    public class Groupe
    {
        [Key]
        [Column("id_groupe")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Column("id_classe")]
        public int ClasseId { get; set; }

        [ForeignKey("ClasseId")]
        public virtual Classe Classe { get; set; }
    }
}