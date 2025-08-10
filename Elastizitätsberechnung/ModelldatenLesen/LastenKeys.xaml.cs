using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using System.Windows.Input;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class LastenKeys
    {
        private readonly FeModell _modell;
        public string Id;
        public LastenKeys(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            Left = 2 * Width;
            Top = Height;

            var lasten = modell.Lasten.Where(item => item.Value is Modelldaten.KnotenLast).
            Select(item => item.Value).ToList();

            var linienlasten = modell.ElementLasten.Where(item => item.Value is LinienLast)
                .Select(AbstraktLast (item) => item.Value).ToList();
            lasten.AddRange(linienlasten);

            var punktlasten = modell.PunktLasten.Where(item => item.Value is PunktLast)
                .Select(AbstraktLast (item) => item.Value).ToList();
            lasten.AddRange(punktlasten);
            LastenKey.ItemsSource = lasten;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LastenKey.SelectedItems.Count <= 0) { return; }
            var last = (FE_Berechnungen.Elastizitätsberechnung.Modelldaten.KnotenLast)LastenKey.SelectedItem;
            if (last == null) return;

            Id = last.LastId;
            _modell.Berechnet = false;
        }

        private void MouseDoubleClickNeueKnotenlast(object sender, MouseButtonEventArgs e)
        {
            var lastNeu = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
            lastNeu.AktuelleId = lastNeu.LastId.Text;
            Close();
        }
    }
}