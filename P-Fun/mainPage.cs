using P_Fun.Data;
using P_Fun.Extensions;
using P_Fun.Models;

namespace P_Fun
{
    public partial class mainPage : Form
    {
        private readonly SeriesDatabase _database = new(DefaultDatabaseFile());
        private readonly List<PriceSeries> _importedSeries = [];
        private readonly List<string> _skippedFiles = [];
        private string _dataSource = string.Empty;
        private Label? _renderLabel;

        // Points actuellement dessinés, conservés pour retrouver la bougie
        // survolée et afficher son prix réel dans une infobulle.
        private readonly List<PlottedSeries> _plottedSeries = [];
        private readonly ToolTip _hoverToolTip = new();
        private string? _hoveredPoint;

        // Affichage superposé : chaque série est ramenée à 100 au lieu d'être
        // tracée dans son prix réel, sinon une série à 60 000 écrase tout le reste.
        private bool _normalizeToBase100 = true;

        public mainPage()
        {
            InitializeComponent();
            ConfigurePlot();

            LoadStoredSeries();
        }

        /// <summary>
        /// Au premier lancement la base est vide : on l'amorce avec les JSON du
        /// dossier "data" du projet. Ensuite on lit uniquement la base, donc le
        /// démarrage ne dépend plus des fichiers JSON.
        /// </summary>
        private void LoadStoredSeries()
        {
            try
            {
                if (_database.IsEmpty())
                {
                    MergeReport seed = _database.ImportFolder(DefaultDataFolder());
                    _skippedFiles.Clear();
                    _skippedFiles.AddRange(seed.SkippedFiles);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Base de données inaccessible", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ReadSeriesFromDatabase();
        }

        /// <summary>
        /// Recharge toutes les séries depuis la base SQLite et redessine le
        /// graphique. C'est la seule source de données après le premier import.
        /// </summary>
        private void ReadSeriesFromDatabase()
        {
            _importedSeries.Clear();
            _importedSeries.AddRange(_database.LoadSeries());
            _dataSource = Path.GetFileName(_database.DatabasePath);

            BuildSidePanel();
            PlotImportedSeries();
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

            plotPanel.MouseMove += OnPlotMouseMove;
            plotPanel.MouseLeave += (_, _) => HideHoverToolTip();
        }

        /// <summary>
        /// Une série telle qu'elle est tracée : les abscisses en OADate et les
        /// ordonnées réellement dessinées (indice en base 100, ou prix). Le prix
        /// réel reste dans <see cref="PriceSeries"/> pour l'infobulle.
        /// </summary>
        private sealed record PlottedSeries(PriceSeries Series, double[] Xs, double[] Values);

        /// <summary>
        /// Trouve le point le plus proche du curseur et affiche son prix réel.
        /// La recherche se fait en pixels, donc elle reste cohérente quel que
        /// soit le zoom : il faut vraiment viser la courbe pour que ça réagisse.
        /// </summary>
        private void OnPlotMouseMove(object? sender, MouseEventArgs e)
        {
            PlottedSeries? nearest = FindNearestPoint(e.X, e.Y, out int index);
            if (nearest is null)
            {
                HideHoverToolTip();
                return;
            }

            string text = DescribePoint(nearest, index);
            if (text == _hoveredPoint)
            {
                return;
            }

            _hoveredPoint = text;
            _hoverToolTip.Show(text, plotPanel, e.X + 14, e.Y + 14, 5000);
        }

        private void HideHoverToolTip()
        {
            _hoveredPoint = null;
            _hoverToolTip.Hide(plotPanel);
        }

        /// <summary>
        /// Distance maximale (en pixels) entre le curseur et un point pour que
        /// son infobulle s'affiche.
        /// </summary>
        private const double HoverRadius = 12;

        private PlottedSeries? FindNearestPoint(int mouseX, int mouseY, out int index)
        {
            index = -1;

            ScottPlot.PixelRect dataArea = plotPanel.Plot.LastRender.DataRect;
            if (dataArea.Width <= 0 || dataArea.Height <= 0 || !dataArea.Contains(new ScottPlot.Pixel(mouseX, mouseY)))
            {
                return null;
            }

            ScottPlot.AxisLimits limits = plotPanel.Plot.Axes.GetLimits();
            if (limits.Right <= limits.Left || limits.Top <= limits.Bottom)
            {
                return null;
            }

            double xScale = dataArea.Width / (limits.Right - limits.Left);
            double yScale = dataArea.Height / (limits.Top - limits.Bottom);

            PlottedSeries? nearest = null;
            double bestDistance = HoverRadius;

            foreach (PlottedSeries plotted in _plottedSeries)
            {
                for (int i = 0; i < plotted.Xs.Length; i++)
                {
                    double x = dataArea.Left + (plotted.Xs[i] - limits.Left) * xScale;
                    double y = dataArea.Bottom - (plotted.Values[i] - limits.Bottom) * yScale;
                    double distance = Math.Sqrt(((x - mouseX) * (x - mouseX)) + ((y - mouseY) * (y - mouseY)));

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        nearest = plotted;
                        index = i;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Prix réel de la bougie survolée, avec sa date, sa variation depuis le
        /// début de la série et, en mode base 100, l'indice affiché à l'écran.
        /// </summary>
        private string DescribePoint(PlottedSeries plotted, int index)
        {
            PriceSeries series = plotted.Series;
            double price = series.Closes[index];
            double reference = series.Base100Reference();

            DateTime time = DateTimeOffset
                .FromUnixTimeMilliseconds((long)series.Timestamps[index])
                .LocalDateTime;

            string variation = reference == 0
                ? string.Empty
                : $"{Environment.NewLine}{(price / reference - 1) * 100:+0.00;-0.00;0.00} % depuis le début";

            string indexText = _normalizeToBase100
                ? $"{Environment.NewLine}indice {plotted.Values[index]:0.##}"
                : string.Empty;

            return $"{series.Name} — {time:dd/MM/yyyy HH:mm}{Environment.NewLine}" +
                   $"{price:0.##} USDT{variation}{indexText}";
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
            BuildNormalizeCheckBox(top + 35);
            BuildStatusLabel(top + 65);
            BuildDatabaseLabel(top + 140);
        }

        /// <summary>
        /// Bascule entre le prix réel et la base 100. Sans elle, une série à
        /// 60 000 écrase une série à 2 500 et le graphique devient illisible.
        /// </summary>
        private void BuildNormalizeCheckBox(int top)
        {
            var normalizeCheckBox = new CheckBox
            {
                Text = "Comparer (base 100)",
                Location = new Point(20, top),
                AutoSize = true,
                Checked = _normalizeToBase100,
                UseVisualStyleBackColor = true,
            };
            normalizeCheckBox.CheckedChanged += (_, _) =>
            {
                _normalizeToBase100 = normalizeCheckBox.Checked;
                PlotImportedSeries();
            };
            sidePanel.Controls.Add(normalizeCheckBox);
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
                    ? "Aucune donnée en base."
                    : $"Séries en base : {string.Join(", ", _importedSeries.Select(series => series.Name))}" +
                      (_skippedFiles.Count == 0
                          ? string.Empty
                          : $"{Environment.NewLine}{_skippedFiles.Count} fichier(s) ignoré(s)"),
            };
            sidePanel.Controls.Add(statusLabel);
        }

        /// <summary>
        /// Informations sur la base SQLite : son nom, le nombre de bougies
        /// qu'elle contient et la place qu'elle occupe sur le disque.
        /// </summary>
        private void BuildDatabaseLabel(int top)
        {
            var databaseLabel = new Label
            {
                Location = new Point(20, top),
                Size = new Size(165, 45),
                Text = $"Base : {Path.GetFileName(_database.DatabasePath)}{Environment.NewLine}" +
                       $"{_database.CandleCount()} bougies — {DatabaseSize()}",
            };
            sidePanel.Controls.Add(databaseLabel);
        }

        /// <summary>Taille du fichier de base, arrondie en unité lisible (« 144 Ko »).</summary>
        private string DatabaseSize()
        {
            string[] units = ["o", "Ko", "Mo", "Go"];
            double size = new FileInfo(_database.DatabasePath).Length;

            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.#} {units[unit]}";
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
        /// Fusionne le dossier dans la base SQLite : les bougies qui chevauchent
        /// des données déjà stockées sont remplacées, les autres sont ajoutées.
        /// Le graphique est ensuite relu depuis la base.
        /// </summary>
        private void ImportJsonFolder(string folderPath, bool notify)
        {
            MergeReport report;
            try
            {
                report = _database.ImportFolder(folderPath);
            }
            catch (Exception ex)
            {
                if (notify)
                {
                    MessageBox.Show(this, ex.Message, "Import impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ReadSeriesFromDatabase();
                return;
            }

            _skippedFiles.Clear();
            _skippedFiles.AddRange(report.SkippedFiles);
            ReadSeriesFromDatabase();

            if (!notify)
            {
                return;
            }

            if (report.Total == 0)
            {
                string detail = report.SkippedFiles.Count == 0
                    ? "Ce dossier ne contient aucun fichier .json."
                    : $"Fichiers ignorés :{Environment.NewLine}{string.Join(Environment.NewLine, report.SkippedFiles)}";

                MessageBox.Show(this, detail, "Aucune donnée importée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ignored = report.SkippedFiles.Count == 0
                ? string.Empty
                : $"{Environment.NewLine}{Environment.NewLine}Fichiers ignorés :{Environment.NewLine}{string.Join(Environment.NewLine, report.SkippedFiles)}";

            MessageBox.Show(
                this,
                $"{report.Added} bougie(s) ajoutée(s).{Environment.NewLine}" +
                $"{report.Overwritten} bougie(s) déjà présente(s) mise(s) à jour.{Environment.NewLine}" +
                $"{Environment.NewLine}Total en base : {_database.CandleCount()} bougies.{ignored}",
                "Import terminé",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void PlotImportedSeries()
        {
            plotPanel.Plot.Clear();
            _plottedSeries.Clear();
            HideHoverToolTip();

            List<PriceSeries> series = [.. SelectedImportedSeries];

            foreach (PriceSeries priceSeries in series)
            {
                PriceSeries plotted = _normalizeToBase100 ? priceSeries.NormalizedToBase100() : priceSeries;
                double[] xs = plotted.Timestamps.Select(ToOADate).ToArray();
                var scatter = plotPanel.Plot.Add.Scatter(xs, plotted.Closes);
                scatter.LegendText = priceSeries.LegendLabel(_normalizeToBase100);

                _plottedSeries.Add(new PlottedSeries(priceSeries, xs, plotted.Closes));
            }

            plotPanel.Plot.Axes.Left.Label.Text = _normalizeToBase100
                ? "Indice (base 100)"
                : "Prix (USDT)";

            if (series.Count > 0)
            {
                plotPanel.Plot.ShowLegend();
            }

            plotPanel.Plot.Title(series.Count == 0
                ? (string.IsNullOrEmpty(_dataSource)
                    ? "Importez un dossier JSON pour tracer les données"
                    : $"Aucune série sélectionnée ({_dataSource})")
                : $"{series.Count}/{_importedSeries.Count} série(s) tracée(s) — {(_normalizeToBase100 ? "base 100 — " : string.Empty)}{_dataSource}");

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

        /// <summary>La base SQLite, stockée à côté des JSON dans le dossier "data".</summary>
        private static string DefaultDatabaseFile() => Path.Combine(DefaultDataFolder(), "p-fun.db");

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
