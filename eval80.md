# Évaluation intermédiaire (80 %) – Damien Rochat

> Mise à jour du 16.10.2026 : les méthodes d'extension ne sont **pas exigées** (notion non vue en cours), même si le CdC les mentionne.

---

## En bref

Projet **techniquement très abouti** : architecture en couches (Core / Data / Models / Extensions), LINQ systématique (aucune boucle impérative), base SQLite avec upsert, mode « base 100 », 5 tests pertinents. Le rapport est presque complet. **Les points d'attention portent sur la cohérence de la documentation et la transparence sur l'usage de l'IA**, pas sur le produit.

## Axes de travail prioritaires pour la finalisation

1. **Réécrire la section IA** de façon exhaustive et factuelle : outils réellement utilisés (y compris Codebuff), mode (agent, chat…), 2 ou 3 exemples concrets (prompt, résultat, corrections). Supprimer les phrases sur ce qui n'a pas été fait.
2. **Corriger les incohérences du rapport** (liste des séries, cohérence, stockage SQLite au lieu de JSON dans l'introduction), ajouter les maquettes et compléter conclusion et bilan du déroulement.
3. **Fournir un jeu de données d'exemple** utilisable sans Python (dans le dépôt si petit, sinon en asset de release) et documenter la procédure.
4. Terminer ou abandonner proprement `feature/double-echelle` (fusion ou mention dans le bilan).
