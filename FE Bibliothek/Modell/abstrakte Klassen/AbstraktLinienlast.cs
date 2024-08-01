namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktLinienlast : AbstraktElementLast
    {
        public string StartKnotenId { get; set; }
        public Knoten StartKnoten { get; set; }
        public string EndKnotenId { get; set; }
        public Knoten EndKnoten { get; set; }

        public void SetzLinienlastReferenzen(FeModell modell)
        {
            if (modell.Elemente.TryGetValue(ElementId, out var element))
            {
                Element = element;
                if (modell.Knoten.TryGetValue(element.KnotenIds[0], out var knoten))
                {
                    StartKnotenId = element.KnotenIds[0];
                    StartKnoten = knoten;
                }
                else
                {
                    throw new BerechnungAusnahme("\nLinienlastknoten " + StartKnotenId + " ist nicht im Modell enthalten.");
                }
                if (modell.Knoten.TryGetValue(element.KnotenIds[1], out knoten))
                {
                    EndKnotenId = element.KnotenIds[1];
                    EndKnoten = knoten;
                }
                else
                {
                    throw new BerechnungAusnahme("\nLinienlastknoten " + EndKnotenId + " ist nicht im Modell enthalten.");
                }
            }
            else
                throw new ModellAusnahme("\nLastreferenz: Element mit ID=" + ElementId + " ist nicht im Modell enthalten");
        }
    }
}
