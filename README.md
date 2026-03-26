# PartageTexte — CFCAL

Outil interne de partage sécurisé de textes et mots de passe.
Chaque partage est chiffré en AES-256, protégeable par mot de passe, limitable en lectures et doté d'une date d'expiration.

---

## Architecture

```
PartageTexte.sln
├── src/
│   ├── PartageTexte.Domain          — Entités, énumérations (aucune dépendance)
│   ├── PartageTexte.Application     — Interfaces, DTOs, ServicePartage, validateurs
│   ├── PartageTexte.Infrastructure  — Persistance fichier, chiffrement AES-256, hachage PBKDF2
│   ├── PartageTexte.Api             — API REST (FastEndpoints)
│   └── PartageTexte.Web             — Frontend Blazor Server
└── tests/
    └── PartageTexte.Tests           — Tests unitaires xUnit (26 tests)
```

**Pattern** : Clean Architecture — les couches internes n'ont aucune connaissance des couches externes.

```
Domain ← Application ← Infrastructure
                     ← Api
                     ← Web
```

---

## Projets

### `PartageTexte.Domain`
Entité centrale `Partage` avec ses invariants métier :
- `Creer(...)` — factory method
- `EstExpire()` — vérifie la date d'expiration
- `EstEpuise()` — vérifie le compteur de lectures
- `EnregistrerLecture()` — incrémente le compteur

### `PartageTexte.Application`
- **`ServicePartage`** — orchestre la création (`CreerAsync`) et l'accès (`AccederAsync`)
- **`CreerPartageValidateur`** — règles FluentValidation (contenu non vide, max 64 Ko, expiration future...)
- **Interfaces** `IDepotPartage`, `IServiceChiffrement`, `IServiceHachage`

### `PartageTexte.Infrastructure`
- **`DepotPartageFichier`** — persistance JSON sur le système de fichiers, compatible volumes Docker partagés
- **`ServiceChiffrement`** — AES-256-CBC, IV aléatoire préfixé au chiffré, clé en configuration Base64
- **`ServiceHachage`** — PBKDF2 via `Rfc2898DeriveBytes` (350 000 itérations, OWASP), pas de dépendance externe

### `PartageTexte.Api`
API REST avec [FastEndpoints](https://fast-endpoints.com/) :

| Méthode | Route | Description |
|---|---|---|
| `POST` | `/api/partages` | Créer un partage |
| `GET` | `/api/partages/{id}/info` | Méta-données (expiration, protection) |
| `POST` | `/api/partages/{id}/acceder` | Accéder au contenu déchiffré |

### `PartageTexte.Web`
Frontend Blazor Server avec trois pages :

| Route | Page | Description |
|---|---|---|
| `/` | `Creer.razor` | Formulaire de création |
| `/voir/{id}` | `Voir.razor` | Affichage du contenu (avec saisie mot de passe si protégé) |
| `/confirme/{id}` | `Confirme.razor` | Lien à partager après création |

---

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (pour le déploiement multi-conteneur)

---

## Lancement en développement

### 1. Restaurer les dépendances

```bash
dotnet restore
```

### 2. Configurer le chemin de stockage local

Créez `src/PartageTexte.Api/appsettings.Development.json` :

```json
{
  "Stockage": {
    "Chemin": "C:/tmp/partages"
  }
}
```

### 3. Lancer l'API

```bash
dotnet run --project src/PartageTexte.Api
# Ecoute sur http://localhost:5037
```

### 4. Lancer le frontend Web

```bash
dotnet run --project src/PartageTexte.Web
# Ecoute sur https://localhost:7210 / http://localhost:5110
```

---

## Déploiement Docker (multi-conteneurs)

Les deux conteneurs partagent un **volume Docker** monté sur `/data/partages`, ce qui garantit la cohérence des fichiers entre instances.

### Build et démarrage

```bash
docker compose up --build -d
```

| Service | Port exposé | Description |
|---|---|---|
| `api` | `5037` | API REST |
| `web` | `5110` | Frontend Blazor |

### Arrêt

```bash
docker compose down
```

### Arrêt avec suppression des données

```bash
docker compose down -v
```

### Variables d'environnement

| Variable | Valeur par défaut | Description |
|---|---|---|
| `Chiffrement__Cle` | *(voir docker-compose.yml)* | Clé AES-256 en Base64 (32 octets) |
| `Stockage__Chemin` | `/data/partages` | Chemin du dossier de stockage |
| `Api__UrlBase` | `http://api:8080` | URL de l'API vue par le frontend |

> **Production** : remplacez `Chiffrement__Cle` par une clé générée de façon sécurisée :
> ```powershell
> [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
> ```

---

## Tests

```bash
dotnet test
```

26 tests couvrant :

| Classe | Cas testés |
|---|---|
| `ServiceChiffrement_Tests` | Chiffrement / déchiffrement, intégrité |
| `ServiceHachage_Tests` | Hachage / vérification, timing-safe |
| `CreerPartageValidateur_Tests` | Règles de validation FluentValidation |
| `ServicePartage_CreerAsync_Tests` | Création, chiffrement, persistance |
| `ServicePartage_AccederAsync_Tests` | Expiration, mot de passe, compteur de lectures |

Les tests créent un dossier temporaire unique par classe et le suppriment après chaque exécution.

---

## Sécurité

| Mécanisme | Détail |
|---|---|
| Chiffrement | AES-256-CBC, IV aléatoire par partage |
| Hachage mot de passe | PBKDF2-HMAC-SHA256, 350 000 itérations, sel aléatoire 16 octets |
| Comparaison timing-safe | `CryptographicOperations.FixedTimeEquals` |
| Données au repos | Fichiers JSON — le contenu en clair n'est jamais écrit sur disque |
| Expiration | Vérifiée côté serveur à chaque accès |

---

## NuGet utilisés

| Package | Projet | Usage |
|---|---|---|
| `FastEndpoints 5.34.*` | Api | Endpoints REST |
| `FluentValidation 12.*` | Application | Validation des requêtes |
| `FluentValidation.DependencyInjectionExtensions` | Application | Enregistrement DI |
| `Flurl.Http 4.*` | Web | Client HTTP vers l'API |
| `xunit 2.5.*` | Tests | Framework de tests |
| `FluentAssertions 8.*` | Tests | Assertions lisibles |

*Chiffrement et hachage : bibliothèques natives .NET, aucun package externe.*
