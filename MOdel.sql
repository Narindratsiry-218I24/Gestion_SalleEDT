-- 1. MENTION
CREATE TABLE MENTION (
    id_mention INT PRIMARY KEY,
    code_mention VARCHAR(20) UNIQUE,
    nom_mention VARCHAR(100)
);

-- 2. FILIERE
CREATE TABLE FILIERE (
    id_filiere INT PRIMARY KEY,
    id_mention INT REFERENCES MENTION(id_mention),
    code_filiere VARCHAR(20),
    nom_filiere VARCHAR(100)
);

-- 3. NIVEAU
CREATE TABLE NIVEAU (
    id_niveau INT PRIMARY KEY,
    code_niveau VARCHAR(10),
    ordre INT,
    id_mention INT REFERENCES MENTION(id_mention)
);

-- 4. REF_SEMESTRE (définition fixe)
CREATE TABLE REF_SEMESTRE (
    id_ref_semestre INT PRIMARY KEY,
    code_semestre VARCHAR(5),
    ordre INT,
    id_niveau INT REFERENCES NIVEAU(id_niveau)
);

-- 5. ANNEE_ACADEMIQUE
CREATE TABLE ANNEE_ACADEMIQUE (
    id_annee INT PRIMARY KEY,
    libelle VARCHAR(20),
    date_debut_annee DATE,
    date_fin_annee DATE
);

-- 6. SEMESTRE (instance annuelle)
CREATE TABLE SEMESTRE (
    id_semestre INT PRIMARY KEY,
    id_ref_semestre INT REFERENCES REF_SEMESTRE(id_ref_semestre),
    id_annee INT REFERENCES ANNEE_ACADEMIQUE(id_annee),
    date_debut DATE,
    date_fin DATE,
    UNIQUE(id_ref_semestre, id_annee)
);

-- 7. CLASSE
CREATE TABLE CLASSE (
    id_classe INT PRIMARY KEY,
    id_filiere INT REFERENCES FILIERE(id_filiere),
    id_semestre INT REFERENCES SEMESTRE(id_semestre),
    nom_classe VARCHAR(100)
);

-- 8. MATIERE
CREATE TABLE MATIERE (
    id_matiere INT PRIMARY KEY,
    id_filiere INT REFERENCES FILIERE(id_filiere),
    code_matiere VARCHAR(20),
    nom_matiere VARCHAR(100),
    id_ref_semestre INT REFERENCES REF_SEMESTRE(id_ref_semestre),
    credit INT
);

-- 9. PROFESSEUR
CREATE TABLE PROFESSEUR (
    id_professeur INT PRIMARY KEY,
    nom VARCHAR(50),
    prenom VARCHAR(50),
    email VARCHAR(100),
    telephone VARCHAR(20)
);

-- 10. SALLE
CREATE TABLE SALLE (
    id_salle INT PRIMARY KEY,
    code_batiment CHAR(1) CHECK (code_batiment IN ('A','B','C','D')),
    etage INT,
    numero_porte VARCHAR(10),
    nom_salle VARCHAR(100),
    capacite INT,
    type_salle VARCHAR(20)
);

-- 11. DISPONIBILITE_PROF
CREATE TABLE DISPONIBILITE_PROF (
    id_dispo INT PRIMARY KEY,
    id_professeur INT REFERENCES PROFESSEUR(id_professeur),
    jour_semaine VARCHAR(10),
    heure_debut TIME,
    heure_fin TIME,
    semaine_type CHAR(1) CHECK (semaine_type IN ('A', 'B'))
);

-- 12. COURS
CREATE TABLE COURS (
    id_cours INT PRIMARY KEY,
    id_matiere INT REFERENCES MATIERE(id_matiere),
    id_professeur INT REFERENCES PROFESSEUR(id_professeur),
    id_classe INT REFERENCES CLASSE(id_classe),
    id_salle INT REFERENCES SALLE(id_salle),
    id_semestre INT REFERENCES SEMESTRE(id_semestre),
    type_cours VARCHAR(10) CHECK (type_cours IN ('cours', 'td', 'tp', 'examen')),
    statut VARCHAR(20) CHECK (statut IN ('planifié', 'validé', 'annulé'))
);

-- 13. CRENEAU
CREATE TABLE CRENEAU (
    id_creneau INT PRIMARY KEY,
    id_cours INT REFERENCES COURS(id_cours),
    jour_semaine VARCHAR(10),
    heure_debut TIME,
    heure_fin TIME,
    semaine_type CHAR(1) CHECK (semaine_type IN ('A', 'B'))
);

-- 14. UTILISATEUR
CREATE TABLE UTILISATEUR (
    id_utilisateur INT PRIMARY KEY,
    nom VARCHAR(50),
    prenom VARCHAR(50),
    email VARCHAR(100),
    role VARCHAR(20) CHECK (role IN ('admin', 'demandeur', 'validateur'))
);

-- 15. DEMANDE_EDT
CREATE TABLE DEMANDE_EDT (
    id_demande INT PRIMARY KEY,
    id_demandeur INT REFERENCES UTILISATEUR(id_utilisateur),
    id_validateur INT REFERENCES UTILISATEUR(id_utilisateur),
    id_cours INT REFERENCES COURS(id_cours),
    id_salle INT REFERENCES SALLE(id_salle),
    type_demande VARCHAR(20) CHECK (type_demande IN ('modification_cours', 'reservation_salle')),
    statut VARCHAR(20) CHECK (statut IN ('en_attente', 'validée', 'refusée')),
    justification TEXT
);