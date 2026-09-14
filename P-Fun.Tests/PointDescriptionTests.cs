using P_Fun.Core;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests du contenu de l'infobulle : elle doit toujours montrer le prix réel,
    /// même quand la courbe est dessinée en base 100.
    /// </summary>
    public class PointDescriptionTests
    {
        /// <summary>Série BTC : 60 000 puis 61 800 (+3 %).</summary>
        private static readonly HoveredPoint Point = new(
            new PlottedSeries(TestSeries.Create("BTC", 60000, 61800), [0, 1], [100, 103]),
            Index: 1);

        [Fact]
        public void Describe_AfficheLePrixReelMemeEnBase100()
        {
            string text = PointDescription.Describe(Point, base100: true);

            Assert.Contains("61800 USDT", text);
        }

        [Fact]
        public void Describe_MentionneLIndiceUniquementEnBase100()
        {
            Assert.Contains("indice 103", PointDescription.Describe(Point, base100: true));
            Assert.DoesNotContain("indice", PointDescription.Describe(Point, base100: false));
        }

        [Fact]
        public void Describe_IndiqueLaVariationDepuisLeDebutDeLaSerie()
        {
            string text = PointDescription.Describe(Point, base100: false);

            Assert.Contains("+3.00 % depuis le début", text);
        }

        [Fact]
        public void Describe_CommenceParLeNomEtLaDateAuFormatJourMoisAnnee()
        {
            string premiereLigne = PointDescription.Describe(Point, base100: false)
                .Split(Environment.NewLine)[0];

            Assert.StartsWith("BTC — ", premiereLigne);
            Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}$", premiereLigne);
        }
    }
}
