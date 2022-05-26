namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktLinienlast : AbstraktElementLast
    {
        public string StartKnotenId { get; set; }
        public Knoten StartKnoten { get; set; }
        public string EndKnotenId { get; set; }
        public Knoten EndKnoten { get; set; }
    }
}
