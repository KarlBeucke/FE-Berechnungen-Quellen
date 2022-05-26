using System;

namespace FEBibliothek.Modell
{
    public class Knoten
    {
        private double[] koordinaten;

        // Properties
        public string Id { get; }
        public int Raumdimension { get; }
        public int AnzahlKnotenfreiheitsgrade { get; }
        public double[] Knotenfreiheitsgrade { get; set; }
        public double[][] KnotenVariable { get; set; }
        public double[][] KnotenAbleitungen { get; set; }
        public double[] Reaktionen { get; set; }
        public double[] Koordinaten
        {
            get => koordinaten;
            set
            {
                koordinaten = value ?? throw new ArgumentNullException(nameof(value));

                if (koordinaten.Length == Raumdimension)
                {
                    koordinaten = new double[Raumdimension];
                }
                else
                {
                    throw new ModellAusnahme("Knoten " + Id + ": Anzahl Koordinaten nicht gleich Raumdimension");
                }
            }
        }
        public int[] SystemIndizes { get; set; }

        // ... Konstruktor ........................................................
        public Knoten(double[] crds, int ndof, int dimension)
        {
            Raumdimension = dimension;
            koordinaten = crds;
            AnzahlKnotenfreiheitsgrade = ndof;
        }
        public Knoten(string id, double[] crds, int ndof, int dimension)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Raumdimension = dimension;
            koordinaten = crds;
            AnzahlKnotenfreiheitsgrade = ndof;
        }
        public int SetzSystemIndizes(int k)
        {
            SystemIndizes = new int[AnzahlKnotenfreiheitsgrade];
            for (var i = 0; i < AnzahlKnotenfreiheitsgrade; i++)
                SystemIndizes[i] = k++;
            // liefert die inkrementierten System Indizes eines Knoten
            return k;
        }
    }
}
