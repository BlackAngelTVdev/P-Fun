using System.Globalization;
using P_Fun.Models;

namespace P_Fun.Core
{
    /// <summary>
    /// Transformations d'une série : mêmes entrées, mêmes sorties, aucun effet
    /// de bord et jamais de modification de la série reçue (le modèle est un
    /// record immuable). Comme rien ici ne touche à l'interface ni au disque,
    /// ces méthodes se testent sans lancer l'application.
    /// </summary>
    public static class PriceSeriesExtensions
    {
        /// <summary>
        /// Valeur de référence de la base 100 : la première clôture non nulle de
        /// la série. C'est elle qui permet de revenir au prix réel
        /// (prix = indice / 100 × référence).
        /// </summary>
        public static double Base100Reference(this PriceSeries series) =>
            series.Closes.FirstOrDefault(close => close > 0);

        /// <summary>
        /// Même série ramenée à une base 100 : la première clôture vaut 100 et
        /// les suivantes s'expriment en pourcentage de celle-ci. C'est ce qui
        /// rend comparables des actifs dont les prix n'ont pas la même échelle
        /// (BTC à 60 000 et ETH à 2 500 deviennent superposables).
        /// </summary>
        public static PriceSeries NormalizedToBase100(this PriceSeries series)
        {
            double reference = series.Base100Reference();
            if (reference == 0)
            {
                return series;
            }

            return series with { Closes = [.. series.Closes.Select(close => close / reference * 100)] };
        }

        /// <summary>
        /// Variation en pourcentage de la valeur d'indice <paramref name="index"/>
        /// par rapport à la référence de la série : 0 au début, +5 pour +5 %.
        /// </summary>
        public static double VariationPercent(this PriceSeries series, int index)
        {
            double reference = series.Base100Reference();
            if (reference == 0)
            {
                return 0;
            }

            return (series.Closes[index] / reference - 1) * 100;
        }

        /// <summary>
        /// Libellé de la légende. En base 100 on rappelle la valeur de référence
        /// de la série, sans quoi le prix réel n'est plus lisible sur le
        /// graphique.
        /// </summary>
        public static string LegendLabel(this PriceSeries series, bool base100) =>
            base100 && series.Base100Reference() != 0
                ? $"{series.Name} (réf. {series.Base100Reference().ToString("0.##", CultureInfo.InvariantCulture)})"
                : series.Name;
    }
}
