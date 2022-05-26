namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktZeitabhängigeElementLast : AbstraktElementLast
    {
        public int VariationsTyp { get; set; }
        public double[] P { get; set; }
        public abstract override double[] BerechneLastVektor();
    }
}
