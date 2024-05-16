using System.Windows;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktKnotenlast : AbstraktLast
    {
        public Knoten Knoten { get; set; }
        public int KnotenFreiheitsgrad { get; set; }

        public void SetzReferenzen(FeModell modell)
        {
            if (KnotenId == "boden") return;
            if (modell.Knoten.TryGetValue(KnotenId, out var node)) { }

            if (node != null) return;
            var message = "Knoten mit ID=" + KnotenId + " ist nicht im Modell enthalten";
            _ = MessageBox.Show(message, "AbstraktKnotenlast");
        }
    }
}
