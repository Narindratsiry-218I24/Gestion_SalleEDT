using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Gestion_SalleClasseEDT.Models;
using Gestion_SalleClasseEDT.Services;

namespace Gestion_SalleClasseEDT.Controllers
{
    public class ProfesseursController : Controller
    {
        private readonly EMITDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string[] _allowedImageExt = new[] { ".jpg", ".jpeg", ".png" };
        private readonly string[] _allowedCvExt = new[] { ".pdf" };
        private const long MaxFileBytes = 2 * 1024 * 1024; // 2MB

        public ProfesseursController(
            EMITDbContext context,
            IWebHostEnvironment env,
            IEmailService emailService,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = context;
            _env = env;
            _emailService = emailService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        // ─────────────────────────────────────────────────────────────
        // INDEX
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var list = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .ToListAsync();
            return View(list);
        }

        // ─────────────────────────────────────────────────────────────
        // DETAILS
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.IdProfesseur == id);
            if (p == null) return NotFound();
            return View(p);
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE GET
        // ─────────────────────────────────────────────────────────────
        public IActionResult Create()
        {
            var maxId = _db.Professeurs.Max(p => (int?)p.IdProfesseur) ?? 0;
            ViewBag.NewMatricule = $"PROF-{(maxId + 1):D4}";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // SEND VERIFICATION CODE
        // ─────────────────────────────────────────────────────────────
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string code, DateTime expiry)> _verificationCodes = new();

        [HttpPost]
        public async Task<IActionResult> SendVerificationCode(string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Email vide." });
            
            if (await _db.Utilisateurs.AnyAsync(u => u.Email == email))
                return Json(new { success = false, message = "Cet email est déjà utilisé." });

            var code = new Random().Next(100000, 999999).ToString();
            var emailKey = email.Trim().ToLowerInvariant();
            _verificationCodes[emailKey] = (code, DateTime.UtcNow.AddMinutes(15));

            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Code de vérification",
                    $"Votre code de vérification est : <b>{code}</b>. Il est valable 15 minutes."
                );
                return Json(new { success = true, message = "Code envoyé avec succès." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SMTP WARNING] Failed to send code to {email}: {ex.Message}");
                return Json(new { success = true, message = $"SMTP hors-ligne. Utilisez le code de test : {code}" });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE POST
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Professeur professeur, IFormFile photoFile, IFormFile cvFile, string VerificationCode)
        {
            var allowedKeys = new[] { "Nom", "Prenom", "Email", "Telephone", "Grade", "Specialite", "Departement", "DateEmbauche", "Statut", "Biographie", "VerificationCode" };
            foreach (var key in ModelState.Keys.ToList())
            {
                if (!allowedKeys.Contains(key))
                {
                    ModelState.Remove(key);
                }
            }

            if (!ModelState.IsValid) return View(professeur);

            // Vérifier si l'email existe déjà
            if (await _db.Utilisateurs.AnyAsync(u => u.Email == professeur.Email))
            {
                ModelState.AddModelError("Email", "Un compte avec cette adresse email existe déjà.");
                return View(professeur);
            }

            // Vérification OTP : optionnelle — si un code a été envoyé et est présent, on le valide
            // Si aucun code n'a été envoyé (pas de SMTP ou admin choisit de sauter), on continue quand même
            bool otpVerified = false;
            if (!string.IsNullOrWhiteSpace(VerificationCode))
            {
                var emailKey = professeur.Email?.Trim().ToLowerInvariant() ?? "";
                if (_verificationCodes.TryGetValue(emailKey, out var val) &&
                    val.code == VerificationCode.Trim() && val.expiry >= DateTime.UtcNow)
                {
                    otpVerified = true;
                    _verificationCodes.TryRemove(emailKey, out _);
                }
                else
                {
                    ModelState.AddModelError("VerificationCode", "Code de vérification invalide ou expiré. Veuillez renvoyer le code.");
                    return View(professeur);
                }
            }

            // Validate files
            if (photoFile != null)
            {
                var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                if (!_allowedImageExt.Contains(ext) || photoFile.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("photoFile", "Photo: types autorisés jpg,png et taille max 2MB.");
                    return View(professeur);
                }
            }
            if (cvFile != null)
            {
                var ext = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                if (!_allowedCvExt.Contains(ext) || cvFile.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("cvFile", "CV: format PDF et taille max 2MB.");
                    return View(professeur);
                }
            }

            // ── 1. Générer un mot de passe temporaire aléatoire ──────
            var motDePasseTemp = GeneratePassword();
            var token          = GenerateSecureToken();
            var tokenExpiry    = DateTime.UtcNow.AddHours(48);

            // ── 2. Créer le compte utilisateur ───────────────────────
            var utilisateur = new Utilisateur
            {
                Nom           = professeur.Nom,
                Prenom        = professeur.Prenom,
                Email         = professeur.Email,
                Role          = "professeur",
                PasswordHash  = HashPassword(motDePasseTemp),
                TokenActivation = token,
                TokenExpiration = tokenExpiry,
                StatutCompte  = "EnAttente",
                PremierLogin  = true,
                DateCreation  = DateTime.UtcNow
            };

            // FIX: Reset PostgreSQL sequences automatically if they are out of sync
            try {
                await _db.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('public.utilisateur', 'id_utilisateur'), coalesce(max(id_utilisateur),0) + 1, false) FROM public.utilisateur;");
                await _db.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('public.professeur', 'id_professeur'), coalesce(max(id_professeur),0) + 1, false) FROM public.professeur;");
            } catch { /* Ignored if not postgres or error */ }

            _db.Utilisateurs.Add(utilisateur);
            await _db.SaveChangesAsync(); // Obtenir l'ID

            // ── 3. Lier le professeur au compte utilisateur ───────────
            professeur.IdProfesseur = utilisateur.IdUtilisateur; // FORCER LE MÊME ID
            professeur.IdUtilisateur = utilisateur.IdUtilisateur;
            professeur.DateCreation  = DateTime.UtcNow;
            if (professeur.CapaciteHoraireMax == 0) professeur.CapaciteHoraireMax = 20;
            if (string.IsNullOrEmpty(professeur.Matricule))
            {
                var maxId = await _db.Professeurs.MaxAsync(p => (int?)p.IdProfesseur) ?? 0;
                professeur.Matricule = $"PROF-{(maxId + 1):D4}";
            }
            professeur.EstActif = true;
            professeur.CvUrl = ""; // Valeur par défaut pour éviter l'erreur NOT NULL
            professeur.PhotoUrl = ""; // Idem
            if (string.IsNullOrEmpty(professeur.SpecialitesSecondaires))
            {
                professeur.SpecialitesSecondaires = "[]";
            }
            if (string.IsNullOrEmpty(professeur.Titre))
            {
                professeur.Titre = "M.";
            }
            if (string.IsNullOrEmpty(professeur.Telephone))
            {
                professeur.Telephone = "";
            }
            if (string.IsNullOrEmpty(professeur.TelephonePortable))
            {
                professeur.TelephonePortable = "";
            }
            if (string.IsNullOrEmpty(professeur.Biographie))
            {
                professeur.Biographie = "";
            }
            if (professeur.Grade == null) professeur.Grade = "";
            if (professeur.Specialite == null) professeur.Specialite = "";
            if (professeur.Statut == null) professeur.Statut = "";

            // Fix Npgsql UTC DateTime restriction for DateEmbauche
            if (professeur.DateEmbauche.HasValue && professeur.DateEmbauche.Value.Kind == DateTimeKind.Unspecified)
            {
                professeur.DateEmbauche = DateTime.SpecifyKind(professeur.DateEmbauche.Value, DateTimeKind.Utc);
            }

            _db.Professeurs.Add(professeur);
            await _db.SaveChangesAsync();

            // ── 4. Upload fichiers ────────────────────────────────────
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "professeurs", professeur.IdProfesseur.ToString());
            Directory.CreateDirectory(uploadsRoot);

            if (photoFile != null)
            {
                var ext      = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                var fileName = "photo" + ext;
                var filePath = Path.Combine(uploadsRoot, fileName);
                using (var stream = System.IO.File.Create(filePath))
                    await photoFile.CopyToAsync(stream);
                professeur.PhotoUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
            }

            if (cvFile != null)
            {
                var fileName = "cv" + Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                var filePath = Path.Combine(uploadsRoot, fileName);
                using (var stream = System.IO.File.Create(filePath))
                    await cvFile.CopyToAsync(stream);
                professeur.CvUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
            }

            _db.Entry(professeur).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            // ── 5. Envoyer l'email d'activation ──────────────────────
            string emailSent = "non";
            try
            {
                var baseUrl = GetBaseUrl();
                var lienActivation = $"{baseUrl}/Professeurs/ActivationCompte?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(professeur.Email)}";
                await _emailService.SendActivationEmailAsync(
                    professeur.Email,
                    $"{professeur.Prenom} {professeur.Nom}",
                    motDePasseTemp,
                    lienActivation
                );
                emailSent = "oui";
            }
            catch (Exception ex)
            {
                // Email non bloquant — le compte est créé mais on signale l'erreur
                TempData["EmailError"] = $"Professeur créé mais l'email n'a pas pu être envoyé : {ex.Message}";
            }

            TempData["SuccessMessage"] = emailSent == "oui"
                ? $"Professeur {professeur.Prenom} {professeur.Nom} créé avec succès. Un email d'activation a été envoyé à {professeur.Email}."
                : $"Professeur créé mais l'email n'a pas pu être envoyé. Mot de passe temporaire : {motDePasseTemp}";

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────
        // ACTIVATION COMPTE (GET) — formulaire changement mdp
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ActivationCompte(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return View("ActivationErreur", "Lien d'activation invalide.");

            var utilisateur = await _db.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email && u.TokenActivation == token);

            if (utilisateur == null)
                return View("ActivationErreur", "Lien d'activation invalide ou déjà utilisé.");

            if (utilisateur.TokenExpiration < DateTime.UtcNow)
                return View("ActivationErreur", "Ce lien d'activation a expiré. Contactez l'administrateur.");

            ViewBag.Email = email;
            ViewBag.Token = token;
            ViewBag.Nom   = $"{utilisateur.Prenom} {utilisateur.Nom}";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // ACTIVATION COMPTE (POST) — enregistre le nouveau mdp
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivationCompte(string token, string email, string nouveauMotDePasse, string confirmationMotDePasse)
        {
            if (nouveauMotDePasse != confirmationMotDePasse)
            {
                ViewBag.Email  = email;
                ViewBag.Token  = token;
                ModelState.AddModelError("", "Les mots de passe ne correspondent pas.");
                return View();
            }

            if (nouveauMotDePasse.Length < 8)
            {
                ViewBag.Email = email;
                ViewBag.Token = token;
                ModelState.AddModelError("", "Le mot de passe doit contenir au moins 8 caractères.");
                return View();
            }

            var utilisateur = await _db.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email && u.TokenActivation == token);

            if (utilisateur == null || utilisateur.TokenExpiration < DateTime.UtcNow)
                return View("ActivationErreur", "Lien invalide ou expiré.");

            // Mettre à jour le compte
            utilisateur.PasswordHash    = HashPassword(nouveauMotDePasse);
            utilisateur.TokenActivation = null;
            utilisateur.TokenExpiration = null;
            utilisateur.StatutCompte    = "Actif";
            utilisateur.PremierLogin    = false;
            await _db.SaveChangesAsync();

            // Mettre à jour le statut du professeur lié
            var professeur = await _db.Professeurs.FirstOrDefaultAsync(p => p.IdUtilisateur == utilisateur.IdUtilisateur);
            if (professeur != null)
            {
                professeur.Statut = "Actif";
                await _db.SaveChangesAsync();
            }

            // Envoyer notification de confirmation
            try { await _emailService.SendPasswordChangedEmailAsync(utilisateur.Email, $"{utilisateur.Prenom} {utilisateur.Nom}"); }
            catch { /* non bloquant */ }

            TempData["ActivationOk"] = true;
            return RedirectToAction("ActivationSuccess");
        }

        // ─────────────────────────────────────────────────────────────
        // ACTIVATION SUCCESS
        // ─────────────────────────────────────────────────────────────
        public IActionResult ActivationSuccess() => View();

        // ─────────────────────────────────────────────────────────────
        // RENVOYER EMAIL D'ACTIVATION (admin)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RenvoyerEmail(int id)
        {
            var professeur = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.IdProfesseur == id);

            if (professeur?.Utilisateur == null)
                return Json(new { success = false, message = "Professeur ou compte introuvable." });

            if (professeur.Utilisateur.StatutCompte == "Actif")
                return Json(new { success = false, message = "Le compte est déjà actif." });

            // Régénérer le token et le mot de passe
            var motDePasseTemp = GeneratePassword();
            var token          = GenerateSecureToken();
            var tokenExpiry    = DateTime.UtcNow.AddHours(48);

            professeur.Utilisateur.PasswordHash    = HashPassword(motDePasseTemp);
            professeur.Utilisateur.TokenActivation = token;
            professeur.Utilisateur.TokenExpiration = tokenExpiry;
            professeur.Utilisateur.StatutCompte    = "EnAttente";
            await _db.SaveChangesAsync();

            try
            {
                var baseUrl = GetBaseUrl();
                var lien    = $"{baseUrl}/Professeurs/ActivationCompte?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(professeur.Email)}";
                await _emailService.SendActivationEmailAsync(
                    professeur.Email,
                    $"{professeur.Prenom} {professeur.Nom}",
                    motDePasseTemp,
                    lien
                );
                return Json(new { success = true, message = "Email d'activation renvoyé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur SMTP : {ex.Message}" });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // CHANGER STATUT COMPTE (suspendre / réactiver)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ChangerStatutCompte(int id, string statut)
        {
            var professeur = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.IdProfesseur == id);

            if (professeur?.Utilisateur == null)
                return Json(new { success = false, message = "Compte introuvable." });

            var statutsValides = new[] { "Actif", "Suspendu", "Desactive" };
            if (!statutsValides.Contains(statut))
                return Json(new { success = false, message = "Statut invalide." });

            professeur.Utilisateur.StatutCompte = statut;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Compte mis à {statut}." });
        }

        // ─────────────────────────────────────────────────────────────
        // EDIT GET
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.IdProfesseur == id);
            if (p == null) return NotFound();
            return View(p);
        }

        // ─────────────────────────────────────────────────────────────
        // EDIT POST
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Professeur professeur, IFormFile photoFile, IFormFile cvFile)
        {
            if (id != professeur.IdProfesseur) return BadRequest();

            var allowedKeys = new[] { "IdProfesseur", "Nom", "Prenom", "Email", "Telephone", "Grade", "Specialite", "Departement", "DateEmbauche", "Statut", "Biographie" };
            foreach (var key in ModelState.Keys.ToList())
            {
                if (!allowedKeys.Contains(key))
                {
                    ModelState.Remove(key);
                }
            }

            if (!ModelState.IsValid) return View(professeur);

            // Vérifier si l'email existe déjà pour un autre utilisateur
            if (await _db.Utilisateurs.AnyAsync(u => u.Email == professeur.Email && u.IdUtilisateur != professeur.IdUtilisateur))
            {
                ModelState.AddModelError("Email", "Un compte avec cette adresse email existe déjà.");
                return View(professeur);
            }

            var existingProfesseur = await _db.Professeurs.FindAsync(id);
            if (existingProfesseur == null) return NotFound();

            // Validate files
            if (photoFile != null)
            {
                var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                if (!_allowedImageExt.Contains(ext) || photoFile.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("photoFile", "Photo: types autorisés jpg,png et taille max 2MB.");
                    return View(professeur);
                }
            }
            if (cvFile != null)
            {
                var ext = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                if (!_allowedCvExt.Contains(ext) || cvFile.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("cvFile", "CV: format PDF et taille max 2MB.");
                    return View(professeur);
                }
            }

            try
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "professeurs", professeur.IdProfesseur.ToString());
                Directory.CreateDirectory(uploadsRoot);

                if (photoFile != null)
                {
                    var ext      = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                    var fileName = "photo" + ext;
                    var filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                        await photoFile.CopyToAsync(stream);
                    existingProfesseur.PhotoUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
                }

                if (cvFile != null)
                {
                    var fileName = "cv" + Path.GetExtension(cvFile.FileName).ToLowerInvariant();
                    var filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                        await cvFile.CopyToAsync(stream);
                    existingProfesseur.CvUrl = $"/uploads/professeurs/{professeur.IdProfesseur}/{fileName}";
                }

                existingProfesseur.Matricule            = professeur.Matricule ?? "";
                existingProfesseur.Titre                = professeur.Titre ?? "";
                existingProfesseur.Nom                  = professeur.Nom;
                existingProfesseur.Prenom               = professeur.Prenom;
                existingProfesseur.Email                = professeur.Email;
                existingProfesseur.Telephone            = professeur.Telephone;
                existingProfesseur.TelephonePortable    = professeur.TelephonePortable ?? "";
                existingProfesseur.Grade                = professeur.Grade ?? "";
                existingProfesseur.Specialite           = professeur.Specialite ?? "";
                existingProfesseur.SpecialitesSecondaires = professeur.SpecialitesSecondaires ?? "[]";
                
                // Fix Npgsql UTC DateTime restriction for DateEmbauche
                if (professeur.DateEmbauche.HasValue && professeur.DateEmbauche.Value.Kind == DateTimeKind.Unspecified)
                {
                    existingProfesseur.DateEmbauche = DateTime.SpecifyKind(professeur.DateEmbauche.Value, DateTimeKind.Utc);
                }
                else
                {
                    existingProfesseur.DateEmbauche = professeur.DateEmbauche;
                }

                existingProfesseur.Statut               = professeur.Statut ?? "";
                existingProfesseur.CapaciteHoraireMax   = professeur.CapaciteHoraireMax;
                existingProfesseur.Biographie           = professeur.Biographie ?? "";
                existingProfesseur.DateModification     = DateTime.UtcNow;

                _db.Update(existingProfesseur);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Professeurs.Any(e => e.IdProfesseur == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE GET
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Professeurs.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE POST
        // ─────────────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Professeurs
                .Include(p => p.Utilisateur)
                .FirstOrDefaultAsync(p => p.IdProfesseur == id);
            if (p == null) return NotFound();

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "professeurs", id.ToString());
            if (Directory.Exists(uploadsRoot)) Directory.Delete(uploadsRoot, true);

            // Supprimer le compte utilisateur associé
            if (p.Utilisateur != null)
                _db.Utilisateurs.Remove(p.Utilisateur);

            _db.Professeurs.Remove(p);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS PRIVÉS
        // ─────────────────────────────────────────────────────────────
        private static string GeneratePassword()
        {
            const string upper   = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower   = "abcdefghjkmnpqrstuvwxyz";
            const string digits  = "23456789";
            const string special = "@#!$";
            const string all     = upper + lower + digits + special;

            using var rng = RandomNumberGenerator.Create();
            var buf = new byte[12];
            rng.GetBytes(buf);

            var password = new char[10];
            password[0] = upper[buf[0]  % upper.Length];
            password[1] = lower[buf[1]  % lower.Length];
            password[2] = digits[buf[2] % digits.Length];
            password[3] = special[buf[3] % special.Length];
            for (int i = 4; i < 10; i++)
                password[i] = all[buf[i] % all.Length];

            // Mélanger
            for (int i = password.Length - 1; i > 0; i--)
            {
                rng.GetBytes(buf);
                int j = buf[0] % (i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }
            return new string(password);
        }

        private static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[48];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "EMIT_SALT_2026"));
            return Convert.ToBase64String(bytes);
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return _config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            return $"{request.Scheme}://{request.Host}";
        }
    }
}
