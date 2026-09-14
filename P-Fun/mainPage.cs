using P_Fun.Core;
using P_Fun.Data;
using P_Fun.Extensions;
using P_Fun.Models;

namespace P_Fun
{
    /// <summary>
    /// Fenêtre principale. Elle ne contient aucune règle de calcul : elle capte
    /// les événements, délègue au noyau pur (<c>P_Fun.Core</c>) et affiche le
    /// résultat. C'est ce qui rend le calcul testable sans interface.
    /// </summary>
    public partial class mainPage : Form
    {
        private readonly SeriesDatabase _database = new(DefaultDatabaseFile());
        private readonly List<PriceSeries> _importedSeries = [];
        private readonly List<string> _skippedFiles = [];
        private string _dataSource = string.Empty;

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

            // ToolTip est un composant à libérer : on l'attache au cycle de vie du
            // formulaire pour ne pas le laisser fuir à la fermeture.
            Disposed += (_, _) => _hoverToolTip.Dispose();

            // Le message d'avertissement est branché sur Shown : une boîte de
            // dialogue ne peut pas s'ouvrir depuis le constructeur, la fenêtre
            // n'existe pas encore.
            Shown += (_, _) => WarnAboutSkippedFiles();

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
        /// Prévient lorsque des fichiers de données ont été ignorés à l'amorçage
        /// de la base : sans ce message, une série peut manquer au graphique sans
        /// que rien ne l'explique (le panneau latéral ne montre qu'un compteur).
        /// </summary>
        private void WarnAboutSkippedFiles()
        {
            if (_skippedFiles.Count == 0)
            {
                return;
            }

            MessageBox.Show(
                this,
                $"{_skippedFiles.Count} fichier(s) de données n'ont pas pu être lus : " +
                $"les séries correspondantes sont absentes du graphique." +
                FormatSkippedFiles(_skippedFiles),
                "Séries incomplètes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Liste des fichiers ignorés mise en forme pour un message : vide s'il
        /// n'y en a aucun. Sert aussi bien à l'amorçage qu'à l'import manuel.
        /// </summary>
        private static string FormatSkippedFiles(IReadOnlyCollection<string> skippedFiles) =>
            skippedFiles.Count == 0
                ? string.Empty
                : $"{Environment.NewLine}{Environment.NewLine}Fichiers ignorés :" +
                  $"{Environment.NewLine}{string.Join(Environment.NewLine, skippedFiles)}";

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
        /// Distance maximale (en pixels) entre le curseur et un point pour que
        /// son infobulle s'affiche.
        /// </summary>
        private const double HoverRadius = 12;

        /// <summary>
        /// Affiche le prix réel de la bougie survolée. Le calcul est délégué à
        /// <see cref="HoverSearch"/> et <see cref="PointDescription"/> : ici on se
        /// contente de traduire l'écran en zone de dessin puis d'afficher le texte.
        /// </summary>
        private void OnPlotMouseMove(object? sender, MouseEventArgs e)
        {
            HoveredPoint? hovered = HoverSearch.FindNearest(
                _plottedSeries,
                PlotChartArea(),
                PlotAxisRange(),
                e.X,
                e.Y,
                HoverRadius);

            if (hovered is null)
            {
                HideHoverToolTip();
                return;
            }

            string text = PointDescription.Describe(hovered, _normalizeToBase100);
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

        /// <summary>Zone de dessin du graphique (pixels), telle que ScottPlot l'a dessinée au dernier rendu.</summary>
        private ChartArea PlotChartArea()
        {
            ScottPlot.PixelRect area = plotPanel.Plot.LastRender.DataRect;
            return new ChartArea(area.Left, area.Bottom, area.Width, area.Height);
        }

        /// <summary>Limites des axes, dans l'unité des données tracées (les dates sont des OADate).</summary>
        private AxisRange PlotAxisRange()
        {
            ScottPlot.AxisLimits limits = plotPanel.Plot.Axes.GetLimits();
            return new AxisRange(limits.Left, limits.Right, limits.Bottom, limits.Top);
        }

        /// <summary>
        /// ScottPlot dessine en bas à gauche du graphique un encadré jaune
        /// « Rendered in … ms » (son benchmark). On le masque : il n'apporte rien
        /// à l'analyse des séries.
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
            const int Base = 1024;
            string[] units = ["o", "Ko", "Mo", "Go"];
            double bytes = new FileInfo(_database.DatabasePath).Length;

            // Puissance de 1024 la plus grande qui reste inférieure à la taille :
            // 1536 octets → Ko, 5 000 000 → Mo. Pas de boucle, et le log d'un
            // fichier vide (0 octet) est ramené à l'unité de base.
            int unit = Math.Min((int)(Math.Log(Math.Max(bytes, 1), Base)), units.Length - 1);

            return $"{bytes / Math.Pow(Base, unit):0.#} {units[unit]}";
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
                string detail = $"Aucun fichier .json exploitable dans ce dossier." +
                                FormatSkippedFiles(report.SkippedFiles);

                MessageBox.Show(this, detail, "Aucune donnée importée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ignored = FormatSkippedFiles(report.SkippedFiles);

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

            // Refresh() déclenche le rendu de façon synchrone : LastRender est à
            // jour dès le retour, ce dont dépend la géométrie du survol.
            plotPanel.Refresh();
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
                // On compare les instances stockées dans le Tag, pas les noms :
                // deux fichiers peuvent donner le même nom de série sans que
                // leurs cases se confondent.
                HashSet<PriceSeries> selected = sidePanel.Controls
                    .OfType<CheckBox>()
                    .Where(checkBox => checkBox.Checked && checkBox.Tag is PriceSeries)
                    .Select(checkBox => (PriceSeries)checkBox.Tag!)
                    .ToHashSet();

                return _importedSeries.Where(selected.Contains);
            }
        }
    }
}
