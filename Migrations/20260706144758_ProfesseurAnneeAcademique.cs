using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class ProfesseurAnneeAcademique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "departement",
                schema: "public",
                table: "professeur");

            migrationBuilder.AddColumn<bool>(
                name: "est_archivee",
                schema: "public",
                table: "semestre",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "id_annee_academique",
                schema: "public",
                table: "professeur",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "est_archivee",
                schema: "public",
                table: "classe",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_archivage",
                schema: "public",
                table: "annee_academique",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "est_active",
                schema: "public",
                table: "annee_academique",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "est_archivee",
                schema: "public",
                table: "annee_academique",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_annee_academique",
                schema: "public",
                table: "professeur",
                column: "id_annee_academique");

            migrationBuilder.AddForeignKey(
                name: "FK_professeur_annee_academique_id_annee_academique",
                schema: "public",
                table: "professeur",
                column: "id_annee_academique",
                principalSchema: "public",
                principalTable: "annee_academique",
                principalColumn: "id_annee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_professeur_annee_academique_id_annee_academique",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropIndex(
                name: "IX_professeur_id_annee_academique",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "est_archivee",
                schema: "public",
                table: "semestre");

            migrationBuilder.DropColumn(
                name: "id_annee_academique",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "est_archivee",
                schema: "public",
                table: "classe");

            migrationBuilder.DropColumn(
                name: "date_archivage",
                schema: "public",
                table: "annee_academique");

            migrationBuilder.DropColumn(
                name: "est_active",
                schema: "public",
                table: "annee_academique");

            migrationBuilder.DropColumn(
                name: "est_archivee",
                schema: "public",
                table: "annee_academique");

            migrationBuilder.AddColumn<string>(
                name: "departement",
                schema: "public",
                table: "professeur",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
