# 🚀 P-Fun
![Stars](https://img.shields.io/github/stars/BlackAngelTVdev/P-Fun?style=for-the-badge&color=yellow)
![Commits](https://img.shields.io/github/commit-activity/m/BlackAngelTVdev/P-Fun?style=for-the-badge&color=blue)
![Issues](https://img.shields.io/github/issues/BlackAngelTVdev/P-Fun?style=for-the-badge&color=orange)
![Forks](https://img.shields.io/github/forks/BlackAngelTVdev/P-Fun?style=for-the-badge&color=808080)
![Last Commit](https://img.shields.io/github/last-commit/BlackAngelTVdev/P-Fun?style=for-the-badge&color=blue)

> **Concevoir un logiciel pour afficher des graphiques de séries temporelles**

Application de bureau qui superpose plusieurs séries temporelles de cryptomonnaies,
d'or et de devises sur un même graphique, avec les données stockées localement.

---

## 🧐 Aperçu

Les bougies sont récupérées au format JSON, importées dans une base SQLite locale, puis
tracées avec ScottPlot. Le cœur de calcul est séparé de l'interface WinForms : la
géométrie du graphique, la description des points et la segmentation des séries vivent
dans `P-Fun/Core/` et se testent sans ouvrir la fenêtre.

| Caractéristique | Détail |
| :--- | :--- |
| Interface | WinForms (.NET 8, `net8.0-windows`) |
| Graphique | ScottPlot 5 |
| Stockage | SQLite locale (`data/p-fun.db`) |
| Séries tracées | 5 (BTC, ETH, SOL, PAXG, EUR) |
| Volume de données | 909 770 bougies du 25/05/2026 au 28/09/2026 |
| Taille de la base | ~23 Mo |
| Unité de temps | 1 minute |
| Plateforme | Windows uniquement (`net8.0-windows` + WinForms) |

## ✨ Fonctionnalités

### Affichage
- Tracé de plusieurs séries temporelles superposées dans un graphique ScottPlot.
- Une case à cocher par série : cocher ou décocher redessine le graphique immédiatement.
- Palette de couleurs catégorie (Category10) attribuée automatiquement à chaque série.
- Mode **comparaison en base 100**, indispensable pour comparer un bitcoin à 60 000
  et un euro à 1,14 sur le même graphique sans que l'un écrase l'autre.
- **Double échelle verticale** en prix réel : la première série cochée garde l'axe de
  gauche, la deuxième passe à droite, ce qui permet de comparer deux prix très éloignés
  sans passer en base 100.
- Segmentation des séries sur les trous de données : deux blocs non jointifs ne sont
  pas reliés, ce qui évite de laisser croire à une tendance qui n'existe pas.

### Interaction
- Infobulle au survol : le point le plus proche du curseur est recherché et sa date et
  son prix sont affichés. L'infobulle disparaît quand la souris quitte le graphique.

### Données
- Import d'un dossier de fichiers JSON via le bouton `Importer un dossier JSON…`.
- Stockage en base SQLite locale, créée automatiquement au premier lancement à partir
  des JSON du dossier `data`.
- Fusion incrémentale : les nouvelles bougies sont ajoutées et celles qui chevauchent
  des données déjà stockées sont remplacées, sans créer de doublon.
- Les fichiers JSON illisibles ou non reconnus sont ignorés, avec un avertissement
  indiquant combien de fichiers ont été sautés.
- Un libellé affiche en permanence le nom de la base, le nombre de bougies et sa taille
  sur le disque (par exemple `909 770 bougies — 23 Mo`).

### Architecture
- Noyau de calcul (`P-Fun/Core/`) séparé de l'interface : géométrie du graphique,
  recherche du point survolé, extensions de séries, segmentation sur les trous.
- Couche d'accès aux données (`P-Fun/Data/`) isolée : import JSON, base SQLite et
  politique d'import (décider si le dossier JSON doit être relu au démarrage).
- Tests xUnit sur la lecture/écriture en base, la politique d'import et le survol.

## 🛠 Tech Stack

| Technologie | Usage |
| :--- | :--- |
| ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white) | Langage principal |
| ![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white) | Runtime / Framework (net8.0-windows) |
| ![WinForms](https://img.shields.io/badge/WinForms-512BD4?style=flat-square) | Interface graphique (desktop) |
| ![ScottPlot](https://img.shields.io/badge/ScottPlot-FF6600?style=flat-square) | Graphiques / Tracés (v5.1.59) |
| ![JSON](https://img.shields.io/badge/JSON-000000?style=flat-square) | Format d'import des données |
| ![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white) | Stockage local des séries (Microsoft.Data.Sqlite) |
| ![xUnit](https://img.shields.io/badge/xUnit-512BD4?style=flat-square) | Tests unitaires (v2.9.2) |
| ![Python](https://img.shields.io/badge/Python-3776AB?style=flat-square&logo=python&logoColor=white) | Script de récupération des données Binance |

## 🚀 Installation & Lancement

Prérequis : **Windows** et le **SDK .NET 8**.

```bash
# Restauration des dépendances et lancement de l'application
dotnet run --project P-Fun
```

Pour lancer les tests :

```bash
dotnet test
```

## 📖 Utilisation

1. Lancez l'application.
2. Les fichiers JSON des séries sont placés dans le dossier `data`.
3. Les graphiques s'affichent automatiquement dans l'interface.

Les données sont stockées dans une base SQLite locale (`data/p-fun.db`). Au premier
lancement, la base est créée à partir des fichiers JSON du dossier `data`, puis
l'application lit uniquement la base. Importer un dossier JSON ajoute les nouvelles
bougies et remplace celles qui chevauchent des données déjà stockées.

### Récupérer les données

> **Les données réelles ne sont pas versionnées.** Le dossier `data/` contient des
> fichiers générés et volumineux : environ 23 Mo pour la base SQLite, plus de 240 Mo
> pour les JSON. Ils sont ignorés par git et doivent être régénérés localement. Seul
> le jeu réduit de `data/exemple/` (~900 Ko) est fourni avec le projet.

L'application n'appelle pas l'API Binance elle-même. C'est le script Python qui récupère
les bougies et écrit un fichier par série :

```bash
# Les 5 séries sur la plage par défaut (depuis le 14/09/2026), bougies de 1 minute
python scripts/fetch_binance_data.py

# Restreindre la plage
python scripts/fetch_binance_data.py --start 2026-09-14 --end 2026-09-28

# Changer l'unité de temps (1m, 5m, 1h, 1d, ...)
python scripts/fetch_binance_data.py --start 2026-09-14 --interval 5m
```

Le script écrit `btc.json`, `eth.json`, `solu.json`, `gold.json` et `eur.json` dans
`data/`. L'API Binance renvoyant au maximum 1000 bougies par requête, le script pagine
automatiquement pour couvrir toute la plage demandée.

### Données d'exemple (sans Python)

Un jeu réduit est versionné dans `data/exemple/` : les 5 séries échantillonnées toutes
les 4 heures du 25/05/2026 au 28/09/2026, soit 759 bougies par série (~900 Ko).

```bash
dotnet run --project P-Fun
```

1. Cliquer sur **Importer un dossier JSON…**.
2. Choisir le dossier `data/exemple`.
3. Les 5 séries s'affichent ; la base `data/p-fun.db` est créée ou mise à jour.

Pour repartir de zéro, fermer l'application et supprimer `data/p-fun.db` : elle sera
recréée au prochain import. Les fichiers gardent le format Binance, donc le même
importeur les lit que les données complètes.

## 🧪 Tests

Les 9 tests unitaires couvrent la couche de données et le survol, là où se trouvent les
règles métier les plus risquées :

| Fichier de test | Ce qui est vérifié |
| :--- | :--- |
| `SeriesDatabaseTests.cs` | Import d'un dossier, remplacement des bougies déjà stockées sans doublon, tri chronologique, fichiers illisibles ignorés |
| `DataFolderPolicyTests.cs` | Le dossier JSON n'est relu que si son fichier le plus récent est plus récent que la base |
| `HoverSearchTests.cs` | Survol avec une ou deux échelles verticales, série de droite, graphique vide |

```bash
dotnet test
```

## 📁 Organisation du projet

```
P-Fun/
├── Core/           Calcul pur (géométrie, points, séries, segmentation) — testable
├── Data/           Import JSON, base SQLite, politique d'import
├── Models/         Modèle de données (PriceSeries)
├── Extensions/     Extensions LINQ sur les séries
├── mainPage.cs     Fenêtre WinForms : assemblage et affichage
└── Program.cs      Point d'entrée
P-Fun.Tests/        Tests xUnit
scripts/            Récupération des données Binance
doc/                Rapport de projet
data/               Données générées (non versionnées) + jeu d'exemple versionné
```

## 🤝 Contribution
1. Forkez le projet
2. Créez votre branche (`git checkout -b feature/AmazingFeature`)
3. Commit (`git commit -m 'Add some AmazingFeature'`)
4. Push (`git push origin feature/AmazingFeature`)
5. Ouvrez une Pull Request

## 👤 Auteur

**BlackAngelTVdev**
![Follow](https://img.shields.io/github/followers/BlackAngelTVdev?label=Follow%20Me&style=social)

---
## 📄 Licence

Ce projet est sous licence :
![GitHub License](https://img.shields.io/github/license/BlackAngelTVdev/P-Fun?style=flat-square&color=blue)

### 🧑‍💻 Contributors

Merci à toutes les personnes qui contribuent au projet.

[![Contributors](https://contrib.rocks/image?repo=BlackAngelTVdev/P-Fun)](https://github.com/BlackAngelTVdev/P-Fun/graphs/contributors)