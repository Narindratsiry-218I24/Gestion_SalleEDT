using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class ClasseAnnuelleAffectationMatiere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.classe DROP CONSTRAINT IF EXISTS \"FK_classe_semestre_id_semestre\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS public.\"IX_classe_id_semestre\";");

            migrationBuilder.RenameColumn(
                name: "id_semestre",
                schema: "public",
                table: "classe",
                newName: "id_annee_academique");

            migrationBuilder.Sql("""
                UPDATE public.classe AS c
                SET id_annee_academique = s.id_annee
                FROM public.semestre AS s
                WHERE c.id_annee_academique = s.id_semestre;
                """);

            migrationBuilder.AddColumn<int>(
                name: "id_affectation",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code_classe",
                schema: "public",
                table: "classe",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE public.classe
                SET code_classe = LEFT(nom_classe, 20)
                WHERE code_classe IS NULL;
                """);

            migrationBuilder.CreateTable(
                name: "affectation_matiere",
                schema: "public",
                columns: table => new
                {
                    id_affectation = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_classe = table.Column<int>(type: "integer", nullable: false),
                    id_matiere = table.Column<int>(type: "integer", nullable: false),
                    id_professeur = table.Column<int>(type: "integer", nullable: false),
                    id_semestre = table.Column<int>(type: "integer", nullable: false),
                    volume_horaire_total = table.Column<int>(type: "integer", nullable: false),
                    heures_cm = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    heures_td = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    heures_tp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    est_actif = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    date_debut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    commentaire = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_affectation_matiere", x => x.id_affectation);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_classe_id_classe",
                        column: x => x.id_classe,
                        principalSchema: "public",
                        principalTable: "classe",
                        principalColumn: "id_classe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_matiere_id_matiere",
                        column: x => x.id_matiere,
                        principalSchema: "public",
                        principalTable: "matiere",
                        principalColumn: "id_matiere",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_professeur_id_professeur",
                        column: x => x.id_professeur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_semestre_id_semestre",
                        column: x => x.id_semestre,
                        principalSchema: "public",
                        principalTable: "semestre",
                        principalColumn: "id_semestre",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_affectation",
                schema: "public",
                table: "cours",
                column: "id_affectation");

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_annee_academique_code_classe",
                schema: "public",
                table: "classe",
                columns: new[] { "id_annee_academique", "code_classe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_affectation_matiere_id_classe_id_matiere_id_professeur_id_s~",
                schema: "public",
                table: "affectation_matiere",
                columns: new[] { "id_classe", "id_matiere", "id_professeur", "id_semestre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_affectation_matiere_id_matiere",
                schema: "public",
                table: "affectation_matiere",
                column: "id_matiere");

            migrationBuilder.CreateIndex(
                name: "IX_affectation_matiere_id_professeur",
                schema: "public",
                table: "affectation_matiere",
                column: "id_professeur");

            migrationBuilder.CreateIndex(
                name: "IX_affectation_matiere_id_semestre",
                schema: "public",
                table: "affectation_matiere",
                column: "id_semestre");

            migrationBuilder.AddForeignKey(
                name: "FK_classe_annee_academique_id_annee_academique",
                schema: "public",
                table: "classe",
                column: "id_annee_academique",
                principalSchema: "public",
                principalTable: "annee_academique",
                principalColumn: "id_annee",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cours_affectation_matiere_id_affectation",
                schema: "public",
                table: "cours",
                column: "id_affectation",
                principalSchema: "public",
                principalTable: "affectation_matiere",
                principalColumn: "id_affectation",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_classe_annee_academique_id_annee_academique",
                schema: "public",
                table: "classe");

            migrationBuilder.DropForeignKey(
                name: "FK_cours_affectation_matiere_id_affectation",
                schema: "public",
                table: "cours");

            migrationBuilder.DropTable(
                name: "affectation_matiere",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_cours_id_affectation",
                schema: "public",
                table: "cours");

            migrationBuilder.DropIndex(
                name: "IX_classe_id_annee_academique_code_classe",
                schema: "public",
                table: "classe");

            migrationBuilder.DropColumn(
                name: "id_affectation",
                schema: "public",
                table: "cours");

            migrationBuilder.DropColumn(
                name: "code_classe",
                schema: "public",
                table: "classe");

            migrationBuilder.RenameColumn(
                name: "id_annee_academique",
                schema: "public",
                table: "classe",
                newName: "id_semestre");

            migrationBuilder.Sql("""
                UPDATE public.classe AS c
                SET id_semestre = s.id_semestre
                FROM public.semestre AS s
                WHERE c.id_semestre = s.id_annee
                  AND s.id_semestre = (
                      SELECT MIN(s2.id_semestre)
                      FROM public.semestre AS s2
                      WHERE s2.id_annee = s.id_annee
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_semestre",
                schema: "public",
                table: "classe",
                column: "id_semestre");

            migrationBuilder.AddForeignKey(
                name: "FK_classe_semestre_id_semestre",
                schema: "public",
                table: "classe",
                column: "id_semestre",
                principalSchema: "public",
                principalTable: "semestre",
                principalColumn: "id_semestre",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
