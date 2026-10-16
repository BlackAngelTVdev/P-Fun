# Questions de compréhension – Damien Rochat

> Pour l'entretien oral. Chaque question porte sur un passage technique du code commité. La section « Éléments attendus » est destinée à l'enseignant : la retirer avant de transmettre le fichier à l'élève.
> Contexte : le commit `048e13d`, qui a introduit ce code, est co-signé par l'agent IA *Codebuff*.

---

## Question 1 – Recherche du point survolé par dichotomie

**Passage** : `P-Fun/Core/PlotGeometry.cs`, lignes 86-129

```csharp
double unitsPerPixel = (range.XMax - range.XMin) / area.Width;
double margin = radius * unitsPerPixel;
double center = range.XMin + ((mouse.X - area.Left) / area.Width) * (range.XMax - range.XMin);

int first = Math.Max(0, IndexAtLeast(xs, center - margin));
int last = Math.Min(xs.Length - 1, IndexAtMost(xs, center + margin));
...
private static int IndexAtLeast(double[] sortedXs, double value)
{
    int found = Array.BinarySearch(sortedXs, value);
    return found >= 0 ? found : ~found;
}
private static int IndexAtMost(double[] sortedXs, double value)
{
    int found = Array.BinarySearch(sortedXs, value);
    return found >= 0 ? found : ~found - 1;
}
```

**Question** : Que retourne `Array.BinarySearch` quand la valeur n'est **pas** dans le tableau ? Que signifie l'opérateur `~` ici ? Avec `xs = [10, 20, 30, 40]`, donnez le résultat de `IndexAtLeast(xs, 25)` et de `IndexAtMost(xs, 25)` en détaillant le calcul. Pourquoi ce code exige-t-il que `xs` soit trié, et qu'est-ce qu'on gagne par rapport à un simple `Where` sur toute la série ?

**Relances possibles**
- Pourquoi convertir le rayon de pixels en unités de données (`unitsPerPixel`) ?
- Pourquoi filtrer encore avec `Where(candidate => candidate.Distance <= radius)` après avoir réduit la plage ?
- Que vaut `IndexAtMost` pour une valeur plus petite que tous les éléments, et pourquoi le `Math.Max(..., 0)` est-il nécessaire ?

**Éléments attendus d'une réponse convaincante**
- Si la valeur est absente, `BinarySearch` retourne le complément binaire (`~index`, un nombre négatif) de l'indice du premier élément plus grand. `~` inverse les bits, donc `~(~i) == i`.
- Pour 25 : `BinarySearch` retourne `~2` = -3. `IndexAtLeast` donne `~(-3)` = 2 (valeur 30, premier élément ≥ 25). `IndexAtMost` donne 2 - 1 = 1 (valeur 20, dernier élément ≤ 25).
- La dichotomie n'est correcte que sur un tableau trié. Elle coûte O(log n) au lieu de O(n) : avec ~180 000 bougies et un appel à chaque mouvement de souris, c'est décisif.
- La plage ne filtre que sur X (une bande verticale). La vraie distance euclidienne en pixels, qui tient compte de Y, se vérifie ensuite.

---

## Question 2 – Découpage d'une série sur les trous

**Passage** : `P-Fun/Core/SeriesSegmentation.cs`, lignes 40-84

```csharp
int[] starts = [0, .. Enumerable
    .Range(0, timestamps.Length - 1)
    .Where(index => timestamps[index + 1] - timestamps[index] > maxGap)
    .Select(index => index + 1)];

return [.. starts.Select((start, order) => new SeriesSegment(
    start,
    (order + 1 < starts.Length ? starts[order + 1] : timestamps.Length) - start))];
...
return gaps.Length == 0
    ? DefaultIntervalMilliseconds
    : (long)gaps.GroupBy(gap => gap).OrderByDescending(group => group.Count()).First().Key;
```

**Question** : Calculez à la main le résultat de `SplitOnGaps` pour les horodatages (en ms) `[0, 60000, 120000, 300000, 360000]`. Donnez l'intervalle nominal, le `maxGap`, le tableau `starts` et la liste des segments. Expliquez le rôle de la syntaxe `[0, .. ]`, et celui du second paramètre `order` dans `Select`.

**Relances possibles**
- Pourquoi prendre l'écart **le plus fréquent** et pas la moyenne des écarts ?
- Pourquoi un facteur de 1,5 et pas simplement `> nominal` ?
- Est-il sûr de faire un `GroupBy` sur des `double` ?

**Éléments attendus d'une réponse convaincante**
- Écarts : 60000, 60000, 180000, 60000. Le mode vaut 60000, donc `maxGap` = 90000.
- Seul l'indice 2 (écart 180000) dépasse le seuil : `starts` = [0, 3]. Segments : (Start 0, Length 3) et (Start 3, Length 2).
- `[0, .. seq]` est une expression de collection (C# 12) : elle crée un tableau qui contient 0 suivi de tous les éléments de `seq` (*spread*).
- `Select((x, i) => …)` fournit l'indice. Il sert ici à lire le début suivant `starts[order + 1]` ; le dernier segment va jusqu'à la fin.
- La moyenne serait faussée par les trous eux-mêmes. Le mode est robuste.
- Le facteur 1,5 tolère une petite gigue sans couper la courbe.
- Le `GroupBy` sur `double` fonctionne parce que les valeurs sont des millisecondes entières, représentées exactement ; ce serait fragile avec des valeurs fractionnaires.
