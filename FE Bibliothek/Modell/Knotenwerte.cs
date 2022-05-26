namespace FEBibliothek.Modell
{
    public class Knotenwerte
    {
        public string KnotenId { get; set; }
        public double[] Werte { get; set; }

        public Knotenwerte(string knotenId, double[] werte)
        {
            KnotenId = knotenId;
            Werte = werte;
        }
    }
}
