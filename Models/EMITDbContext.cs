using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Npgsql;

namespace Gestion_SalleClasseEDT.Models
{
    public class EMITDbContext : DbContext
{
    public EMITDbContext()
    {
    }

    public EMITDbContext(DbContextOptions<EMITDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString());
        }
    }

    private static string GetConnectionString()
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "EMIT_EDT_DB";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "tsiririmlay";

        return $"Server={server};Port={port};Database={database};User Id={user};Password={password};";
    }

    public DbSet<Mention> Mentions { get; set; }
    public DbSet<Filiere> Filieres { get; set; }
    public DbSet<Niveau> Niveaux { get; set; }
    public DbSet<RefSemestre> RefSemestres { get; set; }
    public DbSet<AnneeAcademique> AnneesAcademiques { get; set; }
    public DbSet<Semestre> Semestres { get; set; }
    public DbSet<Classe> Classes { get; set; }
    public DbSet<AffectationMatiere> AffectationsMatieres { get; set; }
    public DbSet<Matiere> Matieres { get; set; }
    public DbSet<Professeur> Professeurs { get; set; }
    public DbSet<Salle> Salles { get; set; }
    public DbSet<DisponibiliteProf> DisponibilitesProf { get; set; }
    public DbSet<Cours> Cours { get; set; }
    public DbSet<Seance> Seances { get; set; }
    public DbSet<Groupe> Groupes { get; set; }
    public DbSet<Creneau> Creneaux { get; set; }
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<DemandeEdt> DemandesEdt { get; set; }
    public DbSet<PropositionAdmin> PropositionsAdmin { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Default schema for PostgreSQL is usually "public"
        modelBuilder.HasDefaultSchema("public");

        // Additional configurations
        modelBuilder.Entity<Subject>(b =>
        {
            b.HasIndex(s => s.Code).IsUnique();
        });

        modelBuilder.Entity<Schedule>(b =>
        {
            b.HasOne(s => s.Subject).WithMany();
            b.HasOne(s => s.Professeur).WithMany();
            b.HasOne(s => s.Salle).WithMany();
        });

        modelBuilder.Entity<Professeur>(b =>
        {
            b.HasIndex(p => p.Matricule).IsUnique();
            b.Property(p => p.SpecialitesSecondaires).HasColumnType("jsonb");
            b.Property(p => p.DateCreation).HasDefaultValueSql("now()");
            b.Property(p => p.EstActif).HasDefaultValue(true);
            b.Property(p => p.CapaciteHoraireMax).HasDefaultValue(20);
        });

        modelBuilder.Entity<Classe>(b =>
        {
            b.HasIndex(c => new { c.IdAnneeAcademique, c.CodeClasse }).IsUnique();
            b.HasOne(c => c.AnneeAcademique)
                .WithMany(a => a.Classes)
                .HasForeignKey(c => c.IdAnneeAcademique)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AffectationMatiere>(b =>
        {
            b.HasIndex(a => new { a.IdClasse, a.IdMatiere, a.IdProfesseur, a.IdSemestre }).IsUnique();
            b.Property(a => a.HeuresCm).HasDefaultValue(0);
            b.Property(a => a.HeuresTd).HasDefaultValue(0);
            b.Property(a => a.HeuresTp).HasDefaultValue(0);
            b.Property(a => a.EstActif).HasDefaultValue(true);
            b.HasOne(a => a.Classe)
                .WithMany(c => c.AffectationsMatieres)
                .HasForeignKey(a => a.IdClasse)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Semestre)
                .WithMany(s => s.AffectationsMatieres)
                .HasForeignKey(a => a.IdSemestre)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Matiere)
                .WithMany(m => m.AffectationsMatieres)
                .HasForeignKey(a => a.IdMatiere)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(a => a.Professeur)
                .WithMany(p => p.AffectationsMatieres)
                .HasForeignKey(a => a.IdProfesseur)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cours>(b =>
        {
            b.HasOne(c => c.AffectationMatiere)
                .WithMany(a => a.Cours)
                .HasForeignKey(c => c.IdAffectation)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Cours>()
            .HasMany(c => c.Seances)
            .WithOne(s => s.Cours)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
}
