using System;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string entity, int? entityId, string operation, string details, string changedBy = "System / Admin");
    }

    public class AuditService : IAuditService
    {
        private readonly EMITDbContext _context;

        public AuditService(EMITDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string entity, int? entityId, string operation, string details, string changedBy = "System / Admin")
        {
            var log = new AuditLog
            {
                Entity = entity,
                EntityId = entityId,
                Operation = operation,
                Details = details,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}