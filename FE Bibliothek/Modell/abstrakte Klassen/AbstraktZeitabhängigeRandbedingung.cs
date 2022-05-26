namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktZeitabhängigeRandbedingung : AbstraktRandbedingung
    {
        public bool Datei { get; set; }
        public int VariationsTyp { get; set; }
        public double KonstanteTemperatur { get; set; }
        public double Amplitude { get; set; }
        public double Frequenz { get; set; }
        public double PhasenWinkel { get; set; }
        public double[] Intervall { get; set; }
    }
}
