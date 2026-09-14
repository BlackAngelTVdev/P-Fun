using P_Fun.Models;

namespace P_Fun.Tests
{
    /// <summary>
    /// Fabrique de séries pour les tests : on ne donne que les clôtures, les
    /// horodatages sont générés automatiquement (une bougie par heure).
    /// </summary>
    internal static class TestSeries
    {
        /// <summary>Première bougie de toutes les séries de test.</summary>
        public static readonly DateTimeOffset Start = new(2026, 9, 14, 8, 0, 0, TimeSpan.Zero);

        public static PriceSeries Create(string name, params double[] closes) => new(
            name,
            [.. closes.Select((_, index) => (double)Start.AddHours(index).ToUnixTimeMilliseconds())],
            closes);
    }
}
