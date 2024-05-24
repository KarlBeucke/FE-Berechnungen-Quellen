using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class KnotenNetzNeu : Window
    {
        private readonly FeModell _modell;
        public KnotenNetzNeu()
        {
            InitializeComponent();
        }
        public KnotenNetzNeu(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
            Show();
        }
    }
}
