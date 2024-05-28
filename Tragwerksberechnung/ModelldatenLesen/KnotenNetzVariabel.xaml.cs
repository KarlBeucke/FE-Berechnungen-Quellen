using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class KnotenNetzVariabel : Window
    {
        private readonly FeModell _modell;
        public KnotenNetzVariabel()
        {
            InitializeComponent();
        }
        public KnotenNetzVariabel(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
        }
    }
}