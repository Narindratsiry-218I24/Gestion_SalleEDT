using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;
using System.Collections.Generic;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly EMITDbContext _db;
        private readonly IEmailService _emailService;

        public NotificationController(EMITDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications(string email)
        {
            var prof = await _db.Professeurs.FirstOrDefaultAsync(p => p.Email == email);
            if (prof == null) return NotFound();

            var notifications = await _db.Notifications
                .Where(n => n.IdProfesseur == prof.IdProfesseur && !n.EstLue)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _db.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.EstLue = true;
            notification.DateLecture = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] string email)
        {
            var prof = await _db.Professeurs.FirstOrDefaultAsync(p => p.Email == email);
            if (prof == null) return NotFound();

            var unread = await _db.Notifications
                .Where(n => n.IdProfesseur == prof.IdProfesseur && !n.EstLue)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.EstLue = true;
                n.DateLecture = DateTime.UtcNow;
            }

            if (unread.Any())
            {
                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
