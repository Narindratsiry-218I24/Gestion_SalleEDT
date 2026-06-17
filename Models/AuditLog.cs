using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id_audit")]
        public int Id { get; set; }

        [Required]
        [Column("entity")]
        [StringLength(100)]
        public string Entity { get; set; }

        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Required]
        [Column("operation")]
        [StringLength(20)]
        public string Operation { get; set; }

        [Column("changed_by")]
        [StringLength(100)]
        public string ChangedBy { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; }

        [Column("details")]
        public string Details { get; set; }
    }
}
