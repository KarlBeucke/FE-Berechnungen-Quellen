using System;

namespace FEBibliothek.Modell
{
    public class Knoten
    {
        private double[] _koordinaten;

        // Properties
        public string Id { get; }
        public int Raumdimension { get; }
        public int AnzahlKnotenfreiheitsgrade { get; set; }
        public double[] Knotenfreiheitsgrade { get; set; }
        public double[][] KnotenVariable { get; set; }
        public double[][] KnotenAbleitungen { get; set; }
        public double[] Reaktionen { get; set; }
        public double[] Koordinaten
        {
            get => _koordinaten;
            set
            {
                _koordinaten = value ?? throw new ArgumentNullException(nameof(value));

                if (_koordinaten.Length == Raumdimension)
                {
                    _koordinaten = new double[Raumdimension];
                }
                else
                {
                    throw new ModellAusnahme("Knoten " + Id + ": Anzahl Koordinaten nicht gleich Raumdimension");
                }
            }
        }
        public int[] SystemIndizes { get; set; }

        public Knoten(double[] crds, int ndof, int dimension)
        {
            Raumdimension = dimension;
            _koordinaten = crds;
            AnzahlKnotenfreiheitsgrade = ndof;
        }
        public Knoten(string id, double[] crds, int ndof, int dimension)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Raumdimension = dimension;
            _koordinaten = crds;
            AnzahlKnotenfreiheitsgrade = ndof;
        }
        public int SetzSystemIndizes(int k)
        {
            SystemIndizes = new int[AnzahlKnotenfreiheitsgrade];
            for (var i = 0; i < AnzahlKnotenfreiheitsgrade; i++)
                SystemIndizes[i] = k++;
            // liefert die inkrementierten Systemindizes eines Knoten
            return k;
        }
    }
}
