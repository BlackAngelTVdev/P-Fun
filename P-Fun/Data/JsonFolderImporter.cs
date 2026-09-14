using System.Globalization;
using System.Text.Json;
using P_Fun.Models;

namespace P_Fun.Data
{
    /// <summary>
    /// Résultat d'un import de dossier : les séries lues et les fichiers ignorés.
    /// </summary>
    public sealed record JsonImportResult(IReadOnlyList<PriceSeries> Series, IReadOnlyList<string> SkippedFiles);

    /// <summary>
    /// Importe toutes les séries crypto contenues dans un dossier de fichiers JSON.
    /// Format attendu : un tableau de bougies, chaque bougie étant un tableau
    /// [openTime, open, high, low, close, volume, ...] (format klines Binance).
    /// </summary>
    public static class JsonFolderImporter
    {
        public static JsonImportResult ImportFolder(string folderPath)
        {
            List<PriceSeries> series = [];
            List<string> skipped = [];

            string[] files = Directory.EnumerateFiles(folderPath, "*.json")
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string file in files)
            {
                PriceSeries? imported = TryImportFile(file, out string? error);

                if (imported is null)
                {
                    skipped.Add($"{Path.GetFileName(file)} ({error})");
                    continue;
                }

                series.Add(imported);
            }

            return new JsonImportResult(series, skipped);
        }

        private static PriceSeries? TryImportFile(string filePath, out string? error)
        {
            error = null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));

                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    error = "la racine du fichier n'est pas un tableau";
                    return null;
                }

                List<double> timestamps = [];
                List<double> closes = [];

                foreach (JsonElement candle in document.RootElement.EnumerateArray())
                {
                    if (candle.ValueKind != JsonValueKind.Array || candle.GetArrayLength() < 5)
                    {
                        continue;
                    }

                    timestamps.Add(candle[0].GetDouble());
                    closes.Add(ReadClose(candle[4]));
                }

                if (closes.Count == 0)
                {
                    error = "aucune bougie exploitable";
                    return null;
                }

                string name = Path.GetFileNameWithoutExtension(filePath).ToUpperInvariant();
                return new PriceSeries(name, [.. timestamps], [.. closes]);
            }
            catch (Exception ex) when (ex is JsonException or IOException or FormatException or InvalidOperationException)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Le prix de clôture est une chaîne ("76714.00000000") mais on accepte
        /// aussi un nombre pour rester tolérant sur le format des fichiers.
        /// </summary>
        private static double ReadClose(JsonElement close) => close.ValueKind switch
        {
            JsonValueKind.String => double.Parse(close.GetString() ?? "0", CultureInfo.InvariantCulture),
            JsonValueKind.Number => close.GetDouble(),
            _ => 0d,
        };
    }
}
