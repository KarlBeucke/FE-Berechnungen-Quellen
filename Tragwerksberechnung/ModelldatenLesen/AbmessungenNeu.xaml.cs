using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class AbmessungenNeu
    {
        private readonly FeModell _modell;

        public AbmessungenNeu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            if (modell.MaxX - modell.MinX == 0 && modell.MaxY - modell.MinY == 0
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
                MinX.Text = xMin.ToString("D");
                MaxX.Text = xMax.ToString("D");
                MinY.Text = yMin.ToString("D");
                MaxY.Text = yMax.ToString("D");

            }
            else
            {
                MinX.Text = modell.MinX.ToString("D");
                MaxX.Text = modell.MaxX.ToString("D");
                MinY.Text = modell.MinY.ToString("D");
                MaxY.Text = modell.MaxY.ToString("D");
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            _modell.MinX = int.Parse(MinX.Text);
            _modell.MaxX = int.Parse(MaxX.Text);
            _modell.MinY = int.Parse(MinY.Text);
            _modell.MaxY = int.Parse(MaxY.Text);
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