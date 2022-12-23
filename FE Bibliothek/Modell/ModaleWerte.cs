namespace FEBibliothek.Modell
{
    public class ModaleWerte
    {
        public double Dämpfung { get; set; }
        public string Text { get; set; }

        public ModaleWerte(double wert)
        {
            Dämpfung = wert;
        }
        public ModaleWerte(double wert, string text)
        {
            Dämpfung = wert;
            Text = text;
        }
    }
}