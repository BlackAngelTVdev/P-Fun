using System.Drawing;
using P_Fun.Models;

namespace P_Fun.Extensions
{
    /// <summary>
    /// Extensions d'interface : ce qui touche aux contrôles WinForms reste ici,
    /// les calculs sur les séries sont dans <see cref="Core.PriceSeriesExtensions"/>.
    /// </summary>
    public static class SeriesExtensions
    {
        /// <summary>
        /// Case à cocher de la série, libellée avec son nombre d'entrées : « BTC (1000) ».
        /// La série est conservée dans le Tag, donc la sélection reste fiable même
        /// si le libellé change et sans dépendre du nom de la série. L'action
        /// fournie est branchée ici pour que l'appelant n'ait pas à repasser sur
        /// les cases une fois construites.
        /// </summary>
        public static CheckBox ToCheckBox(this PriceSeries series, int index, Action? onCheckedChanged = null)
        {
            CheckBox checkBox = new()
            {
                Text = $"{series.Name} ({series.Closes.Length})",
                Tag = series,
                Location = new Point(20, 20 + index * 30),
                AutoSize = true,
                Checked = true,
                UseVisualStyleBackColor = true,
            };

            if (onCheckedChanged is not null)
            {
                checkBox.CheckedChanged += (_, _) => onCheckedChanged();
            }

            return checkBox;
        }
    }
}
