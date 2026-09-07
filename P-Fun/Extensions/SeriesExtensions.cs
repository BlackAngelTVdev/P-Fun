using System.Drawing;
using P_Fun.Models;

namespace P_Fun.Extensions
{
    public static class SeriesExtensions
    {
        public static CheckBox ToCheckBox(this CryptoSeries series, int index) => new()
        {
            Text = series.Pair,
            Tag = series,
            Location = new Point(20, 20 + index * 30),
            AutoSize = true,
            Checked = true,
            UseVisualStyleBackColor = true,
        };

        public static IEnumerable<CryptoSeries> Selected(this IEnumerable<CryptoSeries> all, IEnumerable<CheckBox> checkBoxes)
        {
            HashSet<string> pairs = checkBoxes
                .Where(cb => cb.Checked)
                .Select(cb => cb.Text)
                .ToHashSet();

            return all.Where(series => pairs.Contains(series.Pair));
        }
    }
}