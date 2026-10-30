using P_Fun.Core;
using P_Fun.Models;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests du survol sur un graphique à deux échelles verticales. Chaque série
    /// est tracée dans son propre intervalle de prix : la même valeur verticale
    /// ne correspond donc pas au même pixel selon la série. Sans correction, le
    /// survol de la série de droite chercherait ses points à une hauteur calculée
    /// sur l'axe de gauche et n'afficherait jamais la bonne bougie.
    /// Aucune donnée réelle n'est utilisée : deux petites séries suffisent.
    /// </summary>
    public class HoverSearchTests
    {
        // Une série de gauche qui monte de 0 à 100 et une série de droite qui
        // descend de 1000 à 900 : mêmes abscisses, prix sans commune mesure.
        private static readonly double[] Xs = [1, 2, 3];
        private static readonly double[] LeftValues = [0, 50, 100];
        private static readonly double[] RightValues = [1000, 950, 900];

        // Zone de dessin : 300 px de large sur 100 px de haut. Un record struct
        // ne peut pas être const, d'où un champ statique en lecture seule.
        private static readonly ChartArea Area = new(0, 100, 300, 100);

        private static PlottedSeries Series(string name, double[] values, bool onRightAxis) =>
            new(new PriceSeries(name, Xs, values), Xs, values, onRightAxis);

        /// <summary>
        /// Un point de la série de droite doit être cherché avec les limites de
        /// l'axe de droite : le curseur posé exactement sur sa dernière bougie
        /// doit la retrouver.
        /// </summary>
        [Fact]
        public void FindNearest_ProjecteChaqueSerieAvecLesLimitesDeSonAxe()
        {
            List<PlottedSeries> plotted =
            [
                Series("BTC", LeftValues, onRightAxis: false),
                Series("ETH", RightValues, onRightAxis: true),
            ];

            ChartArea area = Area;
            AxisRange leftRange = new(Xs[0], Xs[2], 0, 100);
            AxisRange rightRange = new(Xs[0], Xs[2], 900, 1000);

            // Dernière bougie de chaque série, exactement à sa place à l'écran.
            PixelPoint leftPoint = area.ToPixel(leftRange, Xs[2], LeftValues[2]);
            PixelPoint rightPoint = area.ToPixel(rightRange, Xs[2], RightValues[2]);

            // L'abscisse est la même sur les deux axes : seule l'ordonnée change.
            Assert.Equal(leftPoint.X, rightPoint.X, 6);
            Assert.NotEqual(leftPoint.Y, rightPoint.Y, 6);

            HoveredPoint? onLeft = HoverSearch.FindNearest(plotted, area, leftRange, leftPoint.X, leftPoint.Y, 5);
            HoveredPoint? onRight = HoverSearch.FindNearest(
                plotted,
                area,
                plot => plot.OnRightAxis ? rightRange : leftRange,
                rightPoint.X,
                rightPoint.Y,
                5);

            Assert.Equal("BTC", onLeft?.Plot.Series.Name);
            Assert.Equal(2, onLeft?.Index);
            Assert.Equal("ETH", onRight?.Plot.Series.Name);
            Assert.Equal(2, onRight?.Index);
        }

        /// <summary>
        /// Sans le sélecteur de limites, la série de droite est introuvable : sa
        /// courbe est loin des pixels calculés sur l'axe de gauche. Le test verrouille
        /// que la correction est bien nécessaire, pas seulement aesthetics.
        /// </summary>
        [Fact]
        public void FindNearest_UneSeuleEchelleNeRetrouvePasLaSerieDeDroite()
        {
            List<PlottedSeries> plotted =
            [
                Series("BTC", LeftValues, onRightAxis: false),
                Series("ETH", RightValues, onRightAxis: true),
            ];

            ChartArea area = Area;
            AxisRange leftRange = new(Xs[0], Xs[2], 0, 100);
            AxisRange rightRange = new(Xs[0], Xs[2], 900, 1000);

            PixelPoint rightPoint = area.ToPixel(rightRange, Xs[2], RightValues[2]);

            // BTC est visible en haut (valeur 100 = bord haut), ETH en bas
            // (valeur 900 = bord bas) : les deux points sont aux extrémités
            // opposées de la zone, à la même abscisse.
            Assert.True(area.ToPixel(leftRange, Xs[2], LeftValues[2]).Y < 50);
            Assert.True(rightPoint.Y > 50);

            HoveredPoint? hovered = HoverSearch.FindNearest(plotted, area, leftRange, rightPoint.X, rightPoint.Y, 5);

            Assert.NotEqual("ETH", hovered?.Plot.Series.Name);
        }

        /// <summary>Un graphique vide ne doit pas faire planter le survol.</summary>
        [Fact]
        public void FindNearest_SansSerieRenvoieNull()
        {
            HoveredPoint? hovered = HoverSearch.FindNearest(
                Array.Empty<PlottedSeries>(),
                Area,
                _ => new AxisRange(1, 3, 0, 100),
                10,
                50,
                5);

            Assert.Null(hovered);
        }

        /// <summary>
        /// La surcharge historique (une seule échelle pour toutes les séries) doit
        /// rester valide : elle délègue à la variante à sélecteur.
        /// </summary>
        [Fact]
        public void FindNearest_UneSeuleEchelleFonctionneAvecLaSurchargeHistorique()
        {
            List<PlottedSeries> plotted = [Series("BTC", LeftValues, onRightAxis: false)];
            ChartArea area = Area;
            AxisRange range = new(Xs[0], Xs[2], 0, 100);

            PixelPoint target = area.ToPixel(range, Xs[1], LeftValues[1]);

            HoveredPoint? hovered = HoverSearch.FindNearest(plotted, area, range, target.X, target.Y, 5);

            Assert.Equal(1, hovered?.Index);
        }
    }
}