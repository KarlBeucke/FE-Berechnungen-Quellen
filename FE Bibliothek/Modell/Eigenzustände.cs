using System.Collections.Generic;

namespace FEBibliothek.Modell
{
    public class Eigenzustände
    {
        public string Id { get; set; }
        public int AnzahlZustände { get; set; }
        public double[] Eigenwerte { get; set; }
        public double[][] Eigenvektoren { get; set; }
        public List<object> DämpfungsRaten { get; set; }

        public Eigenzustände(string id, int anzahlZustände)
        {
            Id = id;
            AnzahlZustände = anzahlZustände;
            DämpfungsRaten = new List<object>();
        }
    }
}