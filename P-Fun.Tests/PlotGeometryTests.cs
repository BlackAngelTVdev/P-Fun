using P_Fun.Core;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests de la géométrie du graphique : conversion données → pixels et
    /// recherche du point survolé. C'est ce qui alimente l'infobulle.
    /// </summary>
    public class PlotGeometryTests
    {
        /// <summary>Zone de dessin de 100 × 200 pixels.</summary>
        private static readonly ChartArea Area = new(Left: 0, Bottom: 200, Width: 100, Height: 200);

        /// <summary>Domaine affiché : X de 0 à 10, Y de 0 à 100.</summary>
        private static readonly AxisRange Range = new(XMin: 0, XMax: 10, YMin: 0, YMax: 100);

        [Fact]
        public void ToPixel_PlaceLesBornesDuDomaineSurLesCoinsDeLaZone()
        {
            Assert.Equal(new PixelPoint(0, 200), Area.ToPixel(Range, 0, 0));
            Assert.Equal(new PixelPoint(100, 0), Area.ToPixel(Range, 10, 100));
        }

        [Fact]
        public void ToPixel_PlaceLeMilieuDuDomaineAuCentreDeLaZone()
        {
            Assert.Equal(new PixelPoint(50, 100), Area.ToPixel(Range, 5, 50));
        }

        [Fact]
        public void FindNearest_RetrouveLePointSousLeCurseur()
        {
            PlottedSeries plot = Plot("BTC", [1, 5, 9], [10, 50, 90]);

            HoveredPoint? found = HoverSearch.FindNearest([plot], Area, Range, 51, 102, radius: 12);

            Assert.NotNull(found);
            Assert.Equal(1, found!.Index);
        }

        [Fact]
        public void FindNearest_RetourneNullQuandLeCurseurEstLoinDeTouteCourbe()
        {
            PlottedSeries plot = Plot("BTC", [1], [90]);

            HoveredPoint? found = HoverSearch.FindNearest([plot], Area, Range, 100, 200, radius: 12);

            Assert.Null(found);
        }

        [Fact]
        public void FindNearest_ChoisitLaSerieLaPlusProcheDuCurseur()
        {
            PlottedSeries basse = Plot("basse", [5], [10]);
            PlottedSeries haute = Plot("haute", [5], [90]);

            HoveredPoint? found = HoverSearch.FindNearest([basse, haute], Area, Range, 50, 20, radius: 12);

            Assert.NotNull(found);
            Assert.Equal("haute", found!.Plot.Series.Name);
        }

        [Fact]
        public void FindNearest_IgnoreLesSeriesVides()
        {
            PlottedSeries vide = new(TestSeries.Create("vide"), [], []);

            HoveredPoint? found = HoverSearch.FindNearest([vide], Area, Range, 50, 100, radius: 12);

            Assert.Null(found);
        }

        [Fact]
        public void FindNearest_AvecDesLimitesDegenerees_RetourneNull()
        {
            PlottedSeries plot = Plot("BTC", [5], [50]);
            AxisRange degenere = new(XMin: 5, XMax: 5, YMin: 0, YMax: 100);

            Assert.Null(HoverSearch.FindNearest([plot], Area, degenere, 50, 100, radius: 12));
        }

        [Fact]
        public void FindNearest_EnDehorsDeLaZoneDeDessin_RetourneNull()
        {
            PlottedSeries plot = Plot("BTC", [5], [50]);

            Assert.Null(HoverSearch.FindNearest([plot], Area, Range, pixelX: -5, pixelY: 100, radius: 12));
        }

        [Fact]
        public void FindNearest_SurUneGrandeSerie_RetrouveLePointVise()
        {
            // 20 000 points : la taille des séries réelles en 1 minute. La recherche
            // doit rester exacte malgré la fenêtre réduite autour du curseur.
            double[] values = [.. Enumerable.Range(0, 20_000).Select(index => 100 + (index % 500))];
            double[] xs = [.. Enumerable.Range(0, 20_000).Select(index => (double)index)];
            PlottedSeries plot = new(TestSeries.Create("BTC", values), xs, values);

            ChartArea area = new(Left: 0, Bottom: 200, Width: 1000, Height: 200);
            AxisRange range = new(XMin: 0, XMax: 20_000, YMin: 0, YMax: 1000);

            const int index = 12_345;
            PixelPoint target = area.ToPixel(range, xs[index], values[index]);

            HoveredPoint? found = HoverSearch.FindNearest([plot], area, range, target.X, target.Y, radius: 5);

            Assert.NotNull(found);
            Assert.Equal(index, found!.Index);
        }

        [Fact]
        public void FindNearest_AuxDeuxBoutsDeLaSerie_RetrouveLePointVise()
        {
            double[] values = [.. Enumerable.Range(0, 20_000).Select(index => 100 + (index % 500))];
            double[] xs = [.. Enumerable.Range(0, 20_000).Select(index => (double)index)];
            PlottedSeries plot = new(TestSeries.Create("BTC", values), xs, values);

            ChartArea area = new(Left: 0, Bottom: 200, Width: 1000, Height: 200);
            AxisRange range = new(XMin: 0, XMax: 20_000, YMin: 0, YMax: 1000);

            foreach (int index in new[] { 0, 19_999 })
            {
                PixelPoint target = area.ToPixel(range, xs[index], values[index]);
                HoveredPoint? found = HoverSearch.FindNearest([plot], area, range, target.X, target.Y, radius: 5);

                Assert.NotNull(found);
                Assert.Equal(index, found!.Index);
            }
        }

        private static PlottedSeries Plot(string name, double[] xs, double[] values) =>
            new(TestSeries.Create(name, values), xs, values);
    }
}
