namespace FEBibliothek.Modell
{
    public class Knotenwerte(string knotenId, double[] werte)
    {
        public string KnotenId { get; set; } = knotenId;
        public double[] Werte { get; set; } = werte;
    }
}
