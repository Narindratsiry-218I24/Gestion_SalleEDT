-- Migration: AddUtilisateurSecurityFields
-- À appliquer sur la base EMIT_EDT_DB

-- Ajout des colonnes de sécurité sur la table utilisateur
ALTER TABLE public.utilisateur
    ADD COLUMN IF NOT EXISTS password_hash VARCHAR(255),
    ADD COLUMN IF NOT EXISTS token_activation VARCHAR(255),
    ADD COLUMN IF NOT EXISTS token_expiration TIMESTAMP WITH TIME ZONE,
    ADD COLUMN IF NOT EXISTS statut_compte VARCHAR(30) NOT NULL DEFAULT 'EnAttente',
    ADD COLUMN IF NOT EXISTS date_creation TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    ADD COLUMN IF NOT EXISTS date_derniere_connexion TIMESTAMP WITH TIME ZONE,
    ADD COLUMN IF NOT EXISTS premier_login BOOLEAN NOT NULL DEFAULT TRUE;

-- Mettre les comptes existants (admin, etc.) à "Actif" pour ne pas bloquer la connexion
UPDATE public.utilisateur
SET statut_compte = 'Actif', premier_login = FALSE
WHERE statut_compte = 'EnAttente';

-- Vérification
SELECT id_utilisateur, email, role, statut_compte, premier_login FROM public.utilisateur;
