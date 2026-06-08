-- 1. Seed MENTIONS
INSERT INTO public.mention (id_mention, code_mention, nom_mention) VALUES
(1, 'INFO', 'Informatique'),
(2, 'ICM', 'Information, Communication et Multimédia'),
(3, 'AES', 'Administration Economique et Sociale')
ON CONFLICT (id_mention) DO NOTHING;

-- 2. Seed FILIERES (Licence + Master)
INSERT INTO public.filiere (id_filiere, id_mention, code_filiere, nom_filiere) VALUES
-- Licence Informatique
(1, 1, 'DA2I', 'Développement Application Intranet et Internet'),
(2, 1, 'CIGSI', 'Conception, Intégration et Gestion des Systèmes d''Information'),
-- Master Informatique
(3, 1, 'SIGD', 'Système d''Information, Géomatique et Décision'),
(4, 1, 'M2I', 'Modélisation et Ingénierie Informatique'),
(5, 1, 'SDIA', 'Sciences de Données et Intelligence Artificielle'),
(6, 1, 'IGTI', 'Ingénierie Géospatiale et Technologies de l''Information'),
-- Master Management
(7, 3, 'MEDA', 'Management d''Entreprises et Développement des Affaires'),
(8, 3, 'MD', 'Management Décisionnel'),
-- Master ICM
(9, 2, 'CNMP', 'Communication Numérique et Management de Projet'),
(10, 2, 'CM', 'Communication Multimédia'),
(11, 2, 'CINE', 'Cinématographie')
ON CONFLICT (id_filiere) DO NOTHING;

-- 3. Seed NIVEAUX
INSERT INTO public.niveau (id_niveau, code_niveau, ordre, id_mention) VALUES
-- Licence Informatique
(1, 'L1', 1, 1),
(2, 'L2', 2, 1),
(3, 'L3', 3, 1),
-- Master Informatique
(4, 'M1', 4, 1),
(5, 'M2', 5, 1),
-- Master Management
(6, 'M1', 4, 3),
(7, 'M2', 5, 3),
-- Master ICM
(8, 'M1', 4, 2),
(9, 'M2', 5, 2)
ON CONFLICT (id_niveau) DO NOTHING;

-- 4. Seed REF_SEMESTRE
INSERT INTO public.ref_semestre (id_ref_semestre, code_semestre, ordre, id_niveau) VALUES
-- Licence L1
(1, 'S1', 1, 1),
(2, 'S2', 2, 1),
-- Licence L2
(3, 'S3', 3, 2),
(4, 'S4', 4, 2),
-- Licence L3
(5, 'S5', 5, 3),
(6, 'S6', 6, 3),
-- Master M1
(7, 'S7', 7, 4),
(8, 'S8', 8, 4),
-- Master M2
(9, 'S9', 9, 5),
(10, 'S10', 10, 5),
-- Master Management M1
(11, 'S7', 7, 6),
(12, 'S8', 8, 6),
-- Master Management M2
(13, 'S9', 9, 7),
(14, 'S10', 10, 7),
-- Master ICM M1
(15, 'S7', 7, 8),
(16, 'S8', 8, 8),
-- Master ICM M2
(17, 'S9', 9, 9),
(18, 'S10', 10, 9)
ON CONFLICT (id_ref_semestre) DO NOTHING;

-- 5. Seed ANNEE_ACADEMIQUE
INSERT INTO public.annee_academique (id_annee, libelle, date_debut_annee, date_fin_annee) VALUES
(1, '2025-2026', '2025-10-01', '2026-07-31')
ON CONFLICT (id_annee) DO NOTHING;

-- 6. Seed SEMESTRE (Année 2025-2026)
INSERT INTO public.semestre (id_semestre, id_ref_semestre, id_annee, date_debut, date_fin) VALUES
-- S5 - S6 (L3)
(1, 5, 1, '2025-10-01', '2026-02-28'),
(2, 6, 1, '2026-03-01', '2026-07-31'),
-- S7 - S8 (M1)
(3, 7, 1, '2025-10-01', '2026-02-28'),
(4, 8, 1, '2026-03-01', '2026-07-31'),
-- S9 - S10 (M2)
(5, 9, 1, '2025-10-01', '2026-02-28'),
(6, 10, 1, '2026-03-01', '2026-07-31')
ON CONFLICT (id_semestre) DO NOTHING;

-- 7. Seed CLASSE
INSERT INTO public.classe (id_classe, id_filiere, id_semestre, nom_classe) VALUES
-- L3 Classes
(1, 1, 1, 'L3 DA2I'),
(2, 2, 1, 'L3 CIGSI'),
-- M1 Classes
(3, 3, 3, 'M1 SIGD'),
(4, 4, 3, 'M1 M2I'),
(5, 5, 3, 'M1 SDIA'),
(6, 6, 3, 'M1 IGTI'),
(7, 7, 3, 'M1 MEDA'),
(8, 8, 3, 'M1 MD'),
(9, 9, 3, 'M1 CNMP'),
(10, 10, 3, 'M1 Communication Multimédia'),
(11, 11, 3, 'M1 Cinématographie'),
-- M2 Classes
(12, 3, 5, 'M2 SIGD'),
(13, 4, 5, 'M2 M2I'),
(14, 5, 5, 'M2 SDIA'),
(15, 6, 5, 'M2 IGTI'),
(16, 7, 5, 'M2 MEDA'),
(17, 8, 5, 'M2 MD'),
(18, 9, 5, 'M2 CNMP'),
(19, 10, 5, 'M2 Communication Multimédia'),
(20, 11, 5, 'M2 Cinématographie')
ON CONFLICT (id_classe) DO NOTHING;

-- 8. Seed MATIERE (Matières pour chaque filière Master)

-- 9. Seed PROFESSEUR
INSERT INTO public.professeur (id_professeur, nom, prenom, email, telephone) VALUES
(1, 'Randriana', 'Aristhène', 'a.randriana@emit.edu', '+261 34 00 000 01'),
(2, 'Rakoto', 'Jean', 'j.rakoto@emit.edu', '+261 34 00 000 02'),
(3, 'Volana', 'Claire', 'c.volana@emit.edu', '+261 34 00 000 03'),
(4, 'Andrian', 'Michaël', 'm.andrian@emit.edu', '+261 34 00 000 04'),
(5, 'Raso', 'Sophie', 's.raso@emit.edu', '+261 34 00 000 05'),
(6, 'Ravelo', 'Haja', 'h.ravelo@emit.edu', '+261 34 00 000 06'),
(7, 'Randria', 'Lova', 'l.randria@emit.edu', '+261 34 00 000 07'),
(8, 'Rakotomalala', 'Tiana', 't.rakotomalala@emit.edu', '+261 34 00 000 08'),
(9, 'Andriamanantena', 'Ny Aina', 'na.andriamanantena@emit.edu', '+261 34 00 000 09'),
(10, 'Razafindrakoto', 'Mamy', 'm.razafindrakoto@emit.edu', '+261 34 00 000 10')
ON CONFLICT (id_professeur) DO NOTHING;

-- 10. Seed SALLE
INSERT INTO public.salle (id_salle, code_batiment, etage, numero_porte, nom_salle, capacite, type_salle) VALUES
-- Amphithéâtres (2 étages)
(1, 'D', 0, '001', 'Amphithéâtre D001', 200, 'amphi'),
(2, 'D', 1, '101', 'Amphithéâtre D101', 180, 'amphi'),

-- Salles simples (Bâtiment B) - 4 étages, 3 salles par étage
-- RDC (étage 0)
(3, 'B', 0, '001', 'Salle B001', 50, 'cours'),
(4, 'B', 0, '002', 'Salle B002', 50, 'cours'),
(5, 'B', 0, '003', 'Salle B003', 50, 'cours'),
-- 1er étage
(6, 'B', 1, '101', 'Salle B101', 50, 'cours'),
(7, 'B', 1, '102', 'Salle B102', 50, 'cours'),
(8, 'B', 1, '103', 'Salle B103', 50, 'cours'),
-- 2ème étage
(9, 'B', 2, '201', 'Salle B201', 50, 'cours'),
(10, 'B', 2, '202', 'Salle B202', 50, 'cours'),
(11, 'B', 2, '203', 'Salle B203', 50, 'cours'),
-- 3ème étage
(12, 'B', 3, '301', 'Salle B301', 50, 'cours'),
(13, 'B', 3, '302', 'Salle B302', 50, 'cours'),
(14, 'B', 3, '303', 'Salle B303', 50, 'cours'),

-- Salles TP / 3D (Bâtiment C)
(15, 'C', 0, '001', 'Salle 3D C001', 25, 'tp'),
(16, 'C', 0, '002', 'Bibliothèque C002', 60, 'bibliotheque'),
(17, 'C', 0, '003', 'Labo TP C003', 30, 'tp'),

-- Scolarité (Bâtiment A)
(18, 'A', 0, '001', 'Scolarité A001', 10, 'administration')
ON CONFLICT (id_salle) DO NOTHING;

-- 11. Seed UTILISATEUR
INSERT INTO public.utilisateur (id_utilisateur, nom, prenom, email, role) VALUES
(1, 'Admin', 'EMIT', 'admin@emit.mg', 'admin'),
(2, 'Demandeur', 'Enseignant', 'demandeur@emit.mg', 'demandeur'),
(3, 'Validateur', 'Chef', 'validateur@emit.mg', 'validateur')
ON CONFLICT (id_utilisateur) DO NOTHING;

-- 12. Seed COURS (Quelques cours pour illustrer)
INSERT INTO public.cours (id_cours, id_matiere, id_professeur, id_classe, id_salle, id_semestre, type_cours, statut) VALUES
-- Cours L3 DA2I
(1, 1, 2, 1, 4, 1, 'cours', 'planifié'),
(2, 2, 1, 1, 2, 1, 'tp', 'planifié'),
-- Cours M1 SIGD
(3, 3, 4, 3, 1, 3, 'cours', 'planifié'),
(4, 4, 6, 3, 2, 3, 'tp', 'planifié'),
-- Cours M1 SDIA
(5, 9, 5, 5, 6, 3, 'cours', 'planifié'),
(6, 10, 5, 5, 6, 3, 'tp', 'planifié'),
-- Cours M1 MEDA
(7, 17, 7, 7, 4, 3, 'cours', 'planifié'),
(8, 18, 8, 7, 4, 3, 'cours', 'planifié'),
-- Cours M1 CNMP
(9, 25, 9, 9, 5, 3, 'cours', 'planifié'),
(10, 26, 9, 9, 5, 3, 'tp', 'planifié')
ON CONFLICT (id_cours) DO NOTHING;

-- 13. Seed CRENEAU
INSERT INTO public.creneau (id_creneau, id_cours, jour_semaine, heure_debut, heure_fin, semaine_type) VALUES
(1, 1, 'MON', '08:00:00', '10:00:00', 'A'),
(2, 2, 'TUE', '10:00:00', '12:00:00', 'A'),
(3, 3, 'WED', '14:00:00', '16:00:00', 'A'),
(4, 4, 'THU', '08:00:00', '10:00:00', 'A'),
(5, 5, 'MON', '10:00:00', '12:00:00', 'A'),
(6, 6, 'TUE', '14:00:00', '16:00:00', 'A'),
(7, 7, 'WED', '08:00:00', '10:00:00', 'A'),
(8, 8, 'THU', '10:00:00', '12:00:00', 'A'),
(9, 9, 'FRI', '08:00:00', '10:00:00', 'A'),
(10, 10, 'FRI', '10:00:00', '12:00:00', 'A')
ON CONFLICT (id_creneau) DO NOTHING;