# Plan de refonte de la création de Cours (Admin)

1. **Backend (APIs)**
   - Ajouter un endpoint pour récupérer les Niveaux.
   - Ajouter un endpoint pour récupérer les Filières (par Niveau).
   - Ajouter un endpoint pour récupérer les Matières correspondantes (par Filière) qui **n'ont pas encore de cours** assigné.
   
2. **Backend (Création de Cours)**
   - Modifier `CoursController.CreateCourse` :
     - Prendre seulement `id_matiere` et `type_cours` (CM, TD, TP) depuis le formulaire.
     - Récupérer automatiquement `id_professeur` depuis `AffectationMatiere` ou `Matiere.IdProfesseurResponsable`.
     - Récupérer `id_classe` et `capacity` (effectif) depuis la Classe liée à la Filière.

3. **Frontend (Interface)**
   - Modifier `Views/Home/Cours.cshtml` (Modale de création) :
     - Ajouter des Select pour `Niveau`, `Filière`, et `Matière`.
     - Supprimer les champs manuels : `Professeur`, `Classe`, `Capacité`, `Groupe`.
     - Ajouter un appel AJAX pour lier la sélection (Niveau -> Filière -> Matière).
     - Renvoyer vers l'interface de planification (bouton Planifier) une fois le cours créé.