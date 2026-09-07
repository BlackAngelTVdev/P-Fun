using P_Fun.Data;
using P_Fun.Extensions;
using P_Fun.Models;

namespace P_Fun
{
    public partial class mainPage : Form
    {
        public mainPage()
        {
            InitializeComponent();
            BuildSeriesCheckBoxes();
        }

        private void BuildSeriesCheckBoxes()
        {
            SeriesCatalog.All
                .Select((series, index) => series.ToCheckBox(index))
                .ToList()
                .ForEach(sidePanel.Controls.Add);
        }

        public IEnumerable<CryptoSeries> SelectedSeries =>
            SeriesCatalog.All.Selected(sidePanel.Controls.OfType<CheckBox>());
    }
}
