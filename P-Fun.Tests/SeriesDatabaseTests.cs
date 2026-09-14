using System.Globalization;
using Microsoft.Data.Sqlite;
using P_Fun.Data;
using P_Fun.Models;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests de la base SQLite : c'est elle qui garantit qu'une bougie n'existe
    /// qu'une fois, même après plusieurs imports de la même période.
    /// Travaille dans un dossier temporaire, aucune donnée réelle n'est touchée.
    /// </summary>
    public class SeriesDatabaseTests : IDisposable
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), $"p-fun-tests-{Guid.NewGuid():N}");

        public SeriesDatabaseTests() => Directory.CreateDirectory(_folder);

        private string DatabaseFile => Path.Combine(_folder, "p-fun.db");

        [Fact]
        public void ImportFolder_AjouteLesBougiesDUneSerie()
        {
            WriteCandles("btc.json", (0, 1000), (1, 1010), (2, 1020));
            SeriesDatabase database = new(DatabaseFile);

            MergeReport report = database.ImportFolder(_folder);

            Assert.Equal(3, report.Added);
            Assert.Equal(0, report.Overwritten);
            Assert.Equal(3, database.CandleCount());
        }

        [Fact]
        public void ImportFolder_RemplaceLesBougiesDejaStockeesSansCreerDeDoublon()
        {
            SeriesDatabase database = new(DatabaseFile);
            WriteCandles("btc.json", (0, 1000), (1, 1010));
            database.ImportFolder(_folder);

            // L'heure 1 est déjà en base (valeur mise à jour), l'heure 2 est nouvelle.
            WriteCandles("btc.json", (1, 1015), (2, 1020));
            MergeReport report = database.ImportFolder(_folder);

            Assert.Equal(1, report.Added);
            Assert.Equal(1, report.Overwritten);
            Assert.Equal(3, database.CandleCount());
            Assert.Equal([1000d, 1015d, 1020d], database.LoadSeries().Single().Closes);
        }

        [Fact]
        public void LoadSeries_TrieLesBougiesParDateEtNommeLaSerieDapresLeFichier()
        {
            WriteCandles("btc.json", (2, 30), (0, 10), (1, 20));
            SeriesDatabase database = new(DatabaseFile);
            database.ImportFolder(_folder);

            PriceSeries series = database.LoadSeries().Single();

            Assert.Equal("BTC", series.Name);
            Assert.Equal([10d, 20d, 30d], series.Closes);
        }

        [Fact]
        public void ImportFolder_IgnoreLesFichiersIllisibles()
        {
            File.WriteAllText(Path.Combine(_folder, "casse.json"), "{ ce n'est pas du JSON");
            SeriesDatabase database = new(DatabaseFile);

            MergeReport report = database.ImportFolder(_folder);

            Assert.Equal(0, report.Total);
            Assert.Single(report.SkippedFiles);
            Assert.Equal(0, database.CandleCount());
        }

        [Fact]
        public void IsEmpty_EstVraiTantQuAucuneBougieNEstImportee()
        {
            SeriesDatabase database = new(DatabaseFile);

            bool avant = database.IsEmpty();
            WriteCandles("btc.json", (0, 1000));
            database.ImportFolder(_folder);

            Assert.True(avant);
            Assert.False(database.IsEmpty());
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(_folder, recursive: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Écrit un fichier au format klines attendu par l'importeur.</summary>
        private void WriteCandles(string fileName, params (int Hour, double Close)[] candles)
        {
            IEnumerable<string> rows = candles.Select(candle =>
                $"[{TestSeries.Start.AddHours(candle.Hour).ToUnixTimeMilliseconds()},\"0\",\"0\",\"0\",\"{Format(candle.Close)}\",\"0\"]");

            File.WriteAllText(Path.Combine(_folder, fileName), $"[{string.Join(",", rows)}]");
        }

        private static string Format(double value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
