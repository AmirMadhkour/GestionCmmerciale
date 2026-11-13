# Gestion Commerciale

Application de gestion des clients, produits et commandes : **Angular 21** + **ASP.NET Core 8** + **SQL Server**.

Documentation détaillée : [docs/DOCUMENTATION.md](docs/DOCUMENTATION.md)

---

## 1. Installation

**Prérequis** : .NET SDK 8, Node.js 24+, SQL Server Express (instance `SQLEXPRESS`), SSMS, Git.

```powershell
dotnet tool install --global dotnet-ef --version 8.*

git clone <adresse-du-depot> mini_project_isi_madhkour
cd mini_project_isi_madhkour

cd backend
dotnet restore
cd ..

cd Frontend
npm install
cd ..
```

Vérification :

```powershell
dotnet --version
node --version
dotnet ef --version
```

---

## 2. Base de données et migrations

Depuis le dossier `backend` :

```powershell
cd backend

dotnet ef database update --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api
```

Autres commandes :

```powershell
dotnet ef migrations add NomMigration --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api

dotnet ef migrations remove --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api

dotnet ef database drop --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api
```

Générer le script SQL (depuis `backend/src`) :

```powershell
cd backend/src
dotnet ef migrations script --idempotent -o ../../database/schema.sql --project GestionCommerciale.Infrastructure --startup-project GestionCommerciale.Api
```

Script SQL complet prêt à exécuter dans SSMS : `database/Mini_Projet.sql`

Chaîne de connexion : `backend/src/GestionCommerciale.Api/appsettings.json`

---

## 3. Lancer le back end

```powershell
cd backend
dotnet run --project src/GestionCommerciale.Api
```

- API : http://localhost:5013
- Swagger : http://localhost:5013/swagger

---

## 4. Lancer le front end

Dans un second terminal :

```powershell
cd Frontend
npm start
```

- Application : http://localhost:4200

---

## 5. Connexion

| Email | Mot de passe | Rôle |
|---|---|---|
| `Amir@isi.com.tn` | `Admin123!` | ADMIN |

---

## 6. Tests

Arrêter l'API avant de lancer les tests.

```powershell
cd backend
dotnet test
```
