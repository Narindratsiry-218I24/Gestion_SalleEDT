using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class AutoFixSequenceAndPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_creation",
                schema: "public",
                table: "utilisateur",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "date_derniere_connexion",
                schema: "public",
                table: "utilisateur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                schema: "public",
                table: "utilisateur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "premier_login",
                schema: "public",
                table: "utilisateur",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "statut_compte",
                schema: "public",
                table: "utilisateur",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "token_activation",
                schema: "public",
                table: "utilisateur",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "token_expiration",
                schema: "public",
                table: "utilisateur",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur",
                column: "id_utilisateur",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur");

            migrationBuilder.DropColumn(
                name: "date_creation",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "date_derniere_connexion",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "password_hash",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "premier_login",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "statut_compte",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "token_activation",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.DropColumn(
                name: "token_expiration",
                schema: "public",
                table: "utilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur",
                column: "id_utilisateur");
        }
    }
}
