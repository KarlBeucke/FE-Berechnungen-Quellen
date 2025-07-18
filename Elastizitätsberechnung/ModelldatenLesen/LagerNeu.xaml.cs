using System.Globalization;
using System.Windows.Input;
using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class LagerNeu
    {
        private readonly FeModell _modell;
        private LagerKeys _lagerKeys;
        public string AktuelleId;

        public LagerNeu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            Show();
            AktuelleId = LagerId.Text;
        }

        public LagerNeu(FeModell modell, AbstraktRandbedingung lager)
        {
            InitializeComponent();
            _modell = modell;
            LagerId.Text = lager.RandbedingungId;
            AktuelleId = lager.RandbedingungId;
            KnotenId.Text = lager.KnotenId;
            if (lager.Festgehalten[0]) Xfest.IsChecked = true;
            if (lager.Festgehalten[1]) Yfest.IsChecked = true;
            VorX.Text = lager.Vordefiniert[0].ToString("0.00");
            VorY.Text = lager.Vordefiniert[1].ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var lagerId = LagerId.Text;

            if (lagerId == "")
            {
                _ = MessageBox.Show("Lager Id muss definiert sein", "neues Lager");
                return;
            }

            // vorhandenes Lager
            if (_modell.Randbedingungen.TryGetValue(lagerId, out var vorhandenesLager))
            {
                if (KnotenId.Text.Length > 0)
                    vorhandenesLager.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
                vorhandenesLager.Festgehalten[0] = false;
                vorhandenesLager.Festgehalten[1] = false;
                vorhandenesLager.Festgehalten[2] = false;

                if (Xfest.IsChecked != null && (bool)Xfest.IsChecked) vorhandenesLager.Festgehalten[0] = true;
                if (Yfest.IsChecked != null && (bool)Yfest.IsChecked) vorhandenesLager.Festgehalten[1] = true;
                
                try
                {
                    if (VorX.Text.Length > 0) vorhandenesLager.Vordefiniert[0] = double.Parse(VorX.Text);
                    if (VorY.Text.Length > 0) vorhandenesLager.Vordefiniert[1] = double.Parse(VorY.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neues Lager");
                    return;
                }
            }

            // neues Lager
            else
            {
                var knotenId = "";
                var conditions = 0;
                var vordefiniert = new double[3];
                if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
                if (!_modell.Knoten.TryGetValue(knotenId, out _))
                    throw new ModellAusnahme("Lagerknoten im Modell nicht vorhanden");

                try
                {
                    if (VorX.Text.Length > 0) { vordefiniert[0] = double.Parse(VorX.Text); conditions  = 1; }
                    if (VorY.Text.Length > 0) { vordefiniert[1] = double.Parse(VorY.Text); conditions += 2; }
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neues Lager");
                    return;
                }

                var lager = new Lager(KnotenId.Text, conditions, vordefiniert, _modell) { RandbedingungId = lagerId };

                lager.RandbedingungId = lagerId;
                _modell.Randbedingungen.Add(lagerId, lager);
            }

            if (AktuelleId != LagerId.Text) _modell.Randbedingungen.Remove(AktuelleId);

            Close();
            StartFenster.ElastizitätVisual.Close();
            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            StartFenster.ElastizitätVisual.IsLager = false;
        }

        private void LagerIdGotFocus(object sender, RoutedEventArgs e)
        {
            _lagerKeys = new LagerKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _lagerKeys.Show();
            _lagerKeys.Focus();
        }

        private void LagerIdLostFocus(object sender, RoutedEventArgs e)
        {
            _lagerKeys?.Close();
            if (!_modell.Randbedingungen.TryGetValue(LagerId.Text, out var vorhandenesLager)) return;

            // vorhandene Lagerdefinition
            LagerId.Text = vorhandenesLager.RandbedingungId;
            KnotenId.Text = vorhandenesLager.KnotenId;
            Xfest.IsChecked = false;
            Yfest.IsChecked = false;
            if (vorhandenesLager.Festgehalten[0]) Xfest.IsChecked = true;
            if (vorhandenesLager.Festgehalten[1]) Yfest.IsChecked = true;
            VorX.Text = vorhandenesLager.Vordefiniert[0].ToString("N2", CultureInfo.CurrentCulture);
            VorY.Text = vorhandenesLager.Vordefiniert[1].ToString("N2", CultureInfo.CurrentCulture);
        }

        private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
            {
                _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neues Lager");
                LagerId.Text = "";
                KnotenId.Text = "";
            }
            else
            {
                KnotenId.Text = vorhandenerKnoten.Id;
                if (LagerId.Text != "") return;
                LagerId.Text = "L_" + KnotenId.Text;
                AktuelleId = LagerId.Text;
            }
        }

        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (!_modell.Randbedingungen.Remove(LagerId.Text, out _)) return;
            Close();
            StartFenster.ElastizitätVisual.Close();

            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
            _modell.Berechnet = false;
        }

        private void KnotenPositionNeu(object sender, MouseButtonEventArgs e)
        {
            _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
            if (knoten == null)
            {
                _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neues Lager");
                return;
            }

            StartFenster.TragwerkVisual.KnotenEdit(knoten);
            Close();
            _modell.Berechnet = false;
        }
    }
}