using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "public",
                columns: table => new
                {
                    id_audit = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<int>(type: "integer", nullable: true),
                    operation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id_audit);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "public",
                columns: table => new
                {
                    id_subject = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    hours = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_niveau = table.Column<int>(type: "integer", nullable: true),
                    id_mention = table.Column<int>(type: "integer", nullable: true),
                    id_filiere = table.Column<int>(type: "integer", nullable: true),
                    id_semestre = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.id_subject);
                    table.ForeignKey(
                        name: "FK_subjects_filiere_id_filiere",
                        column: x => x.id_filiere,
                        principalSchema: "public",
                        principalTable: "filiere",
                        principalColumn: "id_filiere");
                    table.ForeignKey(
                        name: "FK_subjects_mention_id_mention",
                        column: x => x.id_mention,
                        principalSchema: "public",
                        principalTable: "mention",
                        principalColumn: "id_mention");
                    table.ForeignKey(
                        name: "FK_subjects_niveau_id_niveau",
                        column: x => x.id_niveau,
                        principalSchema: "public",
                        principalTable: "niveau",
                        principalColumn: "id_niveau");
                    table.ForeignKey(
                        name: "FK_subjects_semestre_id_semestre",
                        column: x => x.id_semestre,
                        principalSchema: "public",
                        principalTable: "semestre",
                        principalColumn: "id_semestre");
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                schema: "public",
                columns: table => new
                {
                    id_schedule = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_subject = table.Column<int>(type: "integer", nullable: true),
                    id_professeur = table.Column<int>(type: "integer", nullable: true),
                    id_salle = table.Column<int>(type: "integer", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    heure_debut = table.Column<TimeSpan>(type: "interval", nullable: false),
                    heure_fin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    session_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.id_schedule);
                    table.ForeignKey(
                        name: "FK_schedules_professeur_id_professeur",
                        column: x => x.id_professeur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur");
                    table.ForeignKey(
                        name: "FK_schedules_salle_id_salle",
                        column: x => x.id_salle,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle");
                    table.ForeignKey(
                        name: "FK_schedules_subjects_id_subject",
                        column: x => x.id_subject,
                        principalSchema: "public",
                        principalTable: "subjects",
                        principalColumn: "id_subject");
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedules_id_professeur",
                schema: "public",
                table: "schedules",
                column: "id_professeur");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_id_salle",
                schema: "public",
                table: "schedules",
                column: "id_salle");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_id_subject",
                schema: "public",
                table: "schedules",
                column: "id_subject");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_code",
                schema: "public",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subjects_id_filiere",
                schema: "public",
                table: "subjects",
                column: "id_filiere");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_id_mention",
                schema: "public",
                table: "subjects",
                column: "id_mention");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_id_niveau",
                schema: "public",
                table: "subjects",
                column: "id_niveau");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_id_semestre",
                schema: "public",
                table: "subjects",
                column: "id_semestre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "public");
        }
    }
}
