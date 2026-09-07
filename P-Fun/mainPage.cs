using P_Fun.Data;
using P_Fun.Extensions;
using P_Fun.Models;

namespace P_Fun
{
    public partial class mainPage : Form
    {
        public mainPage()
        {
            InitializeComponent();
            BuildSeriesCheckBoxes();
            PlotSampleData();
        }

        private void BuildSeriesCheckBoxes()
        {
            SeriesCatalog.All
                .Select((series, index) => series.ToCheckBox(index))
                .ToList()
                .ForEach(sidePanel.Controls.Add);
        }

        private void PlotSampleData()
        {
            double[] xs = Enumerable.Range(0, 100).Select(i => (double)i).ToArray();
            double[] ys = xs.Select(x => Math.Sin(x / 10)).ToArray();
            plotPanel.Plot.Add.Scatter(xs, ys);
            plotPanel.Plot.Title("Exemple X/Y");
            plotPanel.Refresh();
        }

        public IEnumerable<CryptoSeries> SelectedSeries =>
            SeriesCatalog.All.Selected(sidePanel.Controls.OfType<CheckBox>());
    }
}
