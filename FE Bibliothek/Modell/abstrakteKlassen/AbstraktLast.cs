namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktLast
    {
        public string LastId { get; set; }
        public string KnotenId { get; set; }
        public double[] Lastwerte { get; set; }
        public abstract double[] BerechneLastVektor();
    }
}