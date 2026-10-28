using P_Fun.Models;

namespace P_Fun.Core
{
    /// <summary>
    /// Une série telle qu'elle est réellement dessinée : les abscisses (dates
    /// converties en OADate) et les ordonnées tracées, qui sont des indices en
    /// base 100 ou des prix selon le mode d'affichage. La série d'origine est
    /// conservée à côté pour pouvoir afficher les vraies valeurs.
    /// </summary>
    /// <param name="OnRightAxis">
    /// Vrai si la série est tracée sur l'axe vertical de droite. Les deux axes
    /// n'ont pas les mêmes limites : le survol doit projeter chaque série avec
    /// l'échelle qui lui correspond.
    /// </param>
    public sealed record PlottedSeries(PriceSeries Series, double[] Xs, double[] Values, bool OnRightAxis = false);

    /// <summary>Une bougie d'une série tracée, identifiée par sa position dans les tableaux.</summary>
    public sealed record HoveredPoint(PlottedSeries Plot, int Index);
}
