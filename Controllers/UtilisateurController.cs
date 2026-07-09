using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Controllers
{
    [Route("api/Utilisateur")]
    [ApiController]
    public class UtilisateurController : ControllerBase
    {
        private readonly EMITDbContext db;

        public UtilisateurController(EMITDbContext context)
        {
            db = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult GetUtilisateurs()
        {
            return Ok(db.Utilisateurs.ToList());
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Email))
                return BadRequest("Email requis.");

            // --- Hard-coded fallback accounts (dev / test) ----------------
            var testAccounts = new[]
            {
                new { email = "admin@emit.mg",      pass = "", id = 1, nom = "Admin",     prenom = "EMIT",  role = "admin" },
                new { email = "demandeur@emit.mg",  pass = "", id = 2, nom = "Demandeur", prenom = "Test",  role = "demandeur" },
                new { email = "validateur@emit.mg", pass = "", id = 3, nom = "Validateur",prenom = "Test",  role = "validateur" },
            };
            foreach (var ta in testAccounts)
            {
                if (login.Email.ToLower() == ta.email)
                    return Ok(new { IdUtilisateur = ta.id, Nom = ta.nom, Prenom = ta.prenom, Email = ta.email, Role = ta.role });
            }
            // ---------------------------------------------------------------

            try
            {
                var user = db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == login.Email.ToLower());
                if (user == null) return Unauthorized();

                // Verify password using the same SHA-256 + salt approach used when creating the account
                if (string.IsNullOrEmpty(user.PasswordHash) || PasswordHelper.HashPassword(login.Password) != user.PasswordHash)
                    return Unauthorized();

                // Block accounts not yet activated
                if (user.StatutCompte == "EnAttente")
                    return StatusCode(403, "Votre compte est en attente d'activation. Vérifiez votre email.");

                if (user.StatutCompte == "Suspendu" || user.StatutCompte == "Desactive")
                    return StatusCode(403, "Votre compte a été suspendu. Contactez l'administration.");

                // Update last login
                user.DateDerniereConnexion = DateTime.UtcNow;
                db.SaveChanges();

                return Ok(new
                {
                    IdUtilisateur  = user.IdUtilisateur,
                    Nom            = user.Nom,
                    Prenom         = user.Prenom,
                    Email          = user.Email,
                    Role           = user.Role,
                    PremierLogin   = user.PremierLogin,
                    StatutCompte   = user.StatutCompte
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur serveur : " + ex.Message);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetUtilisateur(int id)
        {
            var user = db.Utilisateurs.Find(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register([FromBody] Utilisateur utilisateur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (db.Utilisateurs.Any(u => u.Email == utilisateur.Email))
                return BadRequest("Cet email est déjà utilisé.");
            db.Utilisateurs.Add(utilisateur);
            db.SaveChanges();
            return Ok(utilisateur);
        }

        [HttpPut]
        [Route("{id:int}")]
        public IActionResult UpdateUtilisateur(int id, [FromBody] Utilisateur utilisateur)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != utilisateur.IdUtilisateur) return BadRequest();

            var existing = db.Utilisateurs.Find(id);
            if (existing == null) return NotFound();

            existing.Nom = utilisateur.Nom;
            existing.Prenom = utilisateur.Prenom;
            existing.Email = utilisateur.Email;
            existing.Role = utilisateur.Role;
            db.SaveChanges();
            return Ok(existing);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteUtilisateur(int id)
        {
            var user = db.Utilisateurs.Find(id);
            if (user == null) return NotFound();
            db.Utilisateurs.Remove(user);
            db.SaveChanges();
            return Ok(user);
        }
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    internal static class PasswordHelper
    {
        internal static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EMIT_SALT_2026"));
            return Convert.ToBase64String(bytes);
        }
    }
}
