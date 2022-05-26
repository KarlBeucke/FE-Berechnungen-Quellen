using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten
{
    public class KnotenLast : AbstraktKnotenlast
    {
        private int[] systemIndices;
        // ....Constructor....................................................
        public KnotenLast() { }
        public KnotenLast(string knotenId)
        {
            KnotenId = knotenId;
        }
        public KnotenLast(string knotenId, double[] stream)
        {
            KnotenId = knotenId;
            //Lastwerte = new double[1];
            Lastwerte = stream;
        }
        public KnotenLast(string id, string knotenId)
        {
            LastId = id;
            KnotenId = knotenId;
        }
        public KnotenLast(string id, string knotenId, double[] stream)
        {
            LastId = id;
            KnotenId = knotenId;
            //Lastwerte = new double[1];
            Lastwerte = stream;
        }
        // ....Compute the system indices of a node ..............................
        public int[] ComputeSystemIndices()
        {
            systemIndices = Knoten.SystemIndizes;
            return systemIndices;
        }
        public override double[] BerechneLastVektor()
        {
            return Lastwerte;
        }
    }
}
