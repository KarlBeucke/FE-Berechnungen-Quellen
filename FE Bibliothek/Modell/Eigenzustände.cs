namespace FEBibliothek.Modell
{
    public class Eigenzustände(string id, int anzahlZustände)
    {
        public string Id { get; set; } = id;
        public int AnzahlZustände { get; set; } = anzahlZustände;
        public double[] Eigenwerte { get; set; }
        public double[][] Eigenvektoren { get; set; }
        public List<object> DämpfungsRaten { get; set; } = [];
    }
}