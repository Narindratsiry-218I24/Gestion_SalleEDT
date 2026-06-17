using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class AddMatiereFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_professeur_responsable",
                schema: "public",
                table: "matiere",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "volume_horaire",
                schema: "public",
                table: "matiere",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_matiere_id_professeur_responsable",
                schema: "public",
                table: "matiere",
                column: "id_professeur_responsable");

            migrationBuilder.AddForeignKey(
                name: "FK_matiere_professeur_id_professeur_responsable",
                schema: "public",
                table: "matiere",
                column: "id_professeur_responsable",
                principalSchema: "public",
                principalTable: "professeur",
                principalColumn: "id_professeur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_matiere_professeur_id_professeur_responsable",
                schema: "public",
                table: "matiere");

            migrationBuilder.DropIndex(
                name: "IX_matiere_id_professeur_responsable",
                schema: "public",
                table: "matiere");

            migrationBuilder.DropColumn(
                name: "id_professeur_responsable",
                schema: "public",
                table: "matiere");

            migrationBuilder.DropColumn(
                name: "volume_horaire",
                schema: "public",
                table: "matiere");
        }
    }
}
