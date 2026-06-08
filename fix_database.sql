-- ============================================================
-- Script de synchronisation de la base EMIT_EDT_DB
-- À exécuter dans pgAdmin ou psql
-- ============================================================

-- 1. Marquer la migration InitialCreate comme déjà appliquée
--    (les tables existent déjà, on évite la recréation)
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260608150516_InitialCreate', '8.0.0')
ON CONFLICT DO NOTHING;

-- 2. Ajouter les nouvelles colonnes à demande_edt si elles n'existent pas
ALTER TABLE public.demande_edt
    ADD COLUMN IF NOT EXISTS id_classe      INTEGER REFERENCES public.classe(id_classe),
    ADD COLUMN IF NOT EXISTS id_matiere     INTEGER REFERENCES public.matiere(id_matiere),
    ADD COLUMN IF NOT EXISTS id_niveau      INTEGER REFERENCES public.niveau(id_niveau),
    ADD COLUMN IF NOT EXISTS date_souhaitee          DATE,
    ADD COLUMN IF NOT EXISTS heure_debut_souhaitee   TIME,
    ADD COLUMN IF NOT EXISTS heure_fin_souhaitee     TIME;

-- 3. Créer la table proposition_admin si elle n'existe pas
CREATE TABLE IF NOT EXISTS public.proposition_admin (
    id_proposition      SERIAL PRIMARY KEY,
    id_demande          INTEGER NOT NULL REFERENCES public.demande_edt(id_demande),
    id_salle_proposee   INTEGER NOT NULL REFERENCES public.salle(id_salle),
    date_proposee       TIMESTAMP WITH TIME ZONE NOT NULL,
    heure_debut_proposee TIME NOT NULL,
    heure_fin_proposee   TIME NOT NULL,
    est_acceptee         BOOLEAN
);

-- 4. Agrandir la colonne statut de demande_edt (15 -> 30 chars pour les nouveaux statuts)
ALTER TABLE public.demande_edt
    ALTER COLUMN statut TYPE VARCHAR(30);

-- Fin du script
SELECT 'Migration manuelle appliquée avec succès !' AS resultat;
