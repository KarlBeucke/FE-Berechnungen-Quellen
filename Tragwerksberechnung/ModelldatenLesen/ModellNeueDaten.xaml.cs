using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class ModellNeueDaten
    {
        private readonly FeModell _modell;

        public ModellNeueDaten(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            ModellName.Text = _modell.ModellId;
            Dimension.Text = _modell.Raumdimension.ToString("D");
            Ndof.Text = _modell.AnzahlKnotenfreiheitsgrade.ToString("D");
            MinX.Text = modell.MinX.ToString("G");
            MaxX.Text = modell.MaxX.ToString("G");
            MinY.Text = modell.MinY.ToString("G");
            MaxY.Text = modell.MaxY.ToString("G");

            if (modell.MaxX - modell.MinX < double.Epsilon && modell.MaxY - modell.MinY < double.Epsilon
                                               && modell.Knoten.Count > 0)
            {
                var x = new List<double>();
                var y = new List<double>();

                foreach (var item in modell.Knoten)
                {
                    x.Add(item.Value.Koordinaten[0]);
                    y.Add(item.Value.Koordinaten[1]);
                }

                var xMin = (int)x.Min();
                var xMax = (int)x.Max();
                var yMin = (int)y.Min();
                var yMax = (int)y.Max();
                MinX.Text = xMin.ToString("G");
                MaxX.Text = xMax.ToString("G");
                MinY.Text = yMin.ToString("G");
                MaxY.Text = yMax.ToString("G");

            }
            else
            {
                MinX.Text = modell.MinX.ToString("G");
                MaxX.Text = modell.MaxX.ToString("G");
                MinY.Text = modell.MinY.ToString("G");
                MaxY.Text = modell.MaxY.ToString("G");
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Dimension.Text, out _) || !int.TryParse(Ndof.Text, out _))
            {
                MessageBox.Show("Bitte geben Sie ganzzahlige Werte ein.", "Fehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _modell.ModellId = ModellName.Text;
            _modell.Raumdimension = int.Parse(Dimension.Text);
            _modell.AnzahlKnotenfreiheitsgrade = int.Parse(Ndof.Text);

            if (!double.TryParse(MinX.Text, out _) || !double.TryParse(MaxX.Text, out _) ||
                !double.TryParse(MinY.Text, out _) || !double.TryParse(MaxY.Text, out _))
            {
                MessageBox.Show("Bitte geben Sie gültige Werte ein.", "Fehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _modell.MinX = double.Parse(MinX.Text);
            _modell.MaxX = double.Parse(MaxX.Text);
            _modell.MinY = double.Parse(MinY.Text);
            _modell.MaxY = double.Parse(MaxY.Text);
            Close();
            StartFenster.TragwerkVisual.Close();

            StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(_modell);
            StartFenster.TragwerkVisual.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}