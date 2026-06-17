using System.Collections.Generic;
using System.Threading.Tasks;
using Gestion_SalleClasseEDT.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_SalleClasseEDT.Services
{
    public class SubjectService
    {
        private readonly EMITDbContext _context;

        public SubjectService(EMITDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetAllAsync()
        {
            return await _context.Subjects.ToListAsync();
        }

        public async Task<Subject> GetByIdAsync(int id)
        {
            return await _context.Subjects.FindAsync(id);
        }

        public async Task<Subject> CreateAsync(Subject subject, string user = null)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = nameof(Subject),
                EntityId = subject.Id,
                Operation = "Create",
                ChangedBy = user ?? "system",
                ChangedAt = System.DateTime.UtcNow,
                Details = subject.Label
            });

            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task UpdateAsync(Subject subject, string user = null)
        {
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = nameof(Subject),
                EntityId = subject.Id,
                Operation = "Update",
                ChangedBy = user ?? "system",
                ChangedAt = System.DateTime.UtcNow,
                Details = subject.Label
            });

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string user = null)
        {
            var s = await _context.Subjects.FindAsync(id);
            if (s == null) return;
            _context.Subjects.Remove(s);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = nameof(Subject),
                EntityId = id,
                Operation = "Delete",
                ChangedBy = user ?? "system",
                ChangedAt = System.DateTime.UtcNow,
                Details = s.Label
            });

            await _context.SaveChangesAsync();
        }
    }
}
