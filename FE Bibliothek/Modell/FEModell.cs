namespace FEBibliothek.Modell
{
    public class FeModell(string modellId, int raumdimension, int anzahlKnotenfreiheitsgrade)
    {
        public string ModellId { get; set; } = modellId;
        public int Raumdimension { get; set; } = raumdimension;
        public int AnzahlKnotenfreiheitsgrade { get; set; } = anzahlKnotenfreiheitsgrade;
        public bool Eigen { get; set; }
        public bool ZeitIntegration { get; set; }
        public bool ZeitintegrationDaten { get; set; }
        public bool Berechnet { get; set; }
        public bool EigenBerechnet { get; set; }
        public bool ZeitintegrationBerechnet { get; set; }
        public Eigenzustände Eigenzustand { get; set; }
        public AbstraktZeitintegration Zeitintegration { get; set; }

        public Dictionary<string, Knoten> Knoten { get; set; } = [];
        public Dictionary<string, AbstraktElement> Elemente { get; set; } = [];
        public Dictionary<string, AbstraktMaterial> Material { get; set; } = [];
        public Dictionary<string, Querschnitt> Querschnitt { get; set; } = [];
        public Dictionary<string, AbstraktLast> Lasten { get; set; } = [];
        public Dictionary<string, AbstraktLinienlast> LinienLasten { get; set; } = [];
        public Dictionary<string, AbstraktElementLast> ElementLasten { get; set; } = [];
        public Dictionary<string, AbstraktElementLast> PunktLasten { get; set; } = [];
        public Dictionary<string, AbstraktRandbedingung> Randbedingungen { get; set; } = [];
        public Dictionary<string, AbstraktZeitabhängigeKnotenlast> ZeitabhängigeKnotenLasten { get; set; } = [];
        public Dictionary<string, AbstraktZeitabhängigeElementLast> ZeitabhängigeElementLasten { get; set; } = [];
        public Dictionary<string, AbstraktZeitabhängigeRandbedingung> ZeitabhängigeRandbedingung { get; set; } = [];
    }
}
