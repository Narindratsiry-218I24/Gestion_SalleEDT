using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class AddNiveauToClasse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_niveau",
                schema: "public",
                table: "classe",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_niveau",
                schema: "public",
                table: "classe",
                column: "id_niveau");

            migrationBuilder.AddForeignKey(
                name: "FK_classe_niveau_id_niveau",
                schema: "public",
                table: "classe",
                column: "id_niveau",
                principalSchema: "public",
                principalTable: "niveau",
                principalColumn: "id_niveau",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classe_niveau_id_niveau",
                schema: "public",
                table: "classe");

            migrationBuilder.DropIndex(
                name: "IX_classe_id_niveau",
                schema: "public",
                table: "classe");

            migrationBuilder.DropColumn(
                name: "id_niveau",
                schema: "public",
                table: "classe");
        }
    }
}
