using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    public partial class UpdateCoursSeanceFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cours_classe_id_classe",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_cours_professeur_id_professeur",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_cours_semestre_id_semestre",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_professeur_id_professeur",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_salle_id_salle",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_subjects_id_subject",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_filiere_id_filiere",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_mention_id_mention",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_niveau_id_niveau",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_semestre_id_semestre",
                schema: "public",
                table: "subjects");

            migrationBuilder.AlterColumn<int>(
                name: "id_semestre",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_niveau",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_mention",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_filiere",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_subject",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_salle",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_professeur",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type_cours",
                schema: "public",
                table: "cours",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "statut",
                schema: "public",
                table: "cours",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "id_semestre",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_professeur",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_classe",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "teaching_mode",
                schema: "public",
                table: "cours",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "volume_hours",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "groupes",
                schema: "public",
                columns: table => new
                {
                    id_groupe = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_classe = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groupes", x => x.id_groupe);
                    table.ForeignKey(
                        name: "FK_groupes_classe_id_classe",
                        column: x => x.id_classe,
                        principalSchema: "public",
                        principalTable: "classe",
                        principalColumn: "id_classe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seances",
                schema: "public",
                columns: table => new
                {
                    id_seance = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cours = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    id_salle = table.Column<int>(type: "integer", nullable: false),
                    id_groupe = table.Column<int>(type: "integer", nullable: false),
                    realized_hours = table.Column<int>(type: "integer", nullable: false),
                    attendance = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seances", x => x.id_seance);
                    table.ForeignKey(
                        name: "FK_seances_cours_id_cours",
                        column: x => x.id_cours,
                        principalSchema: "public",
                        principalTable: "cours",
                        principalColumn: "id_cours",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seances_groupes_id_groupe",
                        column: x => x.id_groupe,
                        principalSchema: "public",
                        principalTable: "groupes",
                        principalColumn: "id_groupe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seances_salle_id_salle",
                        column: x => x.id_salle,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_groupes_id_classe",
                schema: "public",
                table: "groupes",
                column: "id_classe");

            migrationBuilder.CreateIndex(
                name: "IX_seances_id_cours",
                schema: "public",
                table: "seances",
                column: "id_cours");

            migrationBuilder.CreateIndex(
                name: "IX_seances_id_groupe",
                schema: "public",
                table: "seances",
                column: "id_groupe");

            migrationBuilder.CreateIndex(
                name: "IX_seances_id_salle",
                schema: "public",
                table: "seances",
                column: "id_salle");

            migrationBuilder.AddForeignKey(
                name: "FK_cours_classe_id_classe",
                schema: "public",
                table: "cours",
                column: "id_classe",
                principalSchema: "public",
                principalTable: "classe",
                principalColumn: "id_classe");

            migrationBuilder.AddForeignKey(
                name: "FK_cours_professeur_id_professeur",
                schema: "public",
                table: "cours",
                column: "id_professeur",
                principalSchema: "public",
                principalTable: "professeur",
                principalColumn: "id_professeur");

            migrationBuilder.AddForeignKey(
                name: "FK_cours_semestre_id_semestre",
                schema: "public",
                table: "cours",
                column: "id_semestre",
                principalSchema: "public",
                principalTable: "semestre",
                principalColumn: "id_semestre");

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_professeur_id_professeur",
                schema: "public",
                table: "schedules",
                column: "id_professeur",
                principalSchema: "public",
                principalTable: "professeur",
                principalColumn: "id_professeur",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_salle_id_salle",
                schema: "public",
                table: "schedules",
                column: "id_salle",
                principalSchema: "public",
                principalTable: "salle",
                principalColumn: "id_salle",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_subjects_id_subject",
                schema: "public",
                table: "schedules",
                column: "id_subject",
                principalSchema: "public",
                principalTable: "subjects",
                principalColumn: "id_subject",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_filiere_id_filiere",
                schema: "public",
                table: "subjects",
                column: "id_filiere",
                principalSchema: "public",
                principalTable: "filiere",
                principalColumn: "id_filiere",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_mention_id_mention",
                schema: "public",
                table: "subjects",
                column: "id_mention",
                principalSchema: "public",
                principalTable: "mention",
                principalColumn: "id_mention",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_niveau_id_niveau",
                schema: "public",
                table: "subjects",
                column: "id_niveau",
                principalSchema: "public",
                principalTable: "niveau",
                principalColumn: "id_niveau",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_semestre_id_semestre",
                schema: "public",
                table: "subjects",
                column: "id_semestre",
                principalSchema: "public",
                principalTable: "semestre",
                principalColumn: "id_semestre",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cours_classe_id_classe",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_cours_professeur_id_professeur",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_cours_semestre_id_semestre",
                schema: "public",
                table: "cours");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_professeur_id_professeur",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_salle_id_salle",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_subjects_id_subject",
                schema: "public",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_filiere_id_filiere",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_mention_id_mention",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_niveau_id_niveau",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_semestre_id_semestre",
                schema: "public",
                table: "subjects");

            migrationBuilder.DropTable(
                name: "seances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "groupes",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "capacity",
                schema: "public",
                table: "cours");

            migrationBuilder.DropColumn(
                name: "teaching_mode",
                schema: "public",
                table: "cours");

            migrationBuilder.DropColumn(
                name: "volume_hours",
                schema: "public",
                table: "cours");

            migrationBuilder.AlterColumn<int>(
                name: "id_semestre",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_niveau",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_mention",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_filiere",
                schema: "public",
                table: "subjects",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_subject",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_salle",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_professeur",
                schema: "public",
                table: "schedules",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "type_cours",
                schema: "public",
                table: "cours",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "statut",
                schema: "public",
                table: "cours",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "id_semestre",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_professeur",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_classe",
                schema: "public",
                table: "cours",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cours_classe_id_classe",
                schema: "public",
                table: "cours",
                column: "id_classe",
                principalSchema: "public",
                principalTable: "classe",
                principalColumn: "id_classe",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cours_professeur_id_professeur",
                schema: "public",
                table: "cours",
                column: "id_professeur",
                principalSchema: "public",
                principalTable: "professeur",
                principalColumn: "id_professeur",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_cours_semestre_id_semestre",
                schema: "public",
                table: "cours",
                column: "id_semestre",
                principalSchema: "public",
                principalTable: "semestre",
                principalColumn: "id_semestre",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_professeur_id_professeur",
                schema: "public",
                table: "schedules",
                column: "id_professeur",
                principalSchema: "public",
                principalTable: "professeur",
                principalColumn: "id_professeur");

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_salle_id_salle",
                schema: "public",
                table: "schedules",
                column: "id_salle",
                principalSchema: "public",
                principalTable: "salle",
                principalColumn: "id_salle");

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_subjects_id_subject",
                schema: "public",
                table: "schedules",
                column: "id_subject",
                principalSchema: "public",
                principalTable: "subjects",
                principalColumn: "id_subject");

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_filiere_id_filiere",
                schema: "public",
                table: "subjects",
                column: "id_filiere",
                principalSchema: "public",
                principalTable: "filiere",
                principalColumn: "id_filiere");

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_mention_id_mention",
                schema: "public",
                table: "subjects",
                column: "id_mention",
                principalSchema: "public",
                principalTable: "mention",
                principalColumn: "id_mention");

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_niveau_id_niveau",
                schema: "public",
                table: "subjects",
                column: "id_niveau",
                principalSchema: "public",
                principalTable: "niveau",
                principalColumn: "id_niveau");

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_semestre_id_semestre",
                schema: "public",
                table: "subjects",
                column: "id_semestre",
                principalSchema: "public",
                principalTable: "semestre",
                principalColumn: "id_semestre");
        }
    }
}
