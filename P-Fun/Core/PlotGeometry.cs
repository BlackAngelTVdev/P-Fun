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

            // SelectMany à plat toutes les séries : on obtient une seule suite de
            // candidats, puis MinBy garde le plus proche. Aucune boucle explicite.
            HoverCandidate? nearest = plotted
                .SelectMany(plot => plot.Xs.Select((x, index) => new HoverCandidate(
                    plot,
                    index,
                    area.ToPixel(range, x, plot.Values[index]).Distance(mouse))))
                .Where(candidate => candidate.Distance <= radius)
                .MinBy(candidate => candidate.Distance);

            return nearest is null ? null : new HoveredPoint(nearest.Plot, nearest.Index);
        }

        /// <summary>Un point candidat et sa distance au curseur, en pixels.</summary>
        private sealed record HoverCandidate(PlottedSeries Plot, int Index, double Distance);
    }
}
