using System.Windows;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class KnotenGruppeNeu : Window
    {
        private readonly FeModell _modell;
        public KnotenGruppeNeu()
        {
            InitializeComponent();
        }
        public KnotenGruppeNeu(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
            Show();
        }
    }
}
