using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Gestion_SalleClasseEDT.Services
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string toEmail, string toName, string motDePasse, string lienActivation);
        Task SendPasswordChangedEmailAsync(string toEmail, string toName);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var s = GetSettings();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(s.FromName, s.FromAddress));
            message.To.Add(new MailboxAddress(toEmail, toEmail)); // Simple nom = email
            message.Subject = subject;

            var body = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = body.ToMessageBody();

            await SendAsync(message, s);
        }

        private SmtpSettings GetSettings() => new SmtpSettings
        {
            Host     = _config["Mail:Host"]     ?? "attwood.dnshostnetwork.com",
            Port     = int.Parse(_config["Mail:Port"] ?? "465"),
            Username = _config["Mail:Username"] ?? "edunovam@edunova.mg",
            Password = _config["Mail:Password"] ?? "P@ssword#1234",
            FromAddress = _config["Mail:FromAddress"] ?? "edunovam@edunova.mg",
            FromName    = _config["Mail:FromName"]    ?? "EDUNOVA",
            UseSsl   = (_config["Mail:Encryption"] ?? "ssl").ToLower() == "ssl"
        };

        public async Task SendActivationEmailAsync(string toEmail, string toName, string motDePasse, string lienActivation)
        {
            var s = GetSettings();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(s.FromName, s.FromAddress));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = " Activation de votre compte EMIT – Identifiants de connexion";

            var body = new BodyBuilder
            {
                HtmlBody = BuildActivationHtml(toName, toEmail, motDePasse, lienActivation)
            };
            message.Body = body.ToMessageBody();

            await SendAsync(message, s);
        }

        public async Task SendPasswordChangedEmailAsync(string toEmail, string toName)
        {
            var s = GetSettings();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(s.FromName, s.FromAddress));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Mot de passe modifié – Compte EMIT";

            var body = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family:Inter,Arial,sans-serif;max-width:600px;margin:0 auto;'>
                  <div style='background:#17203A;padding:24px;border-radius:12px 12px 0 0;'>
                    <h1 style='color:#fff;margin:0;font-size:20px;'> EMIT – Système de gestion</h1>
                  </div>
                  <div style='background:#f8fafc;padding:32px;border-radius:0 0 12px 12px;'>
                    <p style='color:#374151;'>Bonjour <strong>{toName}</strong>,</p>
                    <p style='color:#374151;'>Votre mot de passe a été modifié avec succès. Votre compte est maintenant <strong>actif</strong>.</p>
                    <p style='color:#6b7280;font-size:13px;margin-top:24px;'>Si vous n'êtes pas à l'origine de cette modification, contactez immédiatement l'administrateur.</p>
                  </div>
                </div>"
            };
            message.Body = body.ToMessageBody();

            await SendAsync(message, s);
        }

        private async Task SendAsync(MimeMessage message, SmtpSettings s)
        {
            using var client = new SmtpClient();
            var secureOption = s.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(s.Host, s.Port, secureOption);
            await client.AuthenticateAsync(s.Username, s.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string BuildActivationHtml(string nom, string email, string motDePasse, string lien)
        {
            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f0f4f8;font-family:Inter,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f0f4f8;padding:40px 0;'>
    <tr><td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
        
        <!-- HEADER -->
        <tr>
          <td style='background:linear-gradient(135deg,#17203A 0%,#2d4a8a 100%);padding:36px 40px;'>
            <h1 style='color:#fff;margin:0;font-size:22px;font-weight:700;letter-spacing:-0.5px;'>🎓 EMIT – Portail Académique</h1>
            <p style='color:rgba(255,255,255,0.7);margin:6px 0 0;font-size:13px;'>Ecole Militaire d'Ingénieurs et de Technologie</p>
          </td>
        </tr>

        <!-- BODY -->
        <tr>
          <td style='padding:40px;'>
            <h2 style='color:#17203A;font-size:18px;margin:0 0 16px;'>Bienvenue, {nom} !</h2>
            <p style='color:#374151;font-size:14px;line-height:1.6;margin:0 0 24px;'>
              Votre compte enseignant a été créé par l'administration. Voici vos identifiants de connexion :
            </p>

            <!-- Credentials Box -->
            <div style='background:#f8fafc;border:1px solid #e5e7eb;border-radius:12px;padding:24px;margin:0 0 28px;'>
              <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                  <td style='padding:8px 0;'>
                    <p style='margin:0;color:#6b7280;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.5px;'>Email</p>
                    <p style='margin:4px 0 0;color:#111827;font-size:15px;font-weight:600;'>{email}</p>
                  </td>
                </tr>
                <tr><td style='padding:12px 0;border-top:1px solid #e5e7eb;margin-top:12px;'></td></tr>
                <tr>
                  <td>
                    <p style='margin:0;color:#6b7280;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.5px;'>Mot de passe temporaire</p>
                    <p style='margin:4px 0 0;'>
                      <code style='background:#17203A;color:#60a5fa;padding:6px 14px;border-radius:8px;font-size:16px;font-weight:700;letter-spacing:2px;'>{motDePasse}</code>
                    </p>
                  </td>
                </tr>
              </table>
            </div>

            <!-- Warning -->
            <div style='background:#fffbeb;border:1px solid #fcd34d;border-radius:10px;padding:14px 18px;margin:0 0 28px;'>
              <p style='margin:0;color:#92400e;font-size:13px;'>
                 <strong>Important :</strong> Vous devrez changer ce mot de passe lors de votre première connexion.
              </p>
            </div>

            <!-- CTA Button -->
            <div style='text-align:center;margin:28px 0;'>
              <a href='{lien}' style='display:inline-block;background:linear-gradient(135deg,#2563eb,#1d4ed8);color:#fff;text-decoration:none;padding:14px 36px;border-radius:10px;font-size:15px;font-weight:700;letter-spacing:0.3px;box-shadow:0 4px 12px rgba(37,99,235,0.3);'>
                 Activer mon compte
              </a>
            </div>

            <p style='color:#6b7280;font-size:12px;text-align:center;margin:0;'>
              Ce lien est valable <strong>48 heures</strong>. Après expiration, contactez l'administrateur.
            </p>
          </td>
        </tr>

        <!-- FOOTER -->
        <tr>
          <td style='background:#f8fafc;padding:20px 40px;border-top:1px solid #e5e7eb;'>
            <p style='color:#9ca3af;font-size:12px;margin:0;text-align:center;'>
              EMIT – Système de gestion des emplois du temps &nbsp;|&nbsp; 
              <a href='mailto:edunovam@edunova.mg' style='color:#6b7280;'>edunovam@edunova.mg</a>
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
        }
    }

    internal class SmtpSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 465;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName { get; set; } = "";
        public bool UseSsl { get; set; } = true;
    }
}
