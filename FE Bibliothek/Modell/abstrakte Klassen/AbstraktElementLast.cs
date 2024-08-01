namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktElementLast : AbstraktLast
    {
        private AbstraktElement _element;
        public string ElementId { get; set; }
        public AbstraktElement Element { get => _element; set => _element = value; }
        public bool InElementKoordinatenSystem { get; set; } = true;
        public double Offset { get; set; }

        // Ausnahmebehandlung: class "ModellAusnahme" definiert als Ableitung von class Exception mit Message (Fehler in Modelldaten:),
        // falls eine Lastreferenz fehlt, wird ModellAusnahme mit entsprechender Message (Lastreferenz fehlt) ausgeworfen
        // class "TragwerkmodelVisualisieren" prüft in try-Block, ob "ModellAusnahme" ausgeworfen wurde
        // im folgenden "catch-Block" werden die beiden Messages in einer MessageBox ausgegeben



        public void SetzElementlastReferenzen(FeModell modell)
        {
            if (modell.Elemente.TryGetValue(ElementId, out _element)) { Element = _element; }
            else throw new ModellAusnahme("\nLastreferenz: Element mit ID=" + ElementId + " ist nicht im Modell enthalten");
        }
        public bool IstInElementKoordinatenSystem() { return InElementKoordinatenSystem; }
    }
}
