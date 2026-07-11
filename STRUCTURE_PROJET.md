# Structure complète du projet Gestion_SalleEDT

Ce projet est une application web ASP.NET Core MVC pour la gestion des salles, des cours, des emplois du temps et des demandes de réservation dans un contexte universitaire.

## 1. Vue d’ensemble du projet

- Technologie principale : ASP.NET Core MVC
- Base de données : PostgreSQL avec Entity Framework Core
- Interface : Razor Views + Tailwind CSS
- Logique métier : services dédiés pour la planification et la gestion des emplois du temps

---

## 2. Structure du dossier racine

- [appsettings.json](appsettings.json) : Configuration globale de l’application, notamment la connexion à la base de données et les paramètres d’environnement.
- [Program.cs](Program.cs) : Point d’entrée de l’application ASP.NET Core ; enregistrement des services, configuration de la base de données, middleware et routage.
- [Gestion_SalleClasseEDT.csproj](Gestion_SalleClasseEDT.csproj) : Fichier de projet .NET contenant les dépendances NuGet nécessaires.
- [global.json](global.json) : Version du SDK .NET utilisée pour exécuter le projet.
- [package.json](package.json) : Dépendances JavaScript et scripts de construction du CSS Tailwind.
- [tailwind.config.js](tailwind.config.js) : Configuration de Tailwind CSS.
- [NuGet.config](NuGet.config) : Configuration des sources de packages NuGet.
- [dotnet-tools.json](dotnet-tools.json) : Outils .NET locaux utilisés par le projet.
- [MOdel.sql](MOdel.sql) : Script SQL de structure ou de modèle de données.
- [seed.sql](seed.sql) : Script SQL utilisé pour peupler la base au démarrage.
- [fix_database.sql](fix_database.sql) : Script de correction ou de réparation de la base de données.
- [fix_props.js](fix_props.js) : Script utilitaire pour corriger des propriétés ou des paramètres du projet.
- [fix_utf8.js](fix_utf8.js) : Script utilitaire pour gérer l’encodage UTF-8.
- [COLLAB_REGLAGE.md](COLLAB_REGLAGE.md) : Documentation de collaboration ou de réglage du projet.
- [Etape integre postgresql.md](Etape%20integre%20postgresql.md) : Notes d’intégration de PostgreSQL.
- [plans/plan.md](plans/plan.md) : Plan ou documentation interne du développement.

---

## 3. Dossier Controllers

Les contrôleurs reçoivent les requêtes HTTP et orchestrent la logique liée aux vues et aux services.

- [Controllers/HomeController.cs](Controllers/HomeController.cs) : Gère les pages d’accueil, tableau de bord, connexion, inscriptions, matières, salles, validation et détails des cours.
- [Controllers/EDTController.cs](Controllers/EDTController.cs) : Expose des API pour retourner les emplois du temps par salle, par classe ou par professeur.
- [Controllers/ScheduleController.cs](Controllers/ScheduleController.cs) : API de création et de consultation des plannings/schedules.
- [Controllers/ClasseController.cs](Controllers/ClasseController.cs) : Gestion des classes.
- [Controllers/CoursController.cs](Controllers/CoursController.cs) : Gestion des cours.
- [Controllers/DashboardController.cs](Controllers/DashboardController.cs) : Endpoints liés au tableau de bord.
- [Controllers/DemandeEDTController.cs](Controllers/DemandeEDTController.cs) : Gestion des demandes d’emplois du temps.
- [Controllers/FiliereController.cs](Controllers/FiliereController.cs) : Gestion des filières.
- [Controllers/MatiereController.cs](Controllers/MatiereController.cs) : Gestion des matières.
- [Controllers/MentionController.cs](Controllers/MentionController.cs) : Gestion des mentions.
- [Controllers/NiveauController.cs](Controllers/NiveauController.cs) : Gestion des niveaux.
- [Controllers/ProfesseurController.cs](Controllers/ProfesseurController.cs) : Gestion des professeurs.
- [Controllers/SalleController.cs](Controllers/SalleController.cs) : Gestion des salles.
- [Controllers/SubjectController.cs](Controllers/SubjectController.cs) : Gestion des sujets ou matières secondaires du système.
- [Controllers/UtilisateurController.cs](Controllers/UtilisateurController.cs) : Gestion des utilisateurs.

---

## 4. Dossier Models

Les modèles représentent les entités métier et la structure de la base de données.

- [Models/EMITDbContext.cs](Models/EMITDbContext.cs) : Contexte Entity Framework Core qui centralise les DbSet et la configuration des relations entre entités.
- [Models/Utilisateurs.cs](Models/Utilisateurs.cs) : Représente les utilisateurs du système.
- [Models/Professeurs.cs](Models/Professeurs.cs) : Représente un professeur.
- [Models/Matiere.cs](Models/Matiere.cs) : Représente une matière.
- [Models/Cours.cs](Models/Cours.cs) : Représente un cours à planifier.
- [Models/Seance.cs](Models/Seance.cs) : Représente une séance effective dans l’emploi du temps.
- [Models/Salle.cs](Models/Salle.cs) : Représente une salle de classe ou de réservation.
- [Models/Classes.cs](Models/Classes.cs) : Représente une classe d’étudiants.
- [Models/Filiere.cs](Models/Filiere.cs) : Représente une filière.
- [Models/Mention.cs](Models/Mention.cs) : Représente une mention.
- [Models/Niveaux.cs](Models/Niveaux.cs) : Représente un niveau d’études.
- [Models/Semestre.cs](Models/Semestre.cs) : Représente un semestre.
- [Models/Ref_semestre.cs](Models/Ref_semestre.cs) : Référence de semestre utilisée pour la structure académique.
- [Models/AnneeAcademiques.cs](Models/AnneeAcademiques.cs) : Représente une année académique.
- [Models/Groupe.cs](Models/Groupe.cs) : Représente un groupe d’étudiants.
- [Models/Creneau.cs](Models/Creneau.cs) : Représente un créneau horaire.
- [Models/DisponibilteProf.cs](Models/DisponibilteProf.cs) : Stocke les disponibilités des professeurs.
- [Models/DemandeEdt.cs](Models/DemandeEdt.cs) : Modèle de demande d’emploi du temps.
- [Models/PropositionAdmin.cs](Models/PropositionAdmin.cs) : Représente une proposition administrateur.
- [Models/StructureConfig.cs](Models/StructureConfig.cs) : Configuration de la structure académique.
- [Models/AuditLog.cs](Models/AuditLog.cs) : Journalisation des actions importantes.
- [Models/Prerequisite.cs](Models/Prerequisite.cs) : Représente des prérequis entre matières ou cours.
- [Models/Subject.cs](Models/Subject.cs) : Entité générique pour les sujets ou objets d’enseignement.
- [Models/Schedule.cs](Models/Schedule.cs) : Représente un planning ou un horaire créé.
- [Models/Enums.cs](Models/Enums.cs) : Définit les énumérations utilisées dans le système.
- [Models/ErrorViewModel.cs](Models/ErrorViewModel.cs) : Modèle utilisé pour les erreurs de vue.

---

## 5. Dossier Services

Les services contiennent la logique métier indépendante des contrôleurs.

- [Services/IPlanningService.cs](Services/IPlanningService.cs) : Interface définissant les opérations de planification.
- [Services/PlanningService.cs](Services/PlanningService.cs) : Service principal pour planifier des cours et des séances avec vérification de conflits.
- [Services/SchedulingService.cs](Services/SchedulingService.cs) : Service pour créer des plannings avec contrôle des conflits de salle, professeur et horaire.
- [Services/SubjectService.cs](Services/SubjectService.cs) : Service lié à la gestion des sujets.
- [Services/MatiereService.cs](Services/MatiereService.cs) : Service lié à la gestion des matières.
- [Services/AuditService.cs](Services/AuditService.cs) : Service de journalisation des actions d’audit.

---

## 6. Dossier Helpers

- [Helpers/UserContextHelper.cs](Helpers/UserContextHelper.cs) : Aide à extraire le contexte utilisateur à partir des headers HTTP pour les requêtes internes ou API.

---

## 7. Dossier Views

Les vues Razor affichent l’interface utilisateur.

### Views/Home

- [Views/Home/Index.cshtml](Views/Home/Index.cshtml) : Page d’accueil du portail avec présentation du système.
- [Views/Home/Connexion.cshtml](Views/Home/Connexion.cshtml) : Page de connexion.
- [Views/Home/Inscription.cshtml](Views/Home/Inscription.cshtml) : Page d’inscription.
- [Views/Home/Dashboard.cshtml](Views/Home/Dashboard.cshtml) : Vue du tableau de bord principal.
- [Views/Home/Cours.cshtml](Views/Home/Cours.cshtml) : Liste des cours disponibles.
- [Views/Home/CourseDetails.cshtml](Views/Home/CourseDetails.cshtml) : Détails d’un cours spécifique.
- [Views/Home/EDT.cshtml](Views/Home/EDT.cshtml) : Page d’affichage de l’emploi du temps.
- [Views/Home/Matieres.cshtml](Views/Home/Matieres.cshtml) : Gestion de la liste des matières.
- [Views/Home/Parametres.cshtml](Views/Home/Parametres.cshtml) : Paramètres de l’application.
- [Views/Home/Privacy.cshtml](Views/Home/Privacy.cshtml) : Page de confidentialité.
- [Views/Home/Professeurs.cshtml](Views/Home/Professeurs.cshtml) : Liste des professeurs.
- [Views/Home/Salles.cshtml](Views/Home/Salles.cshtml) : Liste des salles.
- [Views/Home/Requetes.cshtml](Views/Home/Requetes.cshtml) : Page de gestion des requêtes.
- [Views/Home/Structures.cshtml](Views/Home/Structures.cshtml) : Interface de gestion des structures académiques.
- [Views/Home/Validation.cshtml](Views/Home/Validation.cshtml) : Page de validation des actions ou demandes.

### Views/Schedule

- [Views/Schedule/Index.cshtml](Views/Schedule/Index.cshtml) : Vue d’index pour la gestion des plannings.

### Views/Subject

- [Views/Subject/Index.cshtml](Views/Subject/Index.cshtml) : Page listant les sujets.
- [Views/Subject/Create.cshtml](Views/Subject/Create.cshtml) : Formulaire de création d’un sujet.

### Views/Shared

- [Views/Shared/_Layout.cshtml](Views/Shared/_Layout.cshtml) : Mise en page commune de toute l’application.
- [Views/Shared/_CoursTable.cshtml](Views/Shared/_CoursTable.cshtml) : Composant Razor réutilisable pour afficher un tableau de cours.
- [Views/Shared/_Filter.cshtml](Views/Shared/_Filter.cshtml) : Composant de filtres réutilisable.
- [Views/Shared/_MatiereForm.cshtml](Views/Shared/_MatiereForm.cshtml) : Formulaire réutilisable de matière.
- [Views/Shared/_ValidationScriptsPartial.cshtml](Views/Shared/_ValidationScriptsPartial.cshtml) : Scripts de validation partagés.
- [Views/Shared/Error.cshtml](Views/Shared/Error.cshtml) : Vue d’erreur globale.

### Views de base

- [Views/_ViewImports.cshtml](Views/_ViewImports.cshtml) : Imports globaux Razor utilisés par les vues.
- [Views/_ViewStart.cshtml](Views/_ViewStart.cshtml) : Code exécuté au démarrage de chaque vue.

---

## 8. Dossier wwwroot

Contient les ressources statiques servies par l’application.

- [wwwroot/css](wwwroot/css) : Styles CSS et fichiers Tailwind compilés.
- [wwwroot/images](wwwroot/images) : Images du site, logos et visuels.
- [wwwroot/js](wwwroot/js) : Scripts JavaScript côté client.
- [wwwroot/lib](wwwroot/lib) : Bibliothèques externes et dépendances client.

---

## 9. Dossier Tests

Contient les tests unitaires du projet.

- [Tests/SchedulingServiceTests.cs](Tests/SchedulingServiceTests.cs) : Tests du service de planification.
- [Tests/Gestion_SalleTests.csproj](Tests/Gestion_SalleTests.csproj) : Fichier de projet pour la suite de tests.
- [Tests/SubjectServiceTests.cs](Tests/SubjectServiceTests.cs) : Tests du service de gestion des sujets.

---

## 10. Dossier Migrations

Contient les migrations Entity Framework Core générées pour la base de données.

- [Migrations/20260710135149_InitSchema.cs](Migrations/20260710135149_InitSchema.cs) : Migration d’initialisation de la base.
- [Migrations/20260710135149_InitSchema.Designer.cs](Migrations/20260710135149_InitSchema.Designer.cs) : Version compilée de la migration.
- [Migrations/EMITDbContextModelSnapshot.cs](Migrations/EMITDbContextModelSnapshot.cs) : Snapshot du modèle de base au moment de la dernière migration.

---

## 11. Résumé fonctionnel du projet

Ce projet permet de :

- gérer les salles, les matières, les professeurs, les classes et les filières ;
- créer et visualiser un emploi du temps ;
- planifier des séances avec détection de conflits ;
- gérer des demandes de réservation ou d’ajustement ;
- journaliser les actions importantes via AuditLog.

---

## 12. Point d’entrée principal

Le cœur du projet démarre dans [Program.cs](Program.cs). C’est là que :

- la base PostgreSQL est configurée ;
- les services métiers sont enregistrés ;
- les routes MVC et API sont configurées ;
- les migrations et le seed SQL sont appliqués au démarrage.
