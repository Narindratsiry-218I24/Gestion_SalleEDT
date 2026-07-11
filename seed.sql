-- 1. Seed MENTIONS
INSERT INTO public.mention (id_mention, code_mention, nom_mention) VALUES
(1, 'INFO', 'INFORMATIQUE'),
(2, 'MANAGEMENT', 'MANAGEMENT'),
(3, 'MULTI-MEDIA', 'MULTI-MEDIA')
ON CONFLICT (id_mention) DO UPDATE SET code_mention = EXCLUDED.code_mention, nom_mention = EXCLUDED.nom_mention;

-- 2. Seed NIVEAUX
INSERT INTO public.niveau (id_niveau, code_niveau, ordre, id_mention) VALUES
-- Licence Info
(1, 'L1', 1, 1), (2, 'L2', 2, 1), (3, 'L3', 3, 1),
-- Master Info
(4, 'M1', 4, 1), (5, 'M2', 5, 1),
-- Licence Management
(6, 'L1', 1, 2), (7, 'L2', 2, 2), (8, 'L3', 3, 2),
-- Master Management
(9, 'M1', 4, 2), (10, 'M2', 5, 2),
-- Licence Multi-Media
(11, 'L1', 1, 3), (12, 'L2', 2, 3), (13, 'L3', 3, 3),
-- Master Multi-Media
(14, 'M1', 4, 3), (15, 'M2', 5, 3)
ON CONFLICT (id_niveau) DO UPDATE SET code_niveau = EXCLUDED.code_niveau, id_mention = EXCLUDED.id_mention;

-- 3. Seed FILIERES (Parcours)
INSERT INTO public.filiere (id_filiere, id_mention, code_filiere, nom_filiere) VALUES
-- Info L
(1, 1, 'DA2I', 'DA2I'),
-- Manag L
(2, 2, 'AES', 'AES'),
-- Multi-Media L
(3, 3, 'ICM', 'ICM'),
-- Info M
(4, 1, 'M2I', 'M2I'),
(5, 1, 'SDIA', 'SDIA'),
(6, 1, 'SIGD', 'SIGD'),
-- Manag M
(7, 2, 'AEE', 'AEE'),
(8, 2, 'ASE', 'ASE'),
(9, 2, 'RPO', 'RPO'),
(10, 2, 'RPCO', 'RPCO'),
(11, 2, 'PCO', 'PCO')
ON CONFLICT (id_filiere) DO UPDATE SET code_filiere = EXCLUDED.code_filiere, nom_filiere = EXCLUDED.nom_filiere, id_mention = EXCLUDED.id_mention;

-- 4. Seed SALLES
INSERT INTO public.salle (id_salle, code_batiment, etage, numero_porte, nom_salle, capacite, type_salle) VALUES
(1, 'A', 0, '001', 'A001', 10, 'administration'),
(2, 'B', 0, '001', 'B001', 50, 'cours'),
(3, 'B', 0, '002', 'B002', 50, 'cours'),
(4, 'B', 0, '003', 'B003', 50, 'cours'),
(5, 'B', 1, '101', 'B101', 50, 'cours'),
(6, 'B', 1, '102', 'B102', 50, 'cours'),
(7, 'B', 1, '103', 'B103', 50, 'cours'),
(8, 'B', 2, '201', 'B201', 50, 'cours'),
(9, 'B', 2, '202', 'B202', 50, 'cours'),
(10, 'B', 2, '203', 'B203', 50, 'cours'),
(11, 'B', 3, '301', 'B301', 50, 'cours'),
(12, 'B', 3, '302', 'B302', 50, 'cours'),
(13, 'B', 3, '303', 'B303', 50, 'cours'),
(14, 'C', 0, '001', 'C001', 50, 'cours'),
(15, 'C', 0, '002', 'C002 Bibliotheque', 60, 'bibliotheque'),
(16, 'C', 0, '003', 'C003', 50, 'cours'),
(17, 'D', 0, '001', 'D001', 200, 'amphi'),
(18, 'D', 1, '002', 'D002', 200, 'amphi')
ON CONFLICT (id_salle) DO UPDATE SET nom_salle = EXCLUDED.nom_salle, type_salle = EXCLUDED.type_salle;

-- 5. Seed REF_SEMESTRE (Pour chaque niveau)
INSERT INTO public.ref_semestre (id_ref_semestre, code_semestre, ordre, id_niveau) VALUES
(1, 'S1', 1, 1), (2, 'S2', 2, 1),
(3, 'S3', 3, 2), (4, 'S4', 4, 2),
(5, 'S5', 5, 3), (6, 'S6', 6, 3),
(7, 'S7', 7, 4), (8, 'S8', 8, 4),
(9, 'S9', 9, 5), (10, 'S10', 10, 5),

(11, 'S1', 1, 6), (12, 'S2', 2, 6),
(13, 'S3', 3, 7), (14, 'S4', 4, 7),
(15, 'S5', 5, 8), (16, 'S6', 6, 8),
(17, 'S7', 7, 9), (18, 'S8', 8, 9),
(19, 'S9', 9, 10), (20, 'S10', 10, 10),

(21, 'S1', 1, 11), (22, 'S2', 2, 11),
(23, 'S3', 3, 12), (24, 'S4', 4, 12),
(25, 'S5', 5, 13), (26, 'S6', 6, 13),
(27, 'S7', 7, 14), (28, 'S8', 8, 14),
(29, 'S9', 9, 15), (30, 'S10', 10, 15)
ON CONFLICT (id_ref_semestre) DO NOTHING;

-- 6. Seed ANNEE_ACADEMIQUE
INSERT INTO public.annee_academique (id_annee, libelle, date_debut_annee, date_fin_annee) VALUES
(1, '2025-2026', '2025-10-01', '2026-07-31')
ON CONFLICT (id_annee) DO NOTHING;

-- 7. Seed SEMESTRE (Année 2025-2026)
INSERT INTO public.semestre (id_semestre, id_ref_semestre, id_annee, date_debut, date_fin) VALUES
(1, 1, 1, '2025-10-01', '2026-02-28'), (2, 2, 1, '2026-03-01', '2026-07-31'),
(3, 3, 1, '2025-10-01', '2026-02-28'), (4, 4, 1, '2026-03-01', '2026-07-31'),
(5, 5, 1, '2025-10-01', '2026-02-28'), (6, 6, 1, '2026-03-01', '2026-07-31'),
(7, 7, 1, '2025-10-01', '2026-02-28'), (8, 8, 1, '2026-03-01', '2026-07-31'),
(9, 9, 1, '2025-10-01', '2026-02-28'), (10, 10, 1, '2026-03-01', '2026-07-31')
ON CONFLICT (id_semestre) DO NOTHING;

-- 8. Seed CLASSE
INSERT INTO public.classe (id_classe, id_filiere, id_annee_academique, nom_classe, id_niveau) VALUES
-- Info
(1, 1, 1, 'L1 DA2I S1', 1),
(2, 4, 1, 'M1 M2I S7', 4),
(3, 5, 1, 'M1 SDIA S7', 4)
ON CONFLICT (id_classe) DO NOTHING;

-- 9. Seed PROFESSEUR
INSERT INTO public.professeur (id_professeur, matricule, nom, prenom, email, telephone) VALUES
(1, 'PROF-0001', 'Randriana', 'Aristhène', 'a.randriana@emit.edu', '+261 34 00 000 01'),
(2, 'PROF-0002', 'Rakoto', 'Jean', 'j.rakoto@emit.edu', '+261 34 00 000 02')
ON CONFLICT (id_professeur) DO UPDATE SET matricule = EXCLUDED.matricule;

-- 10. Seed UTILISATEUR
INSERT INTO public.utilisateur (id_utilisateur, nom, prenom, email, role) VALUES
(1, 'Admin', 'EMIT', 'admin@emit.mg', 'admin')
ON CONFLICT (id_utilisateur) DO NOTHING;

ALTER TABLE matiere ADD COLUMN IF NOT EXISTS volume_horaire INT NOT NULL DEFAULT 0;
ALTER TABLE matiere ADD COLUMN IF NOT EXISTS id_professeur_responsable INT REFERENCES professeur(id_professeur);

-- 9. Nettoyer les anciennes classes
TRUNCATE public.classe RESTART IDENTITY CASCADE;

-- 10. Cr�er les classes pour l'ann�e 2025-2026
-- Ces classes seront g�n�r�es automatiquement par le syst�me
-- Mais on peut en ajouter manuellement si besoin

-- 11. Mettre � jour les s�quences
SELECT setval('public.classe_id_classe_seq', (SELECT COALESCE(MAX(id_classe), 0) + 1 FROM public.classe), false);

