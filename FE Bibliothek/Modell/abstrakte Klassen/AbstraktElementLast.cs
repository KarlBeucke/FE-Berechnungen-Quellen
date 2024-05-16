using System.Windows;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktElementLast : AbstraktLast
    {
        private AbstraktElement _element;
        public string ElementId { get; set; }
        public AbstraktElement Element { get => _element; set => _element = value; }
        public bool InElementKoordinatenSystem { get; set; } = true;
        public double Offset { get; set; }

        public void SetzElementlastReferenzen(FeModell modell)
        {
            if (modell.Elemente.TryGetValue(ElementId, out _element)) { Element = _element; }

            if (_element != null) return;
            var message = "Element mit ID=" + ElementId + " ist nicht im Modell enthalten";
            _ = MessageBox.Show(message, "AbstraktElementLast");
        }
        public bool IstInElementKoordinatenSystem() { return InElementKoordinatenSystem; }
    }
}
