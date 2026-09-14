using P_Fun.Data;
using P_Fun.Extensions;
using P_Fun.Models;

namespace P_Fun
{
    public partial class mainPage : Form
    {
        private readonly List<PriceSeries> _importedSeries = [];
        private readonly List<string> _skippedFiles = [];
        private string _importedFolder = string.Empty;
        private Label? _renderLabel;

        public mainPage()
        {
            InitializeComponent();
            ConfigurePlot();

            // Au démarrage on charge directement le dossier "data" du projet.
            ImportJsonFolder(DefaultDataFolder(), notify: false);
        }

        /// <summary>
        /// Axe du bas en dates/heures (ScottPlot attend des doubles OADate) et
        /// axes nommés pour savoir où on se situe sur le graphique.
        /// </summary>
        private void ConfigurePlot()
        {
            plotPanel.Plot.Axes.DateTimeTicksBottom();
            plotPanel.Plot.Axes.Bottom.Label.Text = "Temps";
            plotPanel.Plot.Axes.Left.Label.Text = "Prix (USDT)";
            HideBenchmark();
        }

        /// <summary>
        /// ScottPlot dessine en bas à gauche du graphique un encadré jaune
        /// « Rendered in … ms » (son benchmark). On le masque : l'information est
        /// affichée dans le panneau latéral, sous la liste des séries importées.
        /// </summary>
        private void HideBenchmark() => plotPanel.Plot.Benchmark.IsVisible = false;

        /// <summary>
        /// Reconstruit le panneau latéral : une case à cocher par série importée,
        /// le bouton d'import, puis les informations d'état et de rendu.
        /// </summary>
        private void BuildSidePanel()
        {
            sidePanel.Controls.Clear();

            List<CheckBox> checkBoxes = _importedSeries
                .Select((series, index) => series.ToCheckBox(index))
                .ToList();

            foreach (CheckBox checkBox in checkBoxes)
            {
                checkBox.CheckedChanged += (_, _) => PlotImportedSeries();
                sidePanel.Controls.Add(checkBox);
            }

            int top = 20 + checkBoxes.Count * 30;
            BuildImportButton(top);
            BuildStatusLabel(top + 40);
        }

        private void BuildImportButton(int top)
        {
            var jsonButton = new Button
            {
                Text = "Importer un dossier JSON…",
                Location = new Point(20, top),
                Size = new Size(160, 30),
            };
            jsonButton.Click += (_, _) => ImportSeriesFromJsonFolder();
            sidePanel.Controls.Add(jsonButton);
        }

        private void BuildStatusLabel(int top)
        {
            var statusLabel = new Label
            {
                Location = new Point(20, top),
                Size = new Size(165, 70),
                Text = _importedSeries.Count == 0
                    ? "Aucune donnée chargée."
                    : $"Séries importées : {string.Join(", ", _importedSeries.Select(series => series.Name))}" +
                      (_skippedFiles.Count == 0
                          ? string.Empty
                          : $"{Environment.NewLine}{_skippedFiles.Count} fichier(s) ignoré(s)"),
            };
            sidePanel.Controls.Add(statusLabel);
        }

        private void ImportSeriesFromJsonFolder()
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = "Choisissez un dossier contenant les fichiers JSON à tracer",
                UseDescriptionForTitle = true,
                SelectedPath = DefaultDataFolder(),
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ImportJsonFolder(dialog.SelectedPath, notify: true);
        }

        /// <summary>
        /// Charge toutes les séries du dossier et les trace. Quand l'import est lancé
        /// automatiquement au démarrage, on n'affiche pas de popup pour ne pas gêner l'utilisateur.
        /// </summary>
        private void ImportJsonFolder(string folderPath, bool notify)
        {
            JsonImportResult result;
            try
            {
                result = JsonFolderImporter.ImportFolder(folderPath);
            }
            catch (Exception ex)
            {
                if (notify)
                {
                    MessageBox.Show(this, ex.Message, "Import impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                BuildSidePanel();
                PlotImportedSeries();
                return;
            }

            if (result.Series.Count == 0)
            {
                if (notify)
                {
                    string detail = result.SkippedFiles.Count == 0
                        ? "Ce dossier ne contient aucun fichier .json."
                        : $"Fichiers ignorés :{Environment.NewLine}{string.Join(Environment.NewLine, result.SkippedFiles)}";

                    MessageBox.Show(this, detail, "Aucune donnée importée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                BuildSidePanel();
                PlotImportedSeries();
                return;
            }

            _importedSeries.Clear();
            _importedSeries.AddRange(result.Series);
            _importedFolder = folderPath;
            _skippedFiles.Clear();
            _skippedFiles.AddRange(result.SkippedFiles);

            BuildSidePanel();
            PlotImportedSeries();

            if (notify && result.SkippedFiles.Count > 0)
            {
                MessageBox.Show(
                    this,
                    $"Séries importées : {result.Series.Count}.{Environment.NewLine}" +
                    $"{Environment.NewLine}Fichiers ignorés :{Environment.NewLine}{string.Join(Environment.NewLine, result.SkippedFiles)}",
                    "Import terminé avec des fichiers ignorés",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void PlotImportedSeries()
        {
            plotPanel.Plot.Clear();

            List<PriceSeries> series = [.. SelectedImportedSeries];

            foreach (PriceSeries priceSeries in series)
            {
                double[] xs = priceSeries.Timestamps.Select(ToOADate).ToArray();
                var scatter = plotPanel.Plot.Add.Scatter(xs, priceSeries.Closes);
                scatter.LegendText = priceSeries.Name;
            }

            if (series.Count > 0)
            {
                plotPanel.Plot.ShowLegend();
            }

            plotPanel.Plot.Title(series.Count == 0
                ? (string.IsNullOrEmpty(_importedFolder)
                    ? "Importez un dossier JSON pour tracer les données"
                    : $"Aucune série sélectionnée (dossier : {_importedFolder})")
                : $"{series.Count}/{_importedSeries.Count} série(s) tracée(s) — {_importedFolder}");

            plotPanel.Plot.Axes.AutoScale();
            HideBenchmark();

            // Refresh() déclenche le rendu de façon synchrone : LastRender contient
            // donc le temps du rendu qui vient d'avoir lieu.
            plotPanel.Refresh();
            UpdateRenderInfo();
        }

        private void UpdateRenderInfo()
        {
            if (_renderLabel is null)
            {
                return;
            }

            double milliseconds = plotPanel.Plot.LastRender.Elapsed.TotalMilliseconds;
            _renderLabel.Text = milliseconds > 0
                ? $"Dernier rendu : {milliseconds:0.0} ms"
                : "Aucun rendu";
        }

        /// <summary>ScottPlot trace les dates sous forme de double OADate, pas en millisecondes Unix.</summary>
        private static double ToOADate(double unixMilliseconds) =>
            DateTimeOffset.FromUnixTimeMilliseconds((long)unixMilliseconds).LocalDateTime.ToOADate();

        /// <summary>Dossier proposé par défaut : le dossier "data" du projet, sinon le dossier courant.</summary>
        private static string DefaultDataFolder()
        {
            string nearExecutable = Path.Combine(AppContext.BaseDirectory, "data");
            if (Directory.Exists(nearExecutable))
            {
                return nearExecutable;
            }

            string projectData = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data"));
            return Directory.Exists(projectData) ? projectData : Environment.CurrentDirectory;
        }

        /// <summary>Les séries importées dont la case à cocher est cochée.</summary>
        private IEnumerable<PriceSeries> SelectedImportedSeries
        {
            get
            {
                HashSet<string> selected = sidePanel.Controls
                    .OfType<CheckBox>()
                    .Where(checkBox => checkBox.Checked && checkBox.Tag is PriceSeries)
                    .Select(checkBox => ((PriceSeries)checkBox.Tag!).Name)
                    .ToHashSet();

                return _importedSeries.Where(series => selected.Contains(series.Name));
            }
        }
    }
}
