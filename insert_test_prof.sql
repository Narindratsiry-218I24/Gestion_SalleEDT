-- Script SQL pour insérer un professeur de test avec un compte utilisateur lié
-- Le mot de passe par défaut est 'password123' (le hash correspond à 'password123' via BCrypt)

-- 1. Insérer l'utilisateur (Compte de connexion)
INSERT INTO utilisateur (
    nom, 
    prenom, 
    email, 
    role, 
    password_hash, 
    statut_compte, 
    premier_login, 
    date_creation
) VALUES (
    'Taratra', 
    'Tsiry', 
    'tsiry@gmail.com', 
    'Professeur', 
    '$2a$11$0wO58yGf1dDTo8y0cTkEreXw4WfEwM6E1t.XyR.R0B/.j2.2sFmXq', -- Hash BCrypt pour 'password123'
    'Actif', 
    false, 
    NOW()
);

-- 2. Récupérer l'ID de l'utilisateur fraîchement inséré et insérer le professeur
-- Note: Sous PostgreSQL, vous pouvez faire ceci avec un DO block ou un CTE, 
-- mais si c'est pour un test rapide, vous pouvez aussi le faire en deux étapes.
-- Voici une méthode qui utilise une sous-requête (en supposant que l'email est unique) :

INSERT INTO professeur (
    nom, 
    prenom, 
    email, 
    telephone, 
    grade, 
    matricule, 
    titre, 
    telephone_portable, 
    specialite, 
    specialites_secondaires, 
    statut, 
    biographie, 
    id_utilisateur, 
    capacite_horaire_max, 
    est_actif, 
    date_creation, 
    heures_effectuees
) VALUES (
    'Taratra', 
    'Tsiry', 
    'tsiry@gmail.com', 
    '0340000000', 
    'Titulaire', 
    'MAT-TEST-001', 
    'Dr', 
    '0340000000', 
    'Informatique', 
    '[]', 
    'Permanent', 
    'Professeur de test pour valider le dashboard.', 
    (SELECT id_utilisateur FROM utilisateur WHERE email = 'tsiry@gmail.com' LIMIT 1), 
    20, 
    true, 
    NOW(), 
    0
);
