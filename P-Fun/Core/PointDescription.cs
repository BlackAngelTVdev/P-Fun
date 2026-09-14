using System.Globalization;
using P_Fun.Models;

namespace P_Fun.Core
{
    /// <summary>
    /// Texte de l'infobulle affichée au survol : le prix réel de la bougie (dans
    /// son unité d'origine, même en base 100), sa date, sa variation depuis le
    /// début de la série et, en base 100, l'indice effectivement tracé à l'écran.
    /// </summary>
    public static class PointDescription
    {
        /// <summary>Unité des prix affichés (les séries du projet sont des paires crypto).</summary>
        public const string PriceUnit = "USDT";

        public static string Describe(HoveredPoint point, bool base100)
        {
            PriceSeries series = point.Plot.Series;
            double price = series.Closes[point.Index];

            DateTime time = DateTimeOffset
                .FromUnixTimeMilliseconds((long)series.Timestamps[point.Index])
                .LocalDateTime;

            string variation = series.Base100Reference() == 0
                ? string.Empty
                : $"{Format(series.VariationPercent(point.Index), "+0.00;-0.00;0.00")} % depuis le début";

            string index = base100
                ? $"indice {Format(point.Plot.Values[point.Index], "0.##")}"
                : string.Empty;

            string[] lines =
            [
                $"{series.Name} — {time.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}",
                $"{Format(price, "0.##")} {PriceUnit}",
                variation,
                index,
            ];

            return string.Join(Environment.NewLine, lines.Where(line => line.Length > 0));
        }

        /// <summary>
        /// Formatage en culture invariante : l'affichage de l'infobulle ne doit
        /// pas dépendre de la locale de la machine (les séries utilisent le point
        /// décimal, comme les fichiers JSON d'origine).
        /// </summary>
        private static string Format(double value, string format) =>
            value.ToString(format, CultureInfo.InvariantCulture);
    }
}
