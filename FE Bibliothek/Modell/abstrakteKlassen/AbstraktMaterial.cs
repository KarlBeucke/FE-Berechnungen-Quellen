namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktMaterial
    {
        public bool Feder { get; set; }
        public string MaterialId { get; set; }
        public double[] MaterialWerte { get; set; }
    }
}
