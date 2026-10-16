# Questions de compréhension – Damien Rochat

> Pour l'entretien oral. Chaque question porte sur un passage technique du code commité. La section « Éléments attendus » est destinée à l'enseignant : la retirer avant de transmettre le fichier à l'élève.
> Contexte : le commit `048e13d`, qui a introduit ce code, est co-signé par l'agent IA _Codebuff_.

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
