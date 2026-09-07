using System.Drawing;

namespace P_Fun
{
    public partial class mainPage : Form
    {
        private static readonly string[] SeriesPairs =
            ["BTC/USDT", "ETH/USDT", "BNB/USDT", "SOL/USDT", "XRP/USDT"];

        public mainPage()
        {
            InitializeComponent();
            BuildSeriesCheckBoxes();
        }

        private void BuildSeriesCheckBoxes()
        {
            SeriesPairs
                .Select((pair, index) => new CheckBox
                {
                    Text = pair,
                    AutoSize = true,
                    Checked = true,
                    Location = new Point(20, 20 + index * 30),
                })
                .ToList()
                .ForEach(sidePanel.Controls.Add);
        }
    }
}
