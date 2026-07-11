using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdSalleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.cours DROP CONSTRAINT IF EXISTS \"FK_cours_salle_id_salle\";");

            migrationBuilder.AlterColumn<int>(
                name: "id_salle",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_cours_salle_id_salle",
                schema: "public",
                table: "cours",
                column: "id_salle",
                principalSchema: "public",
                principalTable: "salle",
                principalColumn: "id_salle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.cours DROP CONSTRAINT IF EXISTS \"FK_cours_salle_id_salle\";");

            migrationBuilder.AlterColumn<int>(
                name: "id_salle",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cours_salle_id_salle",
                schema: "public",
                table: "cours",
                column: "id_salle",
                principalSchema: "public",
                principalTable: "salle",
                principalColumn: "id_salle",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
