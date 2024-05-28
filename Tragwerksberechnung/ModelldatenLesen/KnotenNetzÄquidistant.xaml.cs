using System.Windows;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class KnotenNetzÄquidistant : Window
    {
        private readonly FeModell _modell;

        public KnotenNetzÄquidistant()
        {
            InitializeComponent();
        }
        public KnotenNetzÄquidistant(FeModell feModell)
        {
            InitializeComponent();
            _modell = feModell;
        }

        private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
