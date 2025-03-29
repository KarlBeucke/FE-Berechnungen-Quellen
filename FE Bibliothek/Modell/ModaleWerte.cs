namespace FEBibliothek.Modell
{
    public class ModaleWerte(double wert, string text)
    {
        public double Dämpfung { get; set; } = wert;
        public string Text { get; set; } = text;

        public ModaleWerte(double wert) : this(wert, string.Empty)
        {
        }
    }
}