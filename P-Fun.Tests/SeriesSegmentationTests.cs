using P_Fun.Core;

namespace P_Fun.Tests
{
    /// <summary>
    /// Tests du découpage en blocs : un trou de données ne doit pas être
    /// traversé par un trait, la série est donc coupée dès que deux bougies ne
    /// se suivent plus. Aucune interface n'est nécessaire, le calcul est pur.
    /// </summary>
    public class SeriesSegmentationTests
    {
        [Fact]
        public void SplitOnGaps_SansTrou_UnSeulBloc()
        {
            IReadOnlyList<SeriesSegment> blocks = SeriesSegmentation.SplitOnGaps(Hours(0, 1, 2, 3, 4));

            Assert.Equal("0+5", Describe(blocks));
        }

        [Fact]
        public void SplitOnGaps_UneHeureManquante_CoupeEnDeuxBlocs()
        {
            // 0, 1 puis 3, 4 : l'heure 2 manque, le trait doit s'interrompre.
            IReadOnlyList<SeriesSegment> blocks = SeriesSegmentation.SplitOnGaps(Hours(0, 1, 3, 4));

            Assert.Equal("0+2,2+2", Describe(blocks));
        }

        [Fact]
        public void SplitOnGaps_PlusieursTrous_UnBlocParPortionContinue()
        {
            IReadOnlyList<SeriesSegment> blocks = SeriesSegmentation.SplitOnGaps(Hours(0, 1, 5, 6, 10));

            Assert.Equal("0+2,2+2,4+1", Describe(blocks));
        }

        [Fact]
        public void SplitOnGaps_SerieSansDonneeOuAvecUnSeulPoint_ResteTracable()
        {
            Assert.Empty(SeriesSegmentation.SplitOnGaps(Hours()));
            Assert.Equal("0+1", Describe(SeriesSegmentation.SplitOnGaps(Hours(7))));
        }

        [Fact]
        public void NominalInterval_RetrouveLIntervalleLePlusFrequent()
        {
            // Une série horaire contenant un trou de deux heures : l'intervalle
            // courant reste l'heure, c'est lui qui sert de référence.
            Assert.Equal(3_600_000, SeriesSegmentation.NominalInterval(Hours(0, 1, 3, 4)));
        }

        [Fact]
        public void SplitOnGaps_SurUneSerieContinueDuProjet_NeCoupeEnDeux()
        {
            // 20 000 bougies à la minute, comme un fichier du dossier data/.
            double[] timestamps = [.. Enumerable
                .Range(0, 20_000)
                .Select(index => 1_788_220_800_000d + (index * 60_000d))];

            IReadOnlyList<SeriesSegment> blocks = SeriesSegmentation.SplitOnGaps(timestamps);

            Assert.Equal("0+20000", Describe(blocks));
        }

        [Fact]
        public void SplitOnGaps_BougiesDeCinqMinutes_GardeLeMemeDecoupage()
        {
            // Le seuil suit l'intervalle des données, il n'est pas figé à la minute.
            double[] timestamps = [.. Enumerable
                .Range(0, 4)
                .Select(index => 1_788_220_800_000d + (index * 300_000d)), 1_788_220_800_000d + (8 * 300_000d)];

            Assert.Equal("0+4,4+1", Describe(SeriesSegmentation.SplitOnGaps(timestamps)));
        }

        /// <summary>
        /// Blocs écrits « début+longueur » séparés par des virgules, plus lisibles
        /// qu'une comparaison de listes en cas d'échec.
        /// </summary>
        private static string Describe(IReadOnlyList<SeriesSegment> blocks) =>
            string.Join(",", blocks.Select(block => $"{block.Start}+{block.Length}"));

        /// <summary>Horodatages en heures depuis le début des séries de test.</summary>
        private static double[] Hours(params int[] hours) =>
            [.. hours.Select(hour => (double)TestSeries.Start.AddHours(hour).ToUnixTimeMilliseconds())];
    }
}
