using P_Fun.Models;

namespace P_Fun.Data
{
    /// <summary>
    /// Importe les séries crypto depuis un fichier CSV.
    /// WIP : le parsing des colonnes n'est pas terminé.
    /// </summary>
    public static class CsvSeriesImporter
    {
        public static IReadOnlyList<CryptoSeries> Import(string filePath)
        {
            List<CryptoSeries> series = [];

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                // WIP : le fichier contient un en-tête mais on ne le saute pas,
                // et l'index de la colonne Description est faux.
                string[] parts = line.Split(';');
                series.Add(new CryptoSeries(parts[0], parts[2]));
            }

            return series;
        }
    }
}