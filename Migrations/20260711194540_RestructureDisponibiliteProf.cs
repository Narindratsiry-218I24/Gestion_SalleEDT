using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class RestructureDisponibiliteProf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "semaine_type",
                schema: "public",
                table: "disponibilite_prof",
                type: "bpchar",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "bpchar",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "jour_semaine",
                schema: "public",
                table: "disponibilite_prof",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_specifique",
                schema: "public",
                table: "disponibilite_prof",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_annee",
                schema: "public",
                table: "disponibilite_prof",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "jour_semaine_code",
                schema: "public",
                table: "disponibilite_prof",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "type_disponibilite",
                schema: "public",
                table: "disponibilite_prof",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_creation",
                schema: "public",
                table: "demande_edt",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "hors_disponibilite",
                schema: "public",
                table: "demande_edt",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_disponibilite_prof_id_annee",
                schema: "public",
                table: "disponibilite_prof",
                column: "id_annee");

            migrationBuilder.AddForeignKey(
                name: "FK_disponibilite_prof_annee_academique_id_annee",
                schema: "public",
                table: "disponibilite_prof",
                column: "id_annee",
                principalSchema: "public",
                principalTable: "annee_academique",
                principalColumn: "id_annee");

            // Migration des données existantes (Phase 1.3)
            migrationBuilder.Sql(@"
                UPDATE public.disponibilite_prof
                SET type_disponibilite = 0, jour_semaine_code = jour_semaine
                WHERE jour_semaine IS NOT NULL AND length(jour_semaine) <= 3;
            ");
            
            migrationBuilder.Sql(@"
                UPDATE public.disponibilite_prof
                SET type_disponibilite = 1, date_specifique = CAST(jour_semaine AS date)
                WHERE jour_semaine IS NOT NULL AND (jour_semaine LIKE '%-%' OR jour_semaine LIKE '%/%');
            ");

            migrationBuilder.Sql(@"
                UPDATE public.disponibilite_prof
                SET id_annee = (SELECT id_annee FROM public.annee_academique WHERE est_active = true LIMIT 1)
                WHERE id_annee IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_disponibilite_prof_annee_academique_id_annee",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropIndex(
                name: "IX_disponibilite_prof_id_annee",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropColumn(
                name: "date_specifique",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropColumn(
                name: "id_annee",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropColumn(
                name: "jour_semaine_code",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropColumn(
                name: "type_disponibilite",
                schema: "public",
                table: "disponibilite_prof");

            migrationBuilder.DropColumn(
                name: "date_creation",
                schema: "public",
                table: "demande_edt");

            migrationBuilder.DropColumn(
                name: "hors_disponibilite",
                schema: "public",
                table: "demande_edt");

            migrationBuilder.AlterColumn<string>(
                name: "semaine_type",
                schema: "public",
                table: "disponibilite_prof",
                type: "bpchar",
                maxLength: 1,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "bpchar",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "jour_semaine",
                schema: "public",
                table: "disponibilite_prof",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
