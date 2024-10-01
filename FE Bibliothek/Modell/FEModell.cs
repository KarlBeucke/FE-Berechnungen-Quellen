namespace FEBibliothek.Modell
{
    public class FeModell(string modellId, int raumdimension, int anzahlKnotenfreiheitsgrade)
    {
        public string ModellId { get; set; } = modellId;
        public int Raumdimension { get; set; } = raumdimension;
        public int AnzahlKnotenfreiheitsgrade { get; set; } = anzahlKnotenfreiheitsgrade;
        public bool Eigen { get; set; }
        public bool ZeitIntegration { get; set; }

        public Dictionary<string, Knoten> Knoten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktElement> Elemente { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktMaterial> Material { get; set; } = [];
        public Dictionary<string, Querschnitt> Querschnitt { get; set; } = [];
        public Dictionary<string, AbstraktLast> Lasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktLinienlast> LinienLasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktElementLast> ElementLasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktElementLast> PunktLasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktRandbedingung> Randbedingungen { get; set; } = [];
        public Eigenzustände Eigenzustand { get; set; }
        public abstrakteKlassen.AbstraktZeitintegration Zeitintegration { get; set; }
        public Dictionary<string, abstrakteKlassen.AbstraktZeitabhängigeKnotenlast> ZeitabhängigeKnotenLasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktZeitabhängigeElementLast> ZeitabhängigeElementLasten { get; set; } = [];
        public Dictionary<string, abstrakteKlassen.AbstraktZeitabhängigeRandbedingung> ZeitabhängigeRandbedingung { get; set; } = [];
    }
}
