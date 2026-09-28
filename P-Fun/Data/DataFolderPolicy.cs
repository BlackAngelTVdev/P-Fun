namespace P_Fun.Data
{
    /// <summary>
    /// Décide si le dossier de données doit être relu au lancement. Le dossier
    /// n'est fusionné dans la base que s'il apporte quelque chose : base encore
    /// vide (premier lancement) ou fichier JSON modifié depuis le dernier
    /// import. Les valeurs relues écrasent alors celles déjà stockées.
    /// Sans cette vérification, soit les nouveaux JSON ne seraient jamais pris
    /// en compte (l'ancien resterait en base), soit des dizaines de Mo de JSON
    /// seraient relus à chaque lancement pour rien.
    /// </summary>
    public static class DataFolderPolicy
    {
        /// <summary>
        /// Vrai s'il faut fusionner le dossier dans la base. Le dossier est
        /// daté par son fichier <c>*.json</c> le plus récent, la base par sa
        /// dernière écriture : dès que la base redevient la plus récente,
        /// l'import ne se répète plus.
        /// </summary>
        public static bool ShouldImport(string dataFolder, string databaseFile, bool databaseIsEmpty)
        {
            if (!Directory.Exists(dataFolder))
            {
                return false;
            }

            if (databaseIsEmpty)
            {
                return true;
            }

            DateTime lastImport = File.GetLastWriteTimeUtc(databaseFile);

            return Directory
                .EnumerateFiles(dataFolder, "*.json")
                .Any(file => File.GetLastWriteTimeUtc(file) > lastImport);
        }
    }
}
