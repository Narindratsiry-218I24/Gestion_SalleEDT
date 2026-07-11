-- Fonction pour créer un utilisateur et un professeur associé en même temps
-- Par défaut, le mot de passe est défini sur 'password123' (haché via BCrypt)

CREATE OR REPLACE FUNCTION ajouter_professeur_complet(
    p_nom VARCHAR,
    p_prenom VARCHAR,
    p_email VARCHAR,
    p_telephone VARCHAR,
    p_grade VARCHAR,
    p_matricule VARCHAR,
    p_titre VARCHAR,
    p_specialite VARCHAR
) RETURNS VOID AS $$
DECLARE
    v_id_utilisateur INT;
BEGIN
    -- 1. Insérer le compte utilisateur (Rôle : Professeur, Actif)
    INSERT INTO public.utilisateur (
        nom, 
        prenom, 
        email, 
        role, 
        password_hash, 
        statut_compte, 
        premier_login, 
        date_creation
    ) VALUES (
        p_nom, 
        p_prenom, 
        p_email, 
        'Professeur', 
        '$2a$11$0wO58yGf1dDTo8y0cTkEreXw4WfEwM6E1t.XyR.R0B/.j2.2sFmXq', -- Hash pour 'password123'
        'Actif', 
        false, 
        NOW()
    ) RETURNING id_utilisateur INTO v_id_utilisateur;

    -- 2. Insérer le professeur avec le MÊME ID que l'utilisateur
    INSERT INTO public.professeur (
        id_professeur,
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
        v_id_utilisateur, -- ID PROFESSEUR = ID UTILISATEUR
        p_nom, 
        p_prenom, 
        p_email, 
        p_telephone, 
        p_grade, 
        p_matricule, 
        p_titre, 
        p_telephone, 
        p_specialite, 
        '[]', 
        'Permanent', 
        '', 
        v_id_utilisateur, 
        20, 
        true, 
        NOW(), 
        0
    );

    -- 3. Mettre à jour la séquence de professeur pour éviter les conflits futurs
    PERFORM setval('public.professeur_id_professeur_seq', (SELECT MAX(id_professeur) FROM public.professeur));

    -- Message de confirmation dans les logs SQL
    RAISE NOTICE 'Professeur et compte utilisateur % créés avec succès (ID Utilisateur: %)', p_email, v_id_utilisateur;
END;
$$ LANGUAGE plpgsql;

/*
=========================================
EXEMPLES D'UTILISATION DE LA FONCTION :
=========================================

-- Pour ajouter un nouveau professeur, vous pouvez maintenant exécuter simplement ceci :

SELECT ajouter_professeur_complet(
    'Dupont', 
    'Jean', 
    'j.dupont@example.com', 
    '0340011122', 
    'Titulaire', 
    'MAT-002', 
    'Pr', 
    'Mathématiques'
);

SELECT ajouter_professeur_complet(
    'Martin', 
    'Sophie', 
    's.martin@example.com', 
    '0340033344', 
    'Vacataire', 
    'MAT-003', 
    'Dr', 
    'Physique'
);

*/
