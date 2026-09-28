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
            // Chaque fichier donne soit une série, soit une raison d'échec : on
            // projette une fois puis on partitionne, au lieu d'accumuler dans
            // deux listes au fil d'une boucle.
            List<(string File, JsonImport Attempt)> attempts = [.. Directory
                .EnumerateFiles(folderPath, "*.json")
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .Select(file => (File: file, Attempt: TryImportFile(file)))];

            return new JsonImportResult(
                [.. attempts.Where(attempt => attempt.Attempt.Series is not null)
                            .Select(attempt => attempt.Attempt.Series!)],
                [.. attempts.Where(attempt => attempt.Attempt.Series is null)
                            .Select(attempt => $"{Path.GetFileName(attempt.File)} ({attempt.Attempt.Error})")]);
        }

        private static JsonImport TryImportFile(string filePath)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));

                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return new JsonImport(null, "la racine du fichier n'est pas un tableau");
                }

                (double Timestamp, double Close)[] candles = [.. document.RootElement
                    .EnumerateArray()
                    .Where(IsUsableCandle)
                    .Select(candle => (Timestamp: candle[0].GetDouble(), Close: ReadClose(candle[4])))];

                if (candles.Length == 0)
                {
                    return new JsonImport(null, "aucune bougie exploitable");
                }

                string name = Path.GetFileNameWithoutExtension(filePath).ToUpperInvariant();
                return new JsonImport(
                    new PriceSeries(
                        name,
                        [.. candles.Select(candle => candle.Timestamp)],
                        [.. candles.Select(candle => candle.Close)]),
                    null);
            }
            catch (Exception ex) when (ex is JsonException or IOException or FormatException or InvalidOperationException)
            {
                return new JsonImport(null, ex.Message);
            }
        }

        /// <summary>Une bougie exploitable : un tableau d'au moins cinq valeurs (openTime, open, high, low, close).</summary>
        private static bool IsUsableCandle(JsonElement candle) =>
            candle.ValueKind == JsonValueKind.Array && candle.GetArrayLength() >= 5;

        /// <summary>Issue de la lecture d'un fichier : la série, ou la raison de l'échec.</summary>
        private sealed record JsonImport(PriceSeries? Series, string? Error);

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
