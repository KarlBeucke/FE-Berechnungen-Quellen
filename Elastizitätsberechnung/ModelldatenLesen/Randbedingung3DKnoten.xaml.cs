using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Randbedingung3DKnoten
    {
        private readonly FeModell _modell;
        private const int AnzahlKnotenfreiheitsgrade = 3;

        public Randbedingung3DKnoten()
        {
            InitializeComponent();
            Show();
        }
        public Randbedingung3DKnoten(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            RandbedingungId.Text = string.Empty;
            KnotenId.Text = string.Empty;
            XFest.IsChecked = false;
            YFest.IsChecked = false;
            ZFest.IsChecked = false;
            VorX.Text = string.Empty;
            VorY.Text = string.Empty;
            VorZ.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            double[] prescribed = [0, 0, 0];
            var randbedingungId = RandbedingungId.Text;
            var knotenId = KnotenId.Text;
            var conditions = 0;
            if (XFest.IsChecked != null && (bool)XFest.IsChecked) conditions += 1;
            if (YFest.IsChecked != null && (bool)YFest.IsChecked) conditions += 2;
            if (ZFest.IsChecked != null && (bool)ZFest.IsChecked) conditions += 4;
            if (VorX.Text.Length != 0) prescribed[0] = double.Parse(VorX.Text);
            if (VorY.Text.Length != 0) prescribed[1] = double.Parse(VorZ.Text);
            if (VorZ.Text.Length != 0) prescribed[2] = double.Parse(VorX.Text);

            var randbedingung = new Lager(knotenId, conditions, prescribed, AnzahlKnotenfreiheitsgrade)
            {
                RandbedingungId = randbedingungId
            };
            _modell.Randbedingungen.Add(randbedingungId, randbedingung);

            Close();
            StartFenster.ElastizitätVisual3D.Close();
            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
