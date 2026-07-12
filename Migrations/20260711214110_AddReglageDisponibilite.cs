using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class AddReglageDisponibilite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reglage_disponibilite",
                schema: "public",
                columns: table => new
                {
                    id_reglage = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_annee = table.Column<int>(type: "integer", nullable: false),
                    dimanche_autorise = table.Column<bool>(type: "boolean", nullable: false),
                    samedi_autorise = table.Column<bool>(type: "boolean", nullable: false),
                    heure_ouverture = table.Column<TimeSpan>(type: "interval", nullable: false),
                    heure_fermeture = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duree_min_creneau_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reglage_disponibilite", x => x.id_reglage);
                    table.ForeignKey(
                        name: "FK_reglage_disponibilite_annee_academique_id_annee",
                        column: x => x.id_annee,
                        principalSchema: "public",
                        principalTable: "annee_academique",
                        principalColumn: "id_annee",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reglage_disponibilite_id_annee",
                schema: "public",
                table: "reglage_disponibilite",
                column: "id_annee");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reglage_disponibilite",
                schema: "public");
        }
    }
}
