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
            migrationBuilder.Sql(
                "ALTER TABLE public.matiere ADD COLUMN IF NOT EXISTS id_professeur_responsable integer;");

            migrationBuilder.Sql(
                "ALTER TABLE public.matiere ADD COLUMN IF NOT EXISTS volume_horaire integer DEFAULT 0;");

            migrationBuilder.Sql(
                @"DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relkind = 'i' AND c.relname = 'IX_matiere_id_professeur_responsable' AND n.nspname = 'public'
    ) THEN
        CREATE INDEX ""IX_matiere_id_professeur_responsable"" ON public.matiere (id_professeur_responsable);
    END IF;
END$$;"
            );

            migrationBuilder.Sql(
                @"DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_matiere_professeur_id_professeur_responsable'
    ) THEN
        ALTER TABLE public.matiere
        ADD CONSTRAINT ""FK_matiere_professeur_id_professeur_responsable"" FOREIGN KEY (id_professeur_responsable) REFERENCES public.professeur(id_professeur);
    END IF;
END$$;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.matiere DROP CONSTRAINT IF EXISTS \"FK_matiere_professeur_id_professeur_responsable\";");
            migrationBuilder.Sql("DO $$ BEGIN IF EXISTS (SELECT 1 FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace WHERE c.relkind = 'i' AND c.relname = 'IX_matiere_id_professeur_responsable' AND n.nspname = 'public') THEN DROP INDEX public.\"IX_matiere_id_professeur_responsable\"; END IF; END$$;");
            migrationBuilder.Sql("ALTER TABLE public.matiere DROP COLUMN IF EXISTS id_professeur_responsable;" );
            migrationBuilder.Sql("ALTER TABLE public.matiere DROP COLUMN IF EXISTS volume_horaire;" );
        }
    }
}
