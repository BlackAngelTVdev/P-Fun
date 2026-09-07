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
            BuildImportButton();
            PlotSampleData();
        }

        private void BuildSeriesCheckBoxes()
        {
            SeriesCatalog.All
                .Select((series, index) => series.ToCheckBox(index))
                .ToList()
                .ForEach(sidePanel.Controls.Add);
        }

        private void BuildImportButton()
        {
            var importButton = new Button
            {
                Text = "Importer un CSV…",
                Location = new Point(20, 20 + SeriesCatalog.All.Count * 30),
                Size = new Size(160, 30),
            };
            importButton.Click += (_, _) => ImportSeriesFromCsv();
            sidePanel.Controls.Add(importButton);
        }

        private void ImportSeriesFromCsv()
        {
            // WIP : le chemin en dur pointe vers un fichier qui n'existe pas encore.
            IReadOnlyList<CryptoSeries> imported = CsvSeriesImporter.Import("data/series.csv");
            SeriesCatalog.Add(imported);
            sidePanel.Controls.Clear();
            BuildSeriesCheckBoxes();
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
