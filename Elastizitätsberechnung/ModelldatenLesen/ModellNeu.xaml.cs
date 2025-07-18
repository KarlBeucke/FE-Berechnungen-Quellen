using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class ModellNeu
    {
        private readonly FeModell _modell;

        public ModellNeu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            Name.Text = _modell.ModellId;
            Dimension.Text = _modell.Raumdimension.ToString("D");
            Ndof.Text = _modell.AnzahlKnotenfreiheitsgrade.ToString("D");
            MinX.Text = modell.MinX.ToString("G");
            MaxX.Text = modell.MaxX.ToString("G");
            MinY.Text = modell.MinY.ToString("G");
            MaxY.Text = modell.MaxY.ToString("G");
            if (modell.Raumdimension == 3)
            {
                MinZ.Text = modell.MinZ.ToString("G");
                MaxZ.Text = modell.MaxZ.ToString("G");
            }
            else
            {
                MinZ.Text = "";
                MaxZ.Text = "";
            }


            if (modell.MaxX - modell.MinX < double.Epsilon && modell.MaxY - modell.MinY < double.Epsilon
                                               && modell.Knoten.Count > 0)
            {
                var x = new List<double>();
                var y = new List<double>();
                var z = new List<double>();

                foreach (var item in modell.Knoten)
                {
                    x.Add(item.Value.Koordinaten[0]);
                    y.Add(item.Value.Koordinaten[1]);
                    if (item.Value.Koordinaten.Length == 3) z.Add(item.Value.Koordinaten[2]);
                }

                var xMin = (int)x.Min();
                var xMax = (int)x.Max();
                var yMin = (int)y.Min();
                var yMax = (int)y.Max();
                MinX.Text = xMin.ToString("G");
                MaxX.Text = xMax.ToString("G");
                MinY.Text = yMin.ToString("G");
                MaxY.Text = yMax.ToString("G");
                if (z.Count <= 0) return;
                var zMin = (int)z.Min();
                var zMax = (int)z.Max();
                MinZ.Text = zMin.ToString("G");
                MaxZ.Text = zMax.ToString("G");
            }
            else
            {
                MinX.Text = modell.MinX.ToString("G");
                MaxX.Text = modell.MaxX.ToString("G");
                MinY.Text = modell.MinY.ToString("G");
                MaxY.Text = modell.MaxY.ToString("G");
                if (_modell.Raumdimension != 3) return;
                MinZ.Text = modell.MinZ.ToString("G");
                MaxZ.Text = modell.MaxZ.ToString("G");
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Dimension.Text, out _) || !int.TryParse(Ndof.Text, out _))
            {
                MessageBox.Show("Bitte ganzzahlige Werte für Dimension " +
                                "und Anzahl Freiheitsgrade eingeben", "Eingabefehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _modell.ModellId = Name.Text;
            _modell.Raumdimension = int.Parse(Dimension.Text);
            _modell.AnzahlKnotenfreiheitsgrade = int.Parse(Ndof.Text);

            if (!double.TryParse(MinX.Text, out _) || !double.TryParse(MaxX.Text, out _) ||
                !double.TryParse(MinY.Text, out _) || !double.TryParse(MaxY.Text, out _))
            {
                MessageBox.Show("Bitte numerische Werte für Abmessungen eingeben",
                    "Eingabefehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _modell.MinX = double.Parse(MinX.Text);
            _modell.MaxX = double.Parse(MaxX.Text);
            _modell.MinY = double.Parse(MinY.Text);
            _modell.MaxY = double.Parse(MaxY.Text);
            if (_modell.Raumdimension == 3)
            {
                _modell.MinZ = double.Parse(MinZ.Text);
                _modell.MaxZ = double.Parse(MaxZ.Text);
            }
            else
            {
                _modell.MinZ = 0.0;
                _modell.MaxZ = 0.0;
            }
            Close();
            switch (_modell.Raumdimension)
            {
                case 2:
                    StartFenster.ElastizitätVisual.Close();
                    StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
                    StartFenster.ElastizitätVisual.Show();
                    break;
                case 3:
                    StartFenster.ElastizitätVisual3D.Close();
                    StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
                    StartFenster.ElastizitätVisual3D.Show();
                    break;
            }
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}