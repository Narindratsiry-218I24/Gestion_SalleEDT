using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gestion_SalleClasseEDT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "annee_academique",
                schema: "public",
                columns: table => new
                {
                    id_annee = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    libelle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date_debut_annee = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_fin_annee = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    est_archivee = table.Column<bool>(type: "boolean", nullable: false),
                    date_archivage = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    est_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annee_academique", x => x.id_annee);
                });

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
                name: "mention",
                schema: "public",
                columns: table => new
                {
                    id_mention = table.Column<int>(type: "integer", nullable: false),
                    code_mention = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom_mention = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mention", x => x.id_mention);
                });

            migrationBuilder.CreateTable(
                name: "salle",
                schema: "public",
                columns: table => new
                {
                    id_salle = table.Column<int>(type: "integer", nullable: false),
                    code_batiment = table.Column<string>(type: "bpchar", maxLength: 1, nullable: false),
                    etage = table.Column<int>(type: "integer", nullable: false),
                    numero_porte = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nom_salle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacite = table.Column<int>(type: "integer", nullable: false),
                    type_salle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salle", x => x.id_salle);
                });

            migrationBuilder.CreateTable(
                name: "utilisateur",
                schema: "public",
                columns: table => new
                {
                    id_utilisateur = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    token_activation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    token_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    statut_compte = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_derniere_connexion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    premier_login = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilisateur", x => x.id_utilisateur);
                });

            migrationBuilder.CreateTable(
                name: "filiere",
                schema: "public",
                columns: table => new
                {
                    id_filiere = table.Column<int>(type: "integer", nullable: false),
                    id_mention = table.Column<int>(type: "integer", nullable: false),
                    code_filiere = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom_filiere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filiere", x => x.id_filiere);
                    table.ForeignKey(
                        name: "FK_filiere_mention_id_mention",
                        column: x => x.id_mention,
                        principalSchema: "public",
                        principalTable: "mention",
                        principalColumn: "id_mention",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "niveau",
                schema: "public",
                columns: table => new
                {
                    id_niveau = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code_niveau = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ordre = table.Column<int>(type: "integer", nullable: false),
                    id_mention = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveau", x => x.id_niveau);
                    table.ForeignKey(
                        name: "FK_niveau_mention_id_mention",
                        column: x => x.id_mention,
                        principalSchema: "public",
                        principalTable: "mention",
                        principalColumn: "id_mention",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "professeur",
                schema: "public",
                columns: table => new
                {
                    id_professeur = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    matricule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titre = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    telephone_portable = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    specialite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    specialites_secondaires = table.Column<string>(type: "jsonb", nullable: false),
                    date_embauche = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    biographie = table.Column<string>(type: "text", nullable: false),
                    cv_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_utilisateur = table.Column<int>(type: "integer", nullable: true),
                    id_annee_academique = table.Column<int>(type: "integer", nullable: true),
                    capacite_horaire_max = table.Column<int>(type: "integer", nullable: false),
                    est_actif = table.Column<bool>(type: "boolean", nullable: false),
                    date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_modification = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    heures_effectuees = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professeur", x => x.id_professeur);
                    table.ForeignKey(
                        name: "FK_professeur_annee_academique_id_annee_academique",
                        column: x => x.id_annee_academique,
                        principalSchema: "public",
                        principalTable: "annee_academique",
                        principalColumn: "id_annee");
                    table.ForeignKey(
                        name: "FK_professeur_utilisateur_id_utilisateur",
                        column: x => x.id_utilisateur,
                        principalSchema: "public",
                        principalTable: "utilisateur",
                        principalColumn: "id_utilisateur");
                });

            migrationBuilder.CreateTable(
                name: "classe",
                schema: "public",
                columns: table => new
                {
                    id_classe = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_filiere = table.Column<int>(type: "integer", nullable: false),
                    id_niveau = table.Column<int>(type: "integer", nullable: false),
                    id_annee_academique = table.Column<int>(type: "integer", nullable: false),
                    nom_classe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code_classe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    est_archivee = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classe", x => x.id_classe);
                    table.ForeignKey(
                        name: "FK_classe_annee_academique_id_annee_academique",
                        column: x => x.id_annee_academique,
                        principalSchema: "public",
                        principalTable: "annee_academique",
                        principalColumn: "id_annee",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_classe_filiere_id_filiere",
                        column: x => x.id_filiere,
                        principalSchema: "public",
                        principalTable: "filiere",
                        principalColumn: "id_filiere",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_classe_niveau_id_niveau",
                        column: x => x.id_niveau,
                        principalSchema: "public",
                        principalTable: "niveau",
                        principalColumn: "id_niveau",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ref_semestre",
                schema: "public",
                columns: table => new
                {
                    id_ref_semestre = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code_semestre = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ordre = table.Column<int>(type: "integer", nullable: false),
                    id_niveau = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_semestre", x => x.id_ref_semestre);
                    table.ForeignKey(
                        name: "FK_ref_semestre_niveau_id_niveau",
                        column: x => x.id_niveau,
                        principalSchema: "public",
                        principalTable: "niveau",
                        principalColumn: "id_niveau",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "disponibilite_prof",
                schema: "public",
                columns: table => new
                {
                    id_dispo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_professeur = table.Column<int>(type: "integer", nullable: false),
                    jour_semaine = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    heure_debut = table.Column<TimeSpan>(type: "interval", nullable: false),
                    heure_fin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    semaine_type = table.Column<string>(type: "bpchar", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disponibilite_prof", x => x.id_dispo);
                    table.ForeignKey(
                        name: "FK_disponibilite_prof_professeur_id_professeur",
                        column: x => x.id_professeur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "public",
                columns: table => new
                {
                    IdNotification = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProfesseur = table.Column<int>(type: "integer", nullable: false),
                    Titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Lien = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstLue = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.IdNotification);
                    table.ForeignKey(
                        name: "FK_Notifications_professeur_IdProfesseur",
                        column: x => x.IdProfesseur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "matiere",
                schema: "public",
                columns: table => new
                {
                    id_matiere = table.Column<int>(type: "integer", nullable: false),
                    id_filiere = table.Column<int>(type: "integer", nullable: false),
                    code_matiere = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom_matiere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_ref_semestre = table.Column<int>(type: "integer", nullable: false),
                    credit = table.Column<int>(type: "integer", nullable: false),
                    volume_horaire = table.Column<int>(type: "integer", nullable: false),
                    id_professeur_responsable = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matiere", x => x.id_matiere);
                    table.ForeignKey(
                        name: "FK_matiere_filiere_id_filiere",
                        column: x => x.id_filiere,
                        principalSchema: "public",
                        principalTable: "filiere",
                        principalColumn: "id_filiere",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matiere_professeur_id_professeur_responsable",
                        column: x => x.id_professeur_responsable,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur");
                    table.ForeignKey(
                        name: "FK_matiere_ref_semestre_id_ref_semestre",
                        column: x => x.id_ref_semestre,
                        principalSchema: "public",
                        principalTable: "ref_semestre",
                        principalColumn: "id_ref_semestre",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "semestre",
                schema: "public",
                columns: table => new
                {
                    id_semestre = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_ref_semestre = table.Column<int>(type: "integer", nullable: false),
                    id_annee = table.Column<int>(type: "integer", nullable: false),
                    date_debut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    est_archivee = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semestre", x => x.id_semestre);
                    table.ForeignKey(
                        name: "FK_semestre_annee_academique_id_annee",
                        column: x => x.id_annee,
                        principalSchema: "public",
                        principalTable: "annee_academique",
                        principalColumn: "id_annee",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_semestre_ref_semestre_id_ref_semestre",
                        column: x => x.id_ref_semestre,
                        principalSchema: "public",
                        principalTable: "ref_semestre",
                        principalColumn: "id_ref_semestre",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prerequisites",
                schema: "public",
                columns: table => new
                {
                    id_prerequisite = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_matiere = table.Column<int>(type: "integer", nullable: false),
                    id_prerequisite_matiere = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prerequisites", x => x.id_prerequisite);
                    table.ForeignKey(
                        name: "FK_prerequisites_matiere_id_matiere",
                        column: x => x.id_matiere,
                        principalSchema: "public",
                        principalTable: "matiere",
                        principalColumn: "id_matiere",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prerequisites_matiere_id_prerequisite_matiere",
                        column: x => x.id_prerequisite_matiere,
                        principalSchema: "public",
                        principalTable: "matiere",
                        principalColumn: "id_matiere",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    heures_cm = table.Column<int>(type: "integer", nullable: false),
                    heures_td = table.Column<int>(type: "integer", nullable: false),
                    heures_tp = table.Column<int>(type: "integer", nullable: false),
                    est_actif = table.Column<bool>(type: "boolean", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_professeur_id_professeur",
                        column: x => x.id_professeur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_affectation_matiere_semestre_id_semestre",
                        column: x => x.id_semestre,
                        principalSchema: "public",
                        principalTable: "semestre",
                        principalColumn: "id_semestre",
                        onDelete: ReferentialAction.Cascade);
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
                name: "cours",
                schema: "public",
                columns: table => new
                {
                    id_cours = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_matiere = table.Column<int>(type: "integer", nullable: false),
                    id_professeur = table.Column<int>(type: "integer", nullable: true),
                    id_classe = table.Column<int>(type: "integer", nullable: true),
                    id_salle = table.Column<int>(type: "integer", nullable: true),
                    id_semestre = table.Column<int>(type: "integer", nullable: false),
                    id_affectation = table.Column<int>(type: "integer", nullable: true),
                    volume_hours = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    type_cours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    teaching_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_groupe = table.Column<int>(type: "integer", nullable: true),
                    objectives = table.Column<string>(type: "text", nullable: true),
                    skills = table.Column<string>(type: "text", nullable: true),
                    evaluation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cours", x => x.id_cours);
                    table.ForeignKey(
                        name: "FK_cours_affectation_matiere_id_affectation",
                        column: x => x.id_affectation,
                        principalSchema: "public",
                        principalTable: "affectation_matiere",
                        principalColumn: "id_affectation");
                    table.ForeignKey(
                        name: "FK_cours_classe_id_classe",
                        column: x => x.id_classe,
                        principalSchema: "public",
                        principalTable: "classe",
                        principalColumn: "id_classe");
                    table.ForeignKey(
                        name: "FK_cours_groupes_id_groupe",
                        column: x => x.id_groupe,
                        principalSchema: "public",
                        principalTable: "groupes",
                        principalColumn: "id_groupe");
                    table.ForeignKey(
                        name: "FK_cours_matiere_id_matiere",
                        column: x => x.id_matiere,
                        principalSchema: "public",
                        principalTable: "matiere",
                        principalColumn: "id_matiere",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cours_professeur_id_professeur",
                        column: x => x.id_professeur,
                        principalSchema: "public",
                        principalTable: "professeur",
                        principalColumn: "id_professeur");
                    table.ForeignKey(
                        name: "FK_cours_salle_id_salle",
                        column: x => x.id_salle,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle");
                    table.ForeignKey(
                        name: "FK_cours_semestre_id_semestre",
                        column: x => x.id_semestre,
                        principalSchema: "public",
                        principalTable: "semestre",
                        principalColumn: "id_semestre",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "creneau",
                schema: "public",
                columns: table => new
                {
                    id_creneau = table.Column<int>(type: "integer", nullable: false),
                    id_cours = table.Column<int>(type: "integer", nullable: false),
                    jour_semaine = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    heure_debut = table.Column<TimeSpan>(type: "interval", nullable: false),
                    heure_fin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    semaine_type = table.Column<string>(type: "bpchar", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creneau", x => x.id_creneau);
                    table.ForeignKey(
                        name: "FK_creneau_cours_id_cours",
                        column: x => x.id_cours,
                        principalSchema: "public",
                        principalTable: "cours",
                        principalColumn: "id_cours",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "demande_edt",
                schema: "public",
                columns: table => new
                {
                    id_demande = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_demandeur = table.Column<int>(type: "integer", nullable: false),
                    id_validateur = table.Column<int>(type: "integer", nullable: true),
                    id_cours = table.Column<int>(type: "integer", nullable: true),
                    id_salle = table.Column<int>(type: "integer", nullable: true),
                    id_classe = table.Column<int>(type: "integer", nullable: true),
                    id_matiere = table.Column<int>(type: "integer", nullable: true),
                    id_niveau = table.Column<int>(type: "integer", nullable: true),
                    date_souhaitee = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    heure_debut_souhaitee = table.Column<TimeSpan>(type: "interval", nullable: true),
                    heure_fin_souhaitee = table.Column<TimeSpan>(type: "interval", nullable: true),
                    type_demande = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    justification = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demande_edt", x => x.id_demande);
                    table.ForeignKey(
                        name: "FK_demande_edt_classe_id_classe",
                        column: x => x.id_classe,
                        principalSchema: "public",
                        principalTable: "classe",
                        principalColumn: "id_classe");
                    table.ForeignKey(
                        name: "FK_demande_edt_cours_id_cours",
                        column: x => x.id_cours,
                        principalSchema: "public",
                        principalTable: "cours",
                        principalColumn: "id_cours");
                    table.ForeignKey(
                        name: "FK_demande_edt_matiere_id_matiere",
                        column: x => x.id_matiere,
                        principalSchema: "public",
                        principalTable: "matiere",
                        principalColumn: "id_matiere");
                    table.ForeignKey(
                        name: "FK_demande_edt_niveau_id_niveau",
                        column: x => x.id_niveau,
                        principalSchema: "public",
                        principalTable: "niveau",
                        principalColumn: "id_niveau");
                    table.ForeignKey(
                        name: "FK_demande_edt_salle_id_salle",
                        column: x => x.id_salle,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle");
                    table.ForeignKey(
                        name: "FK_demande_edt_utilisateur_id_demandeur",
                        column: x => x.id_demandeur,
                        principalSchema: "public",
                        principalTable: "utilisateur",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_demande_edt_utilisateur_id_validateur",
                        column: x => x.id_validateur,
                        principalSchema: "public",
                        principalTable: "utilisateur",
                        principalColumn: "id_utilisateur");
                });

            migrationBuilder.CreateTable(
                name: "seances",
                schema: "public",
                columns: table => new
                {
                    id_seance = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cours = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    id_salle = table.Column<int>(type: "integer", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_groupe = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seances", x => x.id_seance);
                    table.ForeignKey(
                        name: "FK_seances_cours_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "public",
                        principalTable: "cours",
                        principalColumn: "id_cours",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seances_salle_id_salle",
                        column: x => x.id_salle,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle");
                });

            migrationBuilder.CreateTable(
                name: "proposition_admin",
                schema: "public",
                columns: table => new
                {
                    id_proposition = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_demande = table.Column<int>(type: "integer", nullable: false),
                    id_salle_proposee = table.Column<int>(type: "integer", nullable: false),
                    date_proposee = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    heure_debut_proposee = table.Column<TimeSpan>(type: "interval", nullable: false),
                    heure_fin_proposee = table.Column<TimeSpan>(type: "interval", nullable: false),
                    est_acceptee = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposition_admin", x => x.id_proposition);
                    table.ForeignKey(
                        name: "FK_proposition_admin_demande_edt_id_demande",
                        column: x => x.id_demande,
                        principalSchema: "public",
                        principalTable: "demande_edt",
                        principalColumn: "id_demande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_proposition_admin_salle_id_salle_proposee",
                        column: x => x.id_salle_proposee,
                        principalSchema: "public",
                        principalTable: "salle",
                        principalColumn: "id_salle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_affectation_matiere_id_classe",
                schema: "public",
                table: "affectation_matiere",
                column: "id_classe");

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

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_annee_academique",
                schema: "public",
                table: "classe",
                column: "id_annee_academique");

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_filiere",
                schema: "public",
                table: "classe",
                column: "id_filiere");

            migrationBuilder.CreateIndex(
                name: "IX_classe_id_niveau",
                schema: "public",
                table: "classe",
                column: "id_niveau");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_affectation",
                schema: "public",
                table: "cours",
                column: "id_affectation");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_classe",
                schema: "public",
                table: "cours",
                column: "id_classe");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_groupe",
                schema: "public",
                table: "cours",
                column: "id_groupe");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_matiere",
                schema: "public",
                table: "cours",
                column: "id_matiere");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_professeur",
                schema: "public",
                table: "cours",
                column: "id_professeur");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_salle",
                schema: "public",
                table: "cours",
                column: "id_salle");

            migrationBuilder.CreateIndex(
                name: "IX_cours_id_semestre",
                schema: "public",
                table: "cours",
                column: "id_semestre");

            migrationBuilder.CreateIndex(
                name: "IX_creneau_id_cours",
                schema: "public",
                table: "creneau",
                column: "id_cours");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_classe",
                schema: "public",
                table: "demande_edt",
                column: "id_classe");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_cours",
                schema: "public",
                table: "demande_edt",
                column: "id_cours");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_demandeur",
                schema: "public",
                table: "demande_edt",
                column: "id_demandeur");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_matiere",
                schema: "public",
                table: "demande_edt",
                column: "id_matiere");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_niveau",
                schema: "public",
                table: "demande_edt",
                column: "id_niveau");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_salle",
                schema: "public",
                table: "demande_edt",
                column: "id_salle");

            migrationBuilder.CreateIndex(
                name: "IX_demande_edt_id_validateur",
                schema: "public",
                table: "demande_edt",
                column: "id_validateur");

            migrationBuilder.CreateIndex(
                name: "IX_disponibilite_prof_id_professeur",
                schema: "public",
                table: "disponibilite_prof",
                column: "id_professeur");

            migrationBuilder.CreateIndex(
                name: "IX_filiere_id_mention",
                schema: "public",
                table: "filiere",
                column: "id_mention");

            migrationBuilder.CreateIndex(
                name: "IX_groupes_id_classe",
                schema: "public",
                table: "groupes",
                column: "id_classe");

            migrationBuilder.CreateIndex(
                name: "IX_matiere_id_filiere",
                schema: "public",
                table: "matiere",
                column: "id_filiere");

            migrationBuilder.CreateIndex(
                name: "IX_matiere_id_professeur_responsable",
                schema: "public",
                table: "matiere",
                column: "id_professeur_responsable");

            migrationBuilder.CreateIndex(
                name: "IX_matiere_id_ref_semestre",
                schema: "public",
                table: "matiere",
                column: "id_ref_semestre");

            migrationBuilder.CreateIndex(
                name: "IX_mention_code_mention",
                schema: "public",
                table: "mention",
                column: "code_mention",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_niveau_id_mention",
                schema: "public",
                table: "niveau",
                column: "id_mention");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdProfesseur",
                schema: "public",
                table: "Notifications",
                column: "IdProfesseur");

            migrationBuilder.CreateIndex(
                name: "IX_prerequisites_id_matiere",
                schema: "public",
                table: "prerequisites",
                column: "id_matiere");

            migrationBuilder.CreateIndex(
                name: "IX_prerequisites_id_prerequisite_matiere",
                schema: "public",
                table: "prerequisites",
                column: "id_prerequisite_matiere");

            migrationBuilder.CreateIndex(
                name: "IX_professeur_email",
                schema: "public",
                table: "professeur",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_annee_academique",
                schema: "public",
                table: "professeur",
                column: "id_annee_academique");

            migrationBuilder.CreateIndex(
                name: "IX_professeur_id_utilisateur",
                schema: "public",
                table: "professeur",
                column: "id_utilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proposition_admin_id_demande",
                schema: "public",
                table: "proposition_admin",
                column: "id_demande");

            migrationBuilder.CreateIndex(
                name: "IX_proposition_admin_id_salle_proposee",
                schema: "public",
                table: "proposition_admin",
                column: "id_salle_proposee");

            migrationBuilder.CreateIndex(
                name: "IX_ref_semestre_id_niveau",
                schema: "public",
                table: "ref_semestre",
                column: "id_niveau");

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
                name: "IX_seances_CourseId",
                schema: "public",
                table: "seances",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_seances_id_salle",
                schema: "public",
                table: "seances",
                column: "id_salle");

            migrationBuilder.CreateIndex(
                name: "IX_semestre_id_annee",
                schema: "public",
                table: "semestre",
                column: "id_annee");

            migrationBuilder.CreateIndex(
                name: "IX_semestre_id_ref_semestre",
                schema: "public",
                table: "semestre",
                column: "id_ref_semestre");

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

            migrationBuilder.CreateIndex(
                name: "IX_utilisateur_email",
                schema: "public",
                table: "utilisateur",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "creneau",
                schema: "public");

            migrationBuilder.DropTable(
                name: "disponibilite_prof",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "prerequisites",
                schema: "public");

            migrationBuilder.DropTable(
                name: "proposition_admin",
                schema: "public");

            migrationBuilder.DropTable(
                name: "schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "seances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "demande_edt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cours",
                schema: "public");

            migrationBuilder.DropTable(
                name: "affectation_matiere",
                schema: "public");

            migrationBuilder.DropTable(
                name: "groupes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "salle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "matiere",
                schema: "public");

            migrationBuilder.DropTable(
                name: "semestre",
                schema: "public");

            migrationBuilder.DropTable(
                name: "classe",
                schema: "public");

            migrationBuilder.DropTable(
                name: "professeur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ref_semestre",
                schema: "public");

            migrationBuilder.DropTable(
                name: "filiere",
                schema: "public");

            migrationBuilder.DropTable(
                name: "annee_academique",
                schema: "public");

            migrationBuilder.DropTable(
                name: "utilisateur",
                schema: "public");

            migrationBuilder.DropTable(
                name: "niveau",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mention",
                schema: "public");
        }
    }
}
