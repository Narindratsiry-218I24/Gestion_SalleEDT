using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class EnrichProfesseurModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "biographie",
                schema: "public",
                table: "professeur",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "capacite_horaire_max",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<string>(
                name: "cv_url",
                schema: "public",
                table: "professeur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_creation",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_embauche",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_modification",
                schema: "public",
                table: "professeur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "departement",
                schema: "public",
                table: "professeur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "est_actif",
                schema: "public",
                table: "professeur",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "grade",
                schema: "public",
                table: "professeur",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "heures_effectuees",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_utilisateur",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "matricule",
                schema: "public",
                table: "professeur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                schema: "public",
                table: "professeur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "specialite",
                schema: "public",
                table: "professeur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "specialites_secondaires",
                schema: "public",
                table: "professeur",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "statut",
                schema: "public",
                table: "professeur",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "telephone_portable",
                schema: "public",
                table: "professeur",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "titre",
                schema: "public",
                table: "professeur",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE public.professeur SET matricule = 'MAT-' || id_professeur::text;");

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur",
                column: "id_utilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_professeur_matricule",
                schema: "public",
                table: "professeur",
                column: "matricule",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_professeur_utilisateur_id_utilisateur",
                schema: "public",
                table: "professeur",
                column: "id_utilisateur",
                principalSchema: "public",
                principalTable: "utilisateur",
                principalColumn: "id_utilisateur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_professeur_utilisateur_id_utilisateur",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropIndex(
                name: "IX_professeur_matricule",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "biographie",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "capacite_horaire_max",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "cv_url",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "date_creation",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "date_embauche",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "date_modification",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "departement",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "est_actif",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "grade",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "heures_effectuees",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "id_utilisateur",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "matricule",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "photo_url",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "specialite",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "specialites_secondaires",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "statut",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "telephone_portable",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "titre",
                schema: "public",
                table: "professeur");
        }
    }
}
