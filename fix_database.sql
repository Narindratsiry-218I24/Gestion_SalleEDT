-- Ajout de la colonne departement à la table professeur si elle n'existe pas
ALTER TABLE public.professeur ADD COLUMN IF NOT EXISTS departement VARCHAR(100) DEFAULT '';

-- Création de la table groupes si elle n'existe pas
CREATE TABLE IF NOT EXISTS public.groupes (
    id_groupe SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    id_classe INT NOT NULL,
    CONSTRAINT fk_groupes_classe FOREIGN KEY (id_classe) REFERENCES public.classe (id_classe) ON DELETE CASCADE
);

-- Création de la table seances si elle n'existe pas
CREATE TABLE IF NOT EXISTS public.seances (
    id_seance SERIAL PRIMARY KEY,
    id_cours INT NOT NULL,
    date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    id_salle INT,
    id_groupe INT,
    realized_hours INT NOT NULL DEFAULT 0,
    attendance DOUBLE PRECISION,
    CONSTRAINT fk_seances_cours FOREIGN KEY (id_cours) REFERENCES public.cours (id_cours) ON DELETE CASCADE,
    CONSTRAINT fk_seances_salle FOREIGN KEY (id_salle) REFERENCES public.salle (id_salle) ON DELETE SET NULL,
    CONSTRAINT fk_seances_groupe FOREIGN KEY (id_groupe) REFERENCES public.groupes (id_groupe) ON DELETE SET NULL
);

-- Création de la table subjects si elle n'existe pas (utilisée par schedules)
CREATE TABLE IF NOT EXISTS public.subjects (
    id_subject SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    label VARCHAR(200) NOT NULL,
    credits INT NOT NULL DEFAULT 0,
    hours INT NOT NULL DEFAULT 0,
    type VARCHAR(50),
    id_niveau INT,
    id_mention INT,
    id_filiere INT,
    id_semestre INT
);

-- Création de la table schedules si elle n'existe pas
CREATE TABLE IF NOT EXISTS public.schedules (
    id_schedule SERIAL PRIMARY KEY,
    id_subject INT NOT NULL,
    session_type VARCHAR(50) NOT NULL,
    id_professeur INT NOT NULL,
    date_schedule DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    id_salle INT NOT NULL,
    status VARCHAR(50),
    notes TEXT
);

