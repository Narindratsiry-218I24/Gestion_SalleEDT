# 🤝 Guide de Collaboration et de Réglage

Ce document détaille les étapes nécessaires pour configurer et lancer le projet **Gestion_Salle_classe** dans un environnement collaboratif.

## 📋 Prérequis système

| Composant | Version recommandée |
|-----------|---------------------|
| **IDE** | Visual Studio 2019 ou 2022 |
| **Framework** | .NET Framework 4.7.2 |
| **Base de données** | PostgreSQL (14, 15 ou 16) |
| **Outil Git** | Git pour Windows |

---

## 🚀 Étape 1 : Clonage et Ouverture du projet

1. Ouvrez votre terminal ou Git Bash.
2. Clonez le dépôt :
   ```bash
   git clone [URL_DU_REPO]
   ```
3. Ouvrez le dossier du projet dans **Visual Studio**.
4. Double-cliquez sur le fichier `Gestion_Salle_classe.sln` pour charger la solution.

---

## 📦 Étape 2 : Gestion des Dépendances (NuGet)

Le projet utilise NuGet pour gérer les bibliothèques. Pour restaurer les dépendances :
1. Faites un clic droit sur la **Solution** dans l'Explorateur de solutions.
2. Choisissez **Restaurer les packages NuGet**.

### Principales dépendances et versions :
| Package | Version | Utilité |
|---------|---------|---------|
| `EntityFramework` | 6.5.2 | ORM pour l'accès aux données |
| `Npgsql` | 4.1.3 | Driver PostgreSQL |
| `EntityFramework6.Npgsql` | 6.4.3 | Support EF6 pour PostgreSQL |
| `DotNetEnv` | 2.3.0 | Chargement des variables d'environnement |
| `Newtonsoft.Json` | 12.0.2 | Manipulation des données JSON |
| `Microsoft.AspNet.WebApi` | 5.2.7 | Framework API REST |

---

## 🗄️ Étape 3 : Réglage de la Base de Données

### 1. Création de la DB
Ouvrez **pgAdmin** ou votre client SQL préféré et exécutez :
```sql
CREATE DATABASE EMIT_EDT_DB;
```

### 2. Configuration locale (.env)
Créez un fichier `.env` à la racine du projet (au même niveau que `Web.config`) avec vos identifiants :
```env
DB_SERVER=localhost
DB_PORT=5432
DB_NAME=EMIT_EDT_DB
DB_USER=postgres
DB_PASSWORD=votre_mot_de_passe
```

### 3. Vérification Web.config
Assurez-vous que la section `<connectionStrings>` dans le fichier `Web.config` correspond à votre setup :
```xml
<connectionStrings>
    <add name="EMITDbContext" 
         connectionString="Server=localhost;Port=5432;Database=EMIT_EDT_DB;User Id=postgres;Password=votre_mot_de_passe;" 
         providerName="Npgsql" />
</connectionStrings>
```

---

## 🔄 Étape 4 : Migration de la Base de Données

Une fois la base de données créée et configurée, vous devez générer les tables.
1. Allez dans **Outils** > **Gestionnaire de packages NuGet** > **Console du gestionnaire de packages**.
2. Exécutez la commande suivante :
   ```powershell
   Update-Database
   ```
*Cela appliquera les migrations existantes (comme `InitialCreate`) à votre base PostgreSQL.*

---

## 🛠️ Réglages techniques importants

### Chargement de l'environnement
Le projet utilise `DotNetEnv` dans `Global.asax.cs` pour charger les réglages du fichier `.env` au démarrage :
```csharp
string envPath = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, ".env");
if (System.IO.File.Exists(envPath)) {
    DotNetEnv.Env.Load(envPath);
}
```

### Architecture
- **Controllers** : Contient les `ApiControllers` pour les endpoints REST.
- **Models** : Contient les entités EF6.
- **Migrations** : Historique des changements de schéma de base de données.

---

## ✅ Vérification du fonctionnement
- Appuyez sur **F5** ou cliquez sur **Démarrer** dans Visual Studio.
- L'application devrait s'ouvrir dans votre navigateur.
- Testez un endpoint API (ex: `/api/votre_controleur`) pour vérifier la connexion DB.
