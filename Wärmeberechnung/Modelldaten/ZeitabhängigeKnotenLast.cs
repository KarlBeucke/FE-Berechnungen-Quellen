using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten
{
    public class ZeitabhängigeKnotenLast : AbstraktZeitabhängigeKnotenlast
    {
        public ZeitabhängigeKnotenLast(string knotenId, bool datei)
        {
            KnotenId = knotenId;
            Datei = datei;
            VariationsTyp = 0;
        }
        public ZeitabhängigeKnotenLast(string knotenId, double konstanteTemperatur)
        {
            KnotenId = knotenId;
            KonstanteTemperatur = konstanteTemperatur;
            VariationsTyp = 1;
        }
        public ZeitabhängigeKnotenLast(string knotenId,
            double amplitude, double frequenz, double phasenWinkel)
        {
            KnotenId = knotenId;
            Amplitude = amplitude;
            Frequenz = frequenz;
            PhasenWinkel = phasenWinkel;
            VariationsTyp = 2;
        }
        public ZeitabhängigeKnotenLast(string knotenId, double[] intervall)
        {
            KnotenId = knotenId;
            Intervall = intervall;
            VariationsTyp = 3;
        }
    }
}
