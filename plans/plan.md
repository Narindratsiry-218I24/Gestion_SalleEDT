# Implementation Plan – Gestion de Cours, Matières et Emplois du Temps (ASP.NET Core MVC Edition)

## Objectif
Intégrer :
1. Entité **Matière** (déjà existante, liée aux filières et semestres).
2. Fonctionnalité de planification de cours avec attribution automatique de salles (A-D, selon capacité).
3. Mise à jour automatique de l’**emploi du temps** (Créneaux/Cours) après chaque planification.
4. Améliorations front‑end (Vue `Views/Home/Cours.cshtml`) et composants Razor réutilisables.

---

## Modifications Backend (C# / EF Core)
### Modèle de données (Entity Framework Core)
- Utiliser les entités existantes (`Matiere`, `Salle`, `Cours`, `Creneau`).
- Si besoin de modifications (ex: s'assurer que `IdSalle` dans `Cours` est nullable avant l'attribution), mettre à jour les classes du modèle dans `Models/` et créer une nouvelle migration EF Core (`dotnet ef migrations add UpdateCoursSalleNullable`).

### Services métier
- Créer une interface `IPlanningService` et son implémentation `PlanningService`.
- **PlanningService** – méthode `PlanifierCours(CoursDto)` : recherche salle disponible (selon la capacité requise par la classe et la disponibilité du créneau), vérification conflits, création/ mise à jour du `Cours` et `Creneau`.
- Gestion des erreurs (salle indisponible, conflit horaire) via des exceptions personnalisées (ex: `PlanningException`).
- Enregistrer le service dans `Program.cs` (`builder.Services.AddScoped<IPlanningService, PlanningService>();`).

### Contrôleurs (MVC / API)
- **MatiereController** – S'assurer que le CRUD est complet et fonctionnel pour les matières.
- **CoursController** – Ajouter/Modifier les actions pour la planification (`[HttpPost] Planifier`, `[HttpGet] EmploisDuTemps`). Ajouter une action `[HttpGet] ExportPdf` pour générer un PDF de l'emploi du temps.

---

## Modifications Front‑end (Razor / ASP.NET MVC)
### Structure des vues
- Créer **Views/Shared/_Filter.cshtml** (barre de recherche réutilisable).
- Créer **Views/Shared/_CoursTable.cshtml** (tableau dynamique qui boucle sur `IEnumerable<Cours>`).
- Refactorer **Views/Home/Cours.cshtml** : wrapper gradient, utilisation des Partial Views (`<partial name="_Filter" />`, etc.), balises sémantiques, accessibilité améliorée.
- Ajouter micro‑animations sur les lignes du tableau (`hover:bg-gray-50 transition`).
- Utiliser des icônes Lucide et des couleurs premium (dégradé sombre `[#17203A] → [#12192B]`).

### Styles
- Utiliser exclusivement des classes utilitaires Tailwind CSS pour le design (glass-morphism, transitions, focus rings) directement dans les vues, au lieu d'un fichier CSS personnalisé.

### Assets
- Assurer la présence des icônes (ex: via CDN pour Lucide ou générer des assets locaux dans `wwwroot/images`).

---

## Plan de vérification
1. **Vérifications manuelles** :
   - Création/Modification d’une matière via l'interface.
   - Planification d’un cours : salle auto‑attribuée selon la disponibilité, aucun conflit.
   - Modification d’un cours → mise à jour de l'EDT.
   - Gestion d’erreur : tentative de double réservation → message clair à l'utilisateur.

---

## Open Questions
- Est-il nécessaire d'ajouter un système d'export (PDF/Excel) pour l'emploi du temps généré ?
- Les cours sont-ils récurrents sur tout le semestre ou définis à la semaine ?