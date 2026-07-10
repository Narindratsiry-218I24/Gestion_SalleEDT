-- ============================================================
-- SCRIPT DE CORRECTION DE SCHÉMA — EMIT EDT
-- À exécuter dans PostgreSQL (psql ou pgAdmin) sur EMIT_EDT_DB
-- ============================================================

-- ─────────────────────────────────────────────────────────────
-- 1. Colonnes manquantes dans la table "professeur"
-- ─────────────────────────────────────────────────────────────
ALTER TABLE public.professeur
    ADD COLUMN IF NOT EXISTS departement       VARCHAR(100),
    ADD COLUMN IF NOT EXISTS titre             VARCHAR(10),
    ADD COLUMN IF NOT EXISTS telephone_portable VARCHAR(20),
    ADD COLUMN IF NOT EXISTS biographie        TEXT,
    ADD COLUMN IF NOT EXISTS specialites_secondaires JSONB DEFAULT '[]'::jsonb,
    ADD COLUMN IF NOT EXISTS capacite_horaire_max INTEGER NOT NULL DEFAULT 20,
    ADD COLUMN IF NOT EXISTS heures_effectuees INTEGER NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS est_actif         BOOLEAN NOT NULL DEFAULT TRUE,
    ADD COLUMN IF NOT EXISTS date_modification TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS statut            VARCHAR(50) DEFAULT 'Actif',
    ADD COLUMN IF NOT EXISTS id_utilisateur    INTEGER REFERENCES public.utilisateur(id_utilisateur);

-- ─────────────────────────────────────────────────────────────
-- 2. Colonnes manquantes dans la table "utilisateur"
-- ─────────────────────────────────────────────────────────────
ALTER TABLE public.utilisateur
    ADD COLUMN IF NOT EXISTS password_hash      VARCHAR(255),
    ADD COLUMN IF NOT EXISTS token_activation   VARCHAR(255),
    ADD COLUMN IF NOT EXISTS token_expiration   TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS statut_compte      VARCHAR(30) NOT NULL DEFAULT 'EnAttente',
    ADD COLUMN IF NOT EXISTS date_creation      TIMESTAMPTZ NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS date_derniere_connexion TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS premier_login      BOOLEAN NOT NULL DEFAULT TRUE;

-- ─────────────────────────────────────────────────────────────
-- 3. Table "seances" (si elle n'existe pas)
-- ─────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.seances (
    id_seance       SERIAL PRIMARY KEY,
    id_cours        INTEGER NOT NULL REFERENCES public.cours(id_cours) ON DELETE CASCADE,
    date            DATE NOT NULL,
    start_time      INTERVAL NOT NULL,
    end_time        INTERVAL NOT NULL,
    id_salle        INTEGER REFERENCES public.salle(id_salle),
    id_groupe       INTEGER,
    realized_hours  INTEGER NOT NULL DEFAULT 0,
    attendance      DOUBLE PRECISION
);

-- ─────────────────────────────────────────────────────────────
-- 4. Réinitialiser les séquences pour éviter les doublons de clé
-- ─────────────────────────────────────────────────────────────
SELECT setval(
    pg_get_serial_sequence('public.utilisateur', 'id_utilisateur'),
    COALESCE(MAX(id_utilisateur), 0) + 1, FALSE
) FROM public.utilisateur;

SELECT setval(
    pg_get_serial_sequence('public.professeur', 'id_professeur'),
    COALESCE(MAX(id_professeur), 0) + 1, FALSE
) FROM public.professeur;

SELECT setval(
    pg_get_serial_sequence('public.seances', 'id_seance'),
    COALESCE(MAX(id_seance), 0) + 1, FALSE
) FROM public.seances;

-- ─────────────────────────────────────────────────────────────
-- 5. Mettre à jour les professeurs existants sans statut_compte
-- ─────────────────────────────────────────────────────────────
UPDATE public.utilisateur
SET statut_compte = 'Actif', premier_login = FALSE
WHERE statut_compte IS NULL OR statut_compte = '';

SELECT 'Schéma mis à jour avec succès.' AS statut;
