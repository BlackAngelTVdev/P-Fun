using P_Fun.Core;
using P_Fun.Models;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests de la normalisation en base 100 et des valeurs dérivées : c'est
    /// elle qui rend comparables des séries d'échelles différentes.
    /// </summary>
    public class PriceSeriesExtensionsTests
    {
        [Fact]
        public void NormalizeToBase100_PlaceLaPremiereClotureA100()
        {
            PriceSeries series = TestSeries.Create("BTC", 60000, 61800, 59400);

            PriceSeries normalized = series.NormalizedToBase100();

            Assert.Equal(100d, normalized.Closes[0], 6);
        }

        [Fact]
        public void NormalizeToBase100_ConserveLesProportionsEntreClotures()
        {
            PriceSeries series = TestSeries.Create("BTC", 200, 100, 300);
            double[] expected = [100, 50, 150];

            IEnumerable<double> actual = series.NormalizedToBase100().Closes.Select(value => Math.Round(value, 6));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NormalizeToBase100_NeModifiePasLaSerieDOrigine()
        {
            PriceSeries series = TestSeries.Create("BTC", 200, 100);

            _ = series.NormalizedToBase100();

            Assert.Equal([200d, 100d], series.Closes);
        }

        [Fact]
        public void Base100Reference_IgnoreLesCloturesNullesDuDebut()
        {
            PriceSeries series = TestSeries.Create("BTC", 0, 0, 400, 500);

            Assert.Equal(400d, series.Base100Reference(), 6);
        }

        [Fact]
        public void Base100Reference_SurUneSerieVideOuNulle_RetourneZero()
        {
            PriceSeries series = TestSeries.Create("BTC", 0, 0);

            Assert.Equal(0d, series.Base100Reference(), 6);
            Assert.Equal([0d, 0d], series.NormalizedToBase100().Closes);
        }

        [Fact]
        public void VariationPercent_ExprimeLaVariationDepuisLeDebut()
        {
            PriceSeries series = TestSeries.Create("BTC", 200, 220, 180);

            Assert.Equal(10d, series.VariationPercent(1), 6);
            Assert.Equal(-10d, series.VariationPercent(2), 6);
        }

        [Fact]
        public void LegendLabel_RappelleLaReferenceUniquementEnBase100()
        {
            PriceSeries series = TestSeries.Create("BTC", 60123.45, 61000);

            Assert.Equal("BTC (réf. 60123.45)", series.LegendLabel(base100: true));
            Assert.Equal("BTC", series.LegendLabel(base100: false));
        }
    }
}
