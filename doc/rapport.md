# Rapport de Projet — P-Fun


| | |
|---|---|
| **Projet** | P-Fun |
| **Auteur(s)** | Damien Rochat |
| **Date de rendu** | *24 août 2026 / 30 octobre 2026* |
| **Encadrant** | *M. Carrel* |

---

## Table des matières


1. [Introduction](#1-introduction)
   - 1.1 [Objectifs du projet](#11-objectifs-du-projet)
   - 1.2 [Description du domaine](#12-description-du-domaine)
2. [Analyse fonctionnelle](#2-analyse-fonctionnelle)
3. [Planification initiale](#3-planification-initiale)
4. [Rapport de tests](#4-rapport-de-tests)
5. [Usage de l'intelligence artificielle dans le projet](#5-usage-de-lintelligence-artificielle-dans-le-projet)
6. [Bilan du déroulement du projet](#6-bilan-du-déroulement-du-projet)
7. [Bilan produit](#7-bilan-produit)
8. [Conclusion](#8-conclusion)

---

## 1. Introduction

### 1.1 Objectifs du projet

#### Objectifs produit

Le projet consiste à concevoir et développer **P-Fun**, une application de bureau permettant d'afficher des graphiques de séries temporelles. Le programme doit être capable de :

- **Afficher plusieurs séries de données simultanément** sur un même graphique, avec un axe temporel commun, afin de pouvoir comparer l'évolution des différentes courbes dans le temps.
- **Stocker les données localement** au format JSON, afin que l'utilisateur puisse consulter et retravailler ses séries même hors connexion.
- **Importer de nouvelles données** depuis différentes sources (fichiers CSV, fichiers JSON, ou directement depuis l'API Binance), afin de mettre à jour les séries stockées localement.
- **Offrir une grande flexibilité d'affichage** : sélection des séries à afficher/masquer, zoom, navigation dans le temps, afin de permettre une analyse fine des données.

Le domaine choisi est la **cryptomonnaie** : l'application permettra notamment de comparer l'évolution de plusieurs paires de devises (BTC/USDT, ETH/USDT, etc.) sur une période commune, ce qui est particulièrement pertinent pour observer les corrélations entre les différentes monnaies.

#### Objectifs pédagogiques

Ce projet a pour but de mettre en pratique les connaissances acquises durant le cours de 323, et plus particulièrement :

- **Mettre en œuvre les principes de la programmation fonctionnelle** en C# : utilisation systématique de **LINQ** (à la place des boucles `for`/`foreach` classiques), fonctions d'ordre supérieur, immuabilité, et développement d'**au moins deux extensions du langage C#** (méthodes d'extension).
- **Approfondir les connaissances en C#** et en .NET, notamment à travers la consommation d'une API REST, la sérialisation/désérialisation JSON et la construction d'une interface graphique avec **WinForms**.
- **Découvrir une librairie de visualisation de données** (ScottPlot) et apprendre à l'intégrer dans une application existante.
- **Appliquer une démarche de gestion de projet** : analyse fonctionnelle, user stories, maquettes, planification en sprints, tests unitaires et journal de travail.

### 1.2 Description du domaine

#### Le domaine d'application

Le domaine choisi est la **cryptomonnaie et la finance de manière générale**. Les marchés de cryptomonnaies sont des marchés extrêmement volatils, ouverts 24h/24 et 7j/7, qui génèrent en continu d'énormes volumes de données temporelles : prix de transaction, volumes d'échanges, capitalisation, etc. C'est donc un terrain idéal pour l'analyse de séries temporelles.

Comparer plusieurs cryptomonnaies sur un même graphique présente un intérêt concret :

- **Comparer les performances** : quelles monnaies ont le mieux performé sur une période donnée ?
- **Observer les corrélations** : les grandes cryptomonnaies (Bitcoin, Ethereum) évoluent souvent de manière similaire ; visualiser plusieurs courbes ensemble permet de repérer ces tendances communes, ainsi que les divergences.
- **Analyser la volatilité** : certaines monnaies sont beaucoup plus volatiles que d'autres, ce qui apparaît clairement lorsqu'on superpose leurs courbes.

#### Les séries de données choisies

Conformément au cahier des charges (au minimum 5 séries cohérentes de 500 valeurs chacune), le projet s'appuie sur les séries suivantes :

| Série | Description | Pourquoi la comparer aux autres ? |
|:---|:---|:---|
| BTC/USDT | Bitcoin contre dollar | Référence du marché, première capitalisation |
| ETH/USDT | Ethereum contre dollar | Deuxième capitalisation, fortement corrélée au BTC |
| BNB/USDT | Binance Coin contre dollar | Monnaie d'échange majeure, liée à l'écosystème Binance |
| SOL/USDT | Solana contre dollar | Alternative technique à Ethereum, dynamique différente |
| XRP/USDT | Ripple contre dollar | Cas d'usage orienté paiements, évolution parfois décorrélée |

Ces cinq séries sont **cohérentes** entre elles : ce sont toutes des paires de cryptomonnaies cotées contre le même actif de référence (l'USDT, stablecoin adossé au dollar), ce qui permet de les comparer directement sur un axe temporel commun sans conversion. L'historique de cours de chacune fournit largement plus de 500 valeurs (données quotidiennes sur plusieurs années, ou données horaires sur quelques semaines).

#### Les sources de données

La source de données principale est l'**API publique de Binance** (`https://api.binance.com`), et plus spécifiquement le endpoint des **klines (chandeliers)**, qui renvoie pour une paire donnée l'historique des cours : prix d'ouverture, de clôture, plus haut, plus bas, et volume, pour un intervalle de temps donné (1h, 1 jour, etc.). Cette API est gratuite, ne nécessite pas d'authentification pour les données publiques, et renvoie les données au format JSON.

Les données récupérées sont ensuite **stockées localement au format JSON**, ce qui permet à l'utilisateur de consulter ses séries hors connexion, et d'y importer de nouvelles valeurs depuis des fichiers CSV ou JSON conformément au cahier des charges.


---

## 2. Analyse fonctionnelle

<!-- Description des fonctionnalités, cas d'utilisation, besoins fonctionnels et non-fonctionnels -->

*À compléter.*

---

## 3. Planification initiale

### 3.1 Méthodologie choisie

Nous adoptons une approche **agile** adaptée à la taille du projet et à l'équipe :

- **Sprints d'une semaine** : chaque sprint se termine par un point d'avancement et une démo interne.
- **Kanban** pour visualiser l'avancement.
- **Revue hebdomadaire** : comparaison de l'avancement réel avec le planning, réajustement des priorités si nécessaire.
- **Versioning Git** : une branche par fonctionnalité, merge après validation, commits réguliers et explicites.

### 3.2 Répartition des tâches

| Rôle | Membre | Responsabilités |
|:---|:---|:---|
| Développeur back-end / données | *À compléter* | Connexion à l'API Binance, parsing, stockage JSON, LINQ |
| Développeur front-end / GUI | *À compléter* | Interface WinForms, intégration ScottPlot, affichage des courbes |
| Tests & documentation | *Binôme* | Tests unitaires, rapport, journal de travail |


### 3.3 Planning prévisionnel

| Sprint | Période | Tâches | Livrable attendu | Statut |
|:---:|:---|:---|:---|:---:|
| Sprint 0 | 24 août | Analyse du besoin, rédaction des user stories, maquettes | Cahier des charges fonctionnel validé | ⬜ |
| Sprint 1 | 31 août | Mise en place du projet (Git, structure C#), premier GUI WinForms | Fenêtre principale avec graphique vide | ⬜ |
| Sprint 2 | 7 septembre | Connexion à l'API Binance, parsing des données | Affichage des premières vraies données | ⬜ |
| Sprint 3 | 14 septembre | Stockage local JSON, importation de nouvelles données (CSV/JSON) | Persistance des données fonctionnelle | ⬜ |
| Sprint 4 | 21 septembre | Affichage multi-séries, sélection des séries, flexibilité d'affichage (zoom, axes) | Graphique multi-courbes interactif | ⬜ |
| Sprint 5 | 28 septembre | Refactoring LINQ, extensions C#, optimisation | Code conforme aux contraintes techniques | ⬜ |
| Sprint 6 | 5 octobre | Tests unitaires (≥ 3 significatifs), corrections de bugs | Rapport de tests | ⬜ |
| Sprint 7 | 12 octobre | Finitions, UI, documentation du code | Version candidate | ⬜ |
| Sprint 8 | 19–30 octobre | Rapport final, bilan, préparation de la release GitHub | Livraison finale | ⬜ |

> **Légende des statuts :**
> - ⬜ Non commencé
> - 🟡 En cours
> - ✅ Terminé
> - ❌ Bloqué

### 3.5 Estimation du temps

Le projet représente **24 périodes** de travail. Sur la base d'un sprint par semaine sur ~9 semaines, cela correspond à environ **2 à 3 périodes par semaine** par membre, hors travail personnel. Une marge de sécurité est intégrée dans les sprints 7 et 8 pour absorber les imprévus.

### 3.6 Gestion des risques

| Risque | Impact | Mesure préventive / corrective |
|:---|:---:|:---|
| Indisponibilité ou changement de l'API Binance | 🔴 | Mettre en cache les données, prévoir une source de secours (données fictives) |
| Retard sur un sprint | 🟠 | Revue hebdomadaire, réajustement des priorités, marge en fin de planning |
| Difficultés avec ScottPlot / WinForms | 🟠 | Montée en compétence en début de projet (Sprint 1), documentation officielle |
| Perte de données / code | 🔴 | Git avec push régulier, sauvegarde des données JSON |



## 4. Rapport de tests

<!-- Stratégie de tests, résultats, couverture, bugs trouvés et corrigés -->

### 4.1 Stratégie de tests

*À compléter.*

### 4.2 Résultats des tests

*À compléter.*

### 4.3 Bugs identifiés et corrections

| Bug | Sévérité | Résolu ? |
|:---|:---:|:---:|
| *À compléter* | | |

> **Légende des sévérités :**
> - 🟢 Mineur
> - 🟠 Moyen
> - 🔴 Critique
>
> **Légende des statuts :**
> - ❌ Non résolu
> - ✅ Résolu

---

## 5. Usage de l'intelligence artificielle dans le projet

|utilisation|Model|
|:---|---|
|L'intelligence artificielle m'a été utile pour corriger l'orthographe dans le projet|Xiaomi MiMo-2.5|

---

## 6. Bilan du déroulement du projet

### 6.1 Planning respecté ?

<!-- Comparer le planning initial avec ce qui a réellement été fait -->

*À compléter.*

### 6.2 Méthodologie

<!-- Méthode utilisée (agile, cascade, etc.), outils de suivi, organisation d'équipe -->

*À compléter.*

### 6.3 Problèmes rencontrés

<!-- Difficultés techniques, organisationnelles, et comment elles ont été surmontées -->

*À compléter.*

---

## 7. Bilan produit

<!-- Ce qu'on a réellement vs ce qu'on voulait initialement -->

| Fonctionnalité | Prévue ? | Réalisée ? | Commentaire |
|:---|:---:|:---:|:---|
| *À compléter* | | | |

---

## 8. Conclusion

<!-- Synthèse du projet, compétences acquises, perspectives d'amélioration -->

*À compléter.*

---

> *Ce rapport sera complété au fil de l'avancement du projet.*
