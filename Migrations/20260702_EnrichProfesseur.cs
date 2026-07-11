using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    public partial class EnrichProfesseur : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "matricule",
                schema: "public",
                table: "professeur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

                        // Nettoyage automatique des matricules en doublon / vides (option A)
                        migrationBuilder.Sql(@"
-- normaliser les chaines vides en NULL
UPDATE public.professeur
SET matricule = NULL
WHERE matricule IS NOT NULL AND trim(matricule) = '';

-- suffixer les doublons en conservant la première occurrence
WITH ranked AS (
    SELECT id_professeur, matricule,
                 ROW_NUMBER() OVER (PARTITION BY matricule ORDER BY id_professeur) AS rn
    FROM public.professeur
    WHERE matricule IS NOT NULL
)
UPDATE public.professeur p
SET matricule = p.matricule || '_' || p.id_professeur::text
FROM ranked r
WHERE p.id_professeur = r.id_professeur AND r.rn > 1;
");

            migrationBuilder.AddColumn<string>(
                name: "titre",
                schema: "public",
                table: "professeur",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telephone_portable",
                schema: "public",
                table: "professeur",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "specialite",
                schema: "public",
                table: "professeur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "specialites_secondaires",
                schema: "public",
                table: "professeur",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "departement",
                schema: "public",
                table: "professeur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_embauche",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "statut",
                schema: "public",
                table: "professeur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                schema: "public",
                table: "professeur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "biographie",
                schema: "public",
                table: "professeur",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cv_url",
                schema: "public",
                table: "professeur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_utilisateur",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "capacite_horaire_max",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<bool>(
                name: "est_actif",
                schema: "public",
                table: "professeur",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_creation",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_modification",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "heures_effectuees",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_professeur_matricule",
                schema: "public",
                table: "professeur",
                column: "matricule",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professeur_matricule",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(name: "matricule", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "titre", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "telephone_portable", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "specialite", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "specialites_secondaires", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "departement", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "date_embauche", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "statut", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "photo_url", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "biographie", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "cv_url", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "id_utilisateur", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "capacite_horaire_max", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "est_actif", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "date_creation", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "date_modification", schema: "public", table: "professeur");
            migrationBuilder.DropColumn(name: "heures_effectuees", schema: "public", table: "professeur");
        }
    }
}
