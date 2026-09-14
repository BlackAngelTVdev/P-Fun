using System.Data;
using Microsoft.Data.Sqlite;
using P_Fun.Models;

namespace P_Fun.Data
{
    /// <summary>
    /// Résultat d'un import dans la base : les bougies réellement ajoutées,
    /// celles qui écrasent une valeur déjà stockée (chevauchement) et les
    /// fichiers qui n'ont pas pu être lus.
    /// </summary>
    public sealed record MergeReport(int Added, int Overwritten, IReadOnlyList<string> SkippedFiles)
    {
        public int Total => Added + Overwritten;
    }

    /// <summary>
    /// Stockage local des séries dans une base SQLite : une seule table de
    /// bougies (clôture + horodatage). La clé primaire (Symbol, OpenTime)
    /// garantit qu'une bougie n'existe qu'une seule fois : réimporter une
    /// période déjà stockée met à jour la valeur au lieu de la dupliquer.
    /// </summary>
    public sealed class SeriesDatabase
    {
        private const string CreateSchema = """
            CREATE TABLE IF NOT EXISTS Candles (
                Symbol   TEXT    NOT NULL,
                OpenTime INTEGER NOT NULL,
                Close    REAL    NOT NULL,
                PRIMARY KEY (Symbol, OpenTime)
            ) WITHOUT ROWID;
            """;

        /// <summary>
        /// En cas de chevauchement, c'est la valeur importée qui gagne : la
        /// ligne existante est mise à jour, pas dupliquée.
        /// </summary>
        private const string UpsertCandle = """
            INSERT INTO Candles (Symbol, OpenTime, Close)
            VALUES ($symbol, $openTime, $close)
            ON CONFLICT (Symbol, OpenTime) DO UPDATE SET Close = excluded.Close;
            """;

        private readonly string _databasePath;

        public SeriesDatabase(string databasePath)
        {
            _databasePath = databasePath;
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath) ?? ".");

            using SqliteConnection connection = OpenConnection();
            Execute(connection, CreateSchema);
        }

        public string DatabasePath => _databasePath;

        /// <summary>Vrai si aucune bougie n'est encore stockée (premier lancement).</summary>
        public bool IsEmpty()
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT EXISTS (SELECT 1 FROM Candles);";
            return Convert.ToInt64(command.ExecuteScalar()) == 0;
        }

        /// <summary>Nombre total de bougies stockées, tous symboles confondus.</summary>
        public int CandleCount()
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Candles;";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>Toutes les séries de la base, triées par symbole puis par date.</summary>
        public IReadOnlyList<PriceSeries> LoadSeries()
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Symbol, OpenTime, Close FROM Candles ORDER BY Symbol, OpenTime;";

            return ReadRows(command)
                .Select(row => new StoredCandle(
                    Convert.ToString(row[0]) ?? string.Empty,
                    Convert.ToInt64(row[1]),
                    Convert.ToDouble(row[2])))
                .GroupBy(candle => candle.Symbol)
                .Select(group => new PriceSeries(
                    group.Key,
                    [.. group.Select(candle => (double)candle.OpenTime)],
                    [.. group.Select(candle => candle.Close)]))
                .ToList();
        }

        /// <summary>
        /// Lit tous les JSON du dossier et les fusionne dans la base : les
        /// bougies dont l'horodatage est déjà stocké pour ce symbole écrasent
        /// l'ancienne valeur, les autres sont ajoutées.
        /// </summary>
        public MergeReport ImportFolder(string folderPath)
        {
            JsonImportResult import = JsonFolderImporter.ImportFolder(folderPath);

            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction transaction = connection.BeginTransaction();

            int added = 0;
            int overwritten = 0;

            foreach (PriceSeries series in import.Series)
            {
                (int seriesAdded, int seriesOverwritten) = Merge(connection, transaction, series);
                added += seriesAdded;
                overwritten += seriesOverwritten;
            }

            transaction.Commit();
            return new MergeReport(added, overwritten, import.SkippedFiles);
        }

        private static (int Added, int Overwritten) Merge(
            SqliteConnection connection,
            SqliteTransaction transaction,
            PriceSeries series)
        {
            HashSet<long> stored = LoadTimestamps(connection, transaction, series.Name);

            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = UpsertCandle;

            SqliteParameter symbol = command.Parameters.Add("$symbol", SqliteType.Text);
            SqliteParameter openTime = command.Parameters.Add("$openTime", SqliteType.Integer);
            SqliteParameter close = command.Parameters.Add("$close", SqliteType.Real);
            symbol.Value = series.Name;

            int overwritten = 0;

            foreach ((double timestamp, double value) in series.Timestamps.Zip(series.Closes))
            {
                long storedTime = (long)timestamp;
                overwritten += stored.Contains(storedTime) ? 1 : 0;

                openTime.Value = storedTime;
                close.Value = value;
                command.ExecuteNonQuery();
            }

            return (series.Closes.Length - overwritten, overwritten);
        }

        private static HashSet<long> LoadTimestamps(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string symbol)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT OpenTime FROM Candles WHERE Symbol = $symbol;";
            command.Parameters.AddWithValue("$symbol", symbol);

            return [.. ReadRows(command).Select(row => Convert.ToInt64(row[0]))];
        }

        /// <summary>
        /// Exécute une requête et rend ses lignes exploitables avec LINQ. C'est
        /// l'équivalent sans boucle de <c>while (reader.Read())</c> : ADO.NET ne
        /// fournit pas d'énumérateur, on matérialise donc la réponse en mémoire
        /// (le volume d'un import reste de l'ordre de quelques milliers de lignes).
        /// </summary>
        private static IEnumerable<DataRow> ReadRows(SqliteCommand command)
        {
            DataTable table = new();
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                table.Load(reader);
            }

            return table.Rows.Cast<DataRow>();
        }

        private SqliteConnection OpenConnection()
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = _databasePath };
            SqliteConnection connection = new(builder.ToString());
            connection.Open();
            return connection;
        }

        private static void Execute(SqliteConnection connection, string sql)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        /// <summary>Une bougie telle qu'elle est stockée dans la base.</summary>
        private sealed record StoredCandle(string Symbol, long OpenTime, double Close);
    }
}
