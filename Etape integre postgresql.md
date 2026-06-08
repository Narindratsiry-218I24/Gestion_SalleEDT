# 🐘 GUIDE D'INSTALLATION : ASP.NET MVC + PostgreSQL

## 📌 Prérequis

| Logiciel | Version |
|----------|---------|
| Visual Studio | 2019 ou 2022 |
| PostgreSQL | 14, 15 ou 16 |
| .NET Framework | 4.7.2 ou 4.8 |

---

## 📁 Étape 1 : Créer le projet

1. Ouvrir **Visual Studio**
2. **Créer un nouveau projet**
3. Choisir : **ASP.NET Web Application (.NET Framework)**
4. Nom : `Gestion_Salle_classe`
5. Modèle : **MVC**
6. Cliquer sur **Créer**

---

## 📦 Étape 2 : Installer les packages NuGet

### Ouvrir la console NuGet
**Outils** → **Gestionnaire de packages NuGet** → **Console du gestionnaire de packages**

### Exécuter ces commandes une par une

```powershell
Install-Package EntityFramework
Install-Package Npgsql
Install-Package Npgsql.EntityFramework
Install-Package EntityFramework6.Npgsql


🗄️ Étape 3 : Créer la base de données PostgreSQL
Ouvrir pgAdmin
Démarrer pgAdmin et se connecter

Créer une nouvelle base de données
sql
CREATE DATABASE EMIT_EDT_DB;

Ou via l'interface : Clic droit sur Databases → Create → Database → Nom : EMIT_EDT_DB

Noter vos identifiants
Information	Valeur
Serveur	localhost
Port	5432
Base de données	EMIT_EDT_DB
Utilisateur	postgres
Mot de passe	votre_mot_de_passe


⚙️ Étape 4 : Configurer Web.config


Ouvrez Web.config à la racine du projet.

Ajouter la chaîne de connexion
Dans la section <connectionStrings> :

xml
<connectionStrings>
  <add name="EMITDbContext"
       connectionString="Server=localhost;Port=5432;Database=EMIT_EDT_DB;User Id=postgres;Password=votre_mot_de_passe;"
       providerName="Npgsql" />
</connectionStrings>


Configurer Entity Framework pour PostgreSQL
Ajoutez cette section après </connectionStrings> :

xml


<entityFramework>
  <providers>
    <provider invariantName="Npgsql" 
              type="Npgsql.NpgsqlServices, Npgsql.EntityFramework" />
  </providers>
</entityFramework>
Ajouter le fournisseur de données

xml


<system.data>
  <DbProviderFactories>
    <remove invariant="Npgsql" />
    <add name="Npgsql Data Provider" 
         invariant="Npgsql" 
         description=".Net Framework Data Provider for PostgreSQL" 
         type="Npgsql.NpgsqlFactory, Npgsql" />
  </DbProviderFactories>
</system.data>
⚠️ Remplacez votre_mot_de_passe par votre vrai mot de passe PostgreSQL.