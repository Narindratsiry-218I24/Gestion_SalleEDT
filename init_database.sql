-- Script de réinitialisation complète de la base de données
-- ATTENTION: Ceci va vider les tables et réinitialiser les compteurs

-- 1. Vider les tables (l'ordre CASCADE gère les clés étrangères)
TRUNCATE TABLE 
    public.affectation_matiere,
    public.cours,
    public.disponibilite_prof,
    public.matiere,
    public.classe,
    public.semestre,
    public.annee_academique,
    public.ref_semestre,
    public.salle,
    public.filiere,
    public.niveau,
    public.mention,
    public.professeur,
    public.utilisateur
RESTART IDENTITY CASCADE;


-- 2. Seed MENTIONS
INSERT INTO public.mention (id_mention, code_mention, nom_mention) VALUES
(1, 'INFO', 'INFORMATIQUE'),
(2, 'MANAGEMENT', 'MANAGEMENT'),
(3, 'MULTI-MEDIA', 'MULTI-MEDIA');

-- 3. Seed NIVEAUX
INSERT INTO public.niveau (id_niveau, code_niveau, ordre, id_mention) VALUES
(1, 'L1', 1, 1), (2, 'L2', 2, 1), (3, 'L3', 3, 1),
(4, 'M1', 4, 1), (5, 'M2', 5, 1),
(6, 'L1', 1, 2), (7, 'L2', 2, 2), (8, 'L3', 3, 2),
(9, 'M1', 4, 2), (10, 'M2', 5, 2),
(11, 'L1', 1, 3), (12, 'L2', 2, 3), (13, 'L3', 3, 3),
(14, 'M1', 4, 3), (15, 'M2', 5, 3);

-- 4. Seed FILIERES (Parcours)
INSERT INTO public.filiere (id_filiere, id_mention, code_filiere, nom_filiere) VALUES
(1, 1, 'DA2I', 'DA2I'),
(2, 2, 'AES', 'AES'),
(3, 3, 'ICM', 'ICM'),
(4, 1, 'M2I', 'M2I'),
(5, 1, 'SDIA', 'SDIA'),
(6, 1, 'SIGD', 'SIGD'),
(7, 2, 'AEE', 'AEE'),
(8, 2, 'ASE', 'ASE'),
(9, 2, 'RPO', 'RPO'),
(10, 2, 'RPCO', 'RPCO'),
(11, 2, 'PCO', 'PCO');

-- 5. Seed SALLES
INSERT INTO public.salle (id_salle, code_batiment, etage, numero_porte, nom_salle, capacite, type_salle) VALUES
(1, 'A', 0, '001', 'A001', 10, 'administration'),
(2, 'B', 0, '001', 'B001', 50, 'cours'),
(3, 'B', 0, '002', 'B002', 50, 'cours'),
(4, 'B', 0, '003', 'B003', 50, 'cours'),
(5, 'C', 0, '001', 'C001', 50, 'cours'),
(6, 'C', 0, '002', 'C002 Bibliotheque', 60, 'bibliotheque'),
(7, 'D', 0, '001', 'D001', 200, 'amphi');

-- 6. Seed REF_SEMESTRE
INSERT INTO public.ref_semestre (id_ref_semestre, code_semestre, ordre, id_niveau) VALUES
(1, 'S1', 1, 1), (2, 'S2', 2, 1),
(3, 'S3', 3, 2), (4, 'S4', 4, 2),
(5, 'S5', 5, 3), (6, 'S6', 6, 3),
(7, 'S7', 7, 4), (8, 'S8', 8, 4),
(9, 'S9', 9, 5), (10, 'S10', 10, 5);

-- 7. Seed ANNEE_ACADEMIQUE
INSERT INTO public.annee_academique (id_annee, libelle, date_debut_annee, date_fin_annee) VALUES
(1, '2025-2026', '2025-10-01', '2026-07-31');

-- 8. Seed SEMESTRE (Année 2025-2026)
INSERT INTO public.semestre (id_semestre, id_ref_semestre, id_annee, date_debut, date_fin) VALUES
(1, 1, 1, '2025-10-01', '2026-02-28'), 
(2, 2, 1, '2026-03-01', '2026-07-31');

-- 9. Seed CLASSE
INSERT INTO public.classe (id_classe, id_filiere, id_annee_academique, nom_classe, id_niveau) VALUES
(1, 1, 1, 'L1 DA2I S1', 1),
(2, 4, 1, 'M1 M2I S7', 4),
(3, 5, 1, 'M1 SDIA S7', 4);

-- 10. Seed UTILISATEUR Admin
INSERT INTO public.utilisateur (id_utilisateur, nom, prenom, email, role, password_hash, statut_compte, premier_login, date_creation) VALUES
(1, 'Admin', 'EMIT', 'admin@emit.mg', 'Admin', '$2a$11$0wO58yGf1dDTo8y0cTkEreXw4WfEwM6E1t.XyR.R0B/.j2.2sFmXq', 'Actif', false, NOW());

-- 11. Seed UTILISATEUR Prof (Test)
INSERT INTO public.utilisateur (id_utilisateur, nom, prenom, email, role, password_hash, statut_compte, premier_login, date_creation) VALUES
(2, 'Taratra', 'Tsiry', 'tsiry@gmail.com', 'Professeur', '$2a$11$0wO58yGf1dDTo8y0cTkEreXw4WfEwM6E1t.XyR.R0B/.j2.2sFmXq', 'Actif', false, NOW());

-- 12. Seed PROFESSEUR (Test)
INSERT INTO public.professeur (
    id_professeur, nom, prenom, email, telephone, grade, matricule, titre, telephone_portable, specialite, specialites_secondaires, statut, biographie, id_utilisateur, capacite_horaire_max, est_actif, date_creation, heures_effectuees
) VALUES
(1, 'Rakotondramanan', 'Tsiry', 'tsiry@gmail.com', '0340000000', 'Titulaire', 'MAT-TEST-001', 'Dr', '0340000000', 'Informatique', '[]', 'Permanent', 'Professeur de test pour valider le dashboard.', 2, 20, true, NOW(), 0);


-- 13. Mettre à jour les séquences pour éviter les problèmes d'insertion par la suite
SELECT setval('public.mention_id_mention_seq', COALESCE((SELECT MAX(id_mention)+1 FROM public.mention), 1), false);
SELECT setval('public.niveau_id_niveau_seq', COALESCE((SELECT MAX(id_niveau)+1 FROM public.niveau), 1), false);
SELECT setval('public.filiere_id_filiere_seq', COALESCE((SELECT MAX(id_filiere)+1 FROM public.filiere), 1), false);
SELECT setval('public.salle_id_salle_seq', COALESCE((SELECT MAX(id_salle)+1 FROM public.salle), 1), false);
SELECT setval('public.ref_semestre_id_ref_semestre_seq', COALESCE((SELECT MAX(id_ref_semestre)+1 FROM public.ref_semestre), 1), false);
SELECT setval('public.annee_academique_id_annee_seq', COALESCE((SELECT MAX(id_annee)+1 FROM public.annee_academique), 1), false);
SELECT setval('public.semestre_id_semestre_seq', COALESCE((SELECT MAX(id_semestre)+1 FROM public.semestre), 1), false);
SELECT setval('public.classe_id_classe_seq', COALESCE((SELECT MAX(id_classe)+1 FROM public.classe), 1), false);
SELECT setval('public.utilisateur_id_utilisateur_seq', COALESCE((SELECT MAX(id_utilisateur)+1 FROM public.utilisateur), 1), false);
SELECT setval('public.professeur_id_professeur_seq', COALESCE((SELECT MAX(id_professeur)+1 FROM public.professeur), 1), false);
