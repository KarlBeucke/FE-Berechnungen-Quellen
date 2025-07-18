namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class LagerKeys
    {
        public LagerKeys(FeModell modell)
        {
            InitializeComponent();
            Left = 2 * Width;
            Top = Height;
            var lager = modell.Randbedingungen.Select(item => item.Value).ToList();
            LagerKey.ItemsSource = lager;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}