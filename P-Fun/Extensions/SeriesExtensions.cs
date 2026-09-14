using System.Drawing;
using P_Fun.Models;

namespace P_Fun.Extensions
{
    public static class SeriesExtensions
    {
        /// <summary>
        /// Case à cocher de la série, libellée avec son nombre d'entrées : « BTC (1000) ».
        /// La série est conservée dans le Tag, donc la sélection reste fiable même
        /// si le libellé change.
        /// </summary>
        public static CheckBox ToCheckBox(this PriceSeries series, int index) => new()
        {
            Text = $"{series.Name} ({series.Closes.Length})",
            Tag = series,
            Location = new Point(20, 20 + index * 30),
            AutoSize = true,
            Checked = true,
            UseVisualStyleBackColor = true,
        };
    }
}
