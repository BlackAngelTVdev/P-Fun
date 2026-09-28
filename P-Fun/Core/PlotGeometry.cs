namespace P_Fun.Core
{
    /// <summary>
    /// Zone de dessin du graphique, en pixels. Volontairement indépendante de
    /// ScottPlot : les coordonnées viennent de <c>PixelRect</c>, mais le calcul
    /// se teste avec de simples nombres.
    /// </summary>
    public readonly record struct ChartArea(double Left, double Bottom, double Width, double Height)
    {
        public double Right => Left + Width;

        public double Top => Bottom - Height;

        public bool Contains(double x, double y) => x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    /// <summary>Limites des axes, exprimées dans l'unité des données tracées.</summary>
    public readonly record struct AxisRange(double XMin, double XMax, double YMin, double YMax)
    {
        /// <summary>Vrai si les limites décrivent une zone non dégénérée.</summary>
        public bool IsValid => XMax > XMin && YMax > YMin;
    }

    /// <summary>Un point de l'écran, en pixels.</summary>
    public readonly record struct PixelPoint(double X, double Y);

    /// <summary>Conversion données ↔ pixels, dans les deux sens.</summary>
    public static class PlotProjection
    {
        /// <summary>Position en pixels d'un point exprimé dans l'unité des données.</summary>
        public static PixelPoint ToPixel(this ChartArea area, AxisRange range, double x, double y) => new(
            area.Left + (x - range.XMin) / (range.XMax - range.XMin) * area.Width,
            area.Bottom - (y - range.YMin) / (range.YMax - range.YMin) * area.Height);

        /// <summary>Distance euclidienne entre deux points de l'écran, en pixels.</summary>
        public static double Distance(this PixelPoint from, PixelPoint to) =>
            Math.Sqrt(((from.X - to.X) * (from.X - to.X)) + ((from.Y - to.Y) * (from.Y - to.Y)));
    }

    /// <summary>
    /// Recherche du point le plus proche du curseur. La comparaison se fait en
    /// pixels, donc le comportement reste le même quel que soit le zoom : il
    /// faut vraiment viser la courbe pour que l'infobulle réagisse.
    /// Les abscisses d'une série doivent être triées par ordre croissant, comme
    /// elles le sont dans une série tracée (triée par date).
    /// </summary>
    public static class HoverSearch
    {
        /// <summary>
        /// Point le plus proche du curseur, ou <c>null</c> s'il n'y en a aucun à
        /// moins de <paramref name="radius"/> pixels.
        /// </summary>
        public static HoveredPoint? FindNearest(
            IEnumerable<PlottedSeries> plotted,
            ChartArea area,
            AxisRange range,
            double pixelX,
            double pixelY,
            double radius)
        {
            if (!range.IsValid || area.Width <= 0 || area.Height <= 0 || !area.Contains(pixelX, pixelY))
            {
                return null;
            }

            PixelPoint mouse = new(pixelX, pixelY);

            // SelectMany à plat les candidats de toutes les séries : on obtient une
            // seule suite de points, puis MinBy garde le plus proche. Aucune boucle
            // explicite. Les candidats sont déjà limités à la fenêtre utile, sans
            // quoi il faudrait reprojeter les 20 000 points de chaque série à
            // chaque déplacement de la souris.
            HoverCandidate? nearest = plotted
                .SelectMany(plot => Candidates(plot, area, range, mouse, radius))
                .MinBy(candidate => candidate.Distance);

            return nearest is null ? null : new HoveredPoint(nearest.Plot, nearest.Index);
        }

        /// <summary>
        /// Points d'une série assez proches du curseur pour être candidats, c'est-à-dire
        /// ceux dont l'abscisse à l'écran tombe à moins de <paramref name="radius"/> pixels.
        /// Les abscisses étant triées par ordre croissant, ils forment une plage contiguë :
        /// on la trouve par dichotomie au lieu de parcourir toute la série.
        /// </summary>
        private static IEnumerable<HoverCandidate> Candidates(
            PlottedSeries plot,
            ChartArea area,
            AxisRange range,
            PixelPoint mouse,
            double radius)
        {
            double[] xs = plot.Xs;
            if (xs.Length == 0 || xs.Length != plot.Values.Length)
            {
                return [];
            }

            // Le rayon et la position du curseur ramenés dans l'unité des données
            // (l'échelle horizontale est linéaire, donc la conversion l'est aussi).
            double unitsPerPixel = (range.XMax - range.XMin) / area.Width;
            double margin = radius * unitsPerPixel;
            double center = range.XMin + ((mouse.X - area.Left) / area.Width) * (range.XMax - range.XMin);

            int first = Math.Max(0, IndexAtLeast(xs, center - margin));
            int last = Math.Min(xs.Length - 1, IndexAtMost(xs, center + margin));

            return Enumerable.Range(first, Math.Max(last - first + 1, 0))
                .Select(index => new HoverCandidate(
                    plot,
                    index,
                    area.ToPixel(range, xs[index], plot.Values[index]).Distance(mouse)))
                .Where(candidate => candidate.Distance <= radius);
        }

        /// <summary>Premier indice dont l'abscisse est supérieure ou égale à <paramref name="value"/>.</summary>
        private static int IndexAtLeast(double[] sortedXs, double value)
        {
            int found = Array.BinarySearch(sortedXs, value);
            return found >= 0 ? found : ~found;
        }

        /// <summary>Dernier indice dont l'abscisse est inférieure ou égale à <paramref name="value"/>.</summary>
        private static int IndexAtMost(double[] sortedXs, double value)
        {
            int found = Array.BinarySearch(sortedXs, value);
            return found >= 0 ? found : ~found - 1;
        }

        /// <summary>Un point candidat et sa distance au curseur, en pixels.</summary>
        private sealed record HoverCandidate(PlottedSeries Plot, int Index, double Distance);
    }
}
