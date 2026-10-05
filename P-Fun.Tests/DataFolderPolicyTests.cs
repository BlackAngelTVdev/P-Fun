using P_Fun.Data;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests de la décision de réimport : le dossier de données n'est relu que
    /// s'il est plus récent que la base. Sans cela, des JSON refaits ne seraient
    /// jamais pris en compte et l'ancien resterait affiché.
    /// Travaille dans un dossier temporaire, aucune donnée réelle n'est touchée.
    /// </summary>
    public class DataFolderPolicyTests : IDisposable
    {
        /// <summary>Date de référence : la dernière écriture de la base.</summary>
        private static readonly DateTime Reference = new(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc);

        private readonly string _folder = Path.Combine(Path.GetTempPath(), $"p-fun-policy-{Guid.NewGuid():N}");
        private readonly string _databaseFile;

        public DataFolderPolicyTests()
        {
            Directory.CreateDirectory(_folder);
            _databaseFile = Path.Combine(_folder, "p-fun.db");
            Write(_databaseFile, Reference);
        }

        [Fact]
        public void ShouldImport_UnSeulJsonModifie_SuffitAReimporterLeDossier()
        {
            WriteJson("btc.json", Reference.AddHours(-1));
            WriteJson("eth.json", Reference.AddMinutes(5));

            Assert.True(DataFolderPolicy.ShouldImport(_folder, _databaseFile, databaseIsEmpty: false));
        }

        [Fact]
        public void ShouldImport_BasePlusRecenteQueTousLesJson_NeRelitRien()
        {
            WriteJson("btc.json", Reference.AddHours(-1));
            WriteJson("eth.json", Reference.AddHours(-2));

            Assert.False(DataFolderPolicy.ShouldImport(_folder, _databaseFile, databaseIsEmpty: false));
        }

        public void Dispose()
        {
            Directory.Delete(_folder, recursive: true);
            GC.SuppressFinalize(this);
        }

        private void WriteJson(string fileName, DateTime lastWrite) =>
            Write(Path.Combine(_folder, fileName), lastWrite);

        private static void Write(string file, DateTime lastWrite)
        {
            File.WriteAllText(file, "[]");
            File.SetLastWriteTimeUtc(file, lastWrite);
        }
    }
}
