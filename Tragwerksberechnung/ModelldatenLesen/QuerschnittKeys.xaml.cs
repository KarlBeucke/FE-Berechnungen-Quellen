using System.Linq;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class QuerschnittKeys
    {
        public QuerschnittKeys(FeModell modell)
        {
            InitializeComponent();
            this.Left = 2 * this.Width;
            this.Top = this.Height;
            var querschnitt = modell.Querschnitt.Select(item => item.Value).ToList();
            QuerschnittKey.ItemsSource = querschnitt;
        }
    }
}
