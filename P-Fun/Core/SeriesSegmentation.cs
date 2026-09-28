namespace P_Fun.Core
{
    /// <summary>
    /// Un bloc contigu de bougies, exprimé en indices : <paramref name="Start"/>
    /// est inclus et <paramref name="End"/> est exclu, comme une plage.
    /// </summary>
    public readonly record struct SeriesSegment(int Start, int Length)
    {
        public int End => Start + Length;
    }

    /// <summary>
    /// Découpage d'une série en blocs contigus. Un trou dans les données ne doit
    /// pas être traversé par un trait : la courbe semblerait alors évoluer
    /// régulièrement entre deux bougies éloignées, alors qu'il n'y a rien entre
    /// elles. Chaque bloc est donc dessiné à part.
    /// Aucune dépendance à l'interface ni à ScottPlot : le calcul se teste seul.
    /// </summary>
    public static class SeriesSegmentation
    {
        /// <summary>
        /// Intervalle supposé quand la série n'est pas assez longue pour être
        /// mesurée : une minute, l'intervalle des données Binance du projet.
        /// </summary>
        public const long DefaultIntervalMilliseconds = 60_000;

        /// <summary>
        /// Un écart sépare deux blocs dès qu'il dépasse une fois et demie
        /// l'intervalle courant : sur des bougies à la minute, une seule minute
        /// manquante (60 000 → 120 000) suffit donc à couper le trait, tandis
        /// qu'un décalage d'une seconde ne le coupe pas.
        /// </summary>
        private const double GapTolerance = 1.5;

        /// <summary>
        /// Blocs contigus de la série : un seul si toutes les bougies se suivent,
        /// un bloc de plus à chaque trou. Les blocs couvrent la série entière,
        /// dans l'ordre, sans se recouvrir.
        /// </summary>
        public static IReadOnlyList<SeriesSegment> SplitOnGaps(double[] timestamps)
        {
            if (timestamps.Length == 0)
            {
                return [];
            }

            long maxGap = MaxGap(timestamps);

            // Le premier bloc commence toujours à l'indice 0 ; chaque écart trop
            // grand ouvre un bloc de plus à l'indice suivant.
            int[] starts = [0, .. Enumerable
                .Range(0, timestamps.Length - 1)
                .Where(index => timestamps[index + 1] - timestamps[index] > maxGap)
                .Select(index => index + 1)];

            // Chaque début de bloc court jusqu'au début du suivant, ou jusqu'à la
            // fin de la série pour le dernier.
            return [.. starts.Select((start, order) => new SeriesSegment(
                start,
                (order + 1 < starts.Length ? starts[order + 1] : timestamps.Length) - start))];
        }

        /// <summary>
        /// Intervalle courant de la série : l'écart entre deux bougies voisines le
        /// plus fréquent. Le mesurer sur les données plutôt que de le fixer à
        /// 60 000 ms garde le découpage correct si les bougies changent
        /// d'intervalle (5 minutes, 1 heure…).
        /// </summary>
        public static long NominalInterval(double[] timestamps)
        {
            double[] gaps = [.. timestamps
                .Zip(timestamps.Skip(1), (previous, next) => next - previous)
                .Where(gap => gap > 0)];

            return gaps.Length == 0
                ? DefaultIntervalMilliseconds
                : (long)gaps
                    .GroupBy(gap => gap)
                    .OrderByDescending(group => group.Count())
                    .First()
                    .Key;
        }

        private static long MaxGap(double[] timestamps) => (long)(NominalInterval(timestamps) * GapTolerance);
    }
}
