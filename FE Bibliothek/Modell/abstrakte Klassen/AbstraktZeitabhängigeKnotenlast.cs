using System;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktZeitabhängigeKnotenlast : AbstraktKnotenlast
    {
        public bool Datei { get; set; }
        public bool Bodenanregung { get; set; }
        public int VariationsTyp { get; set; }
        public double KonstanteTemperatur { get; set; }
        public double Amplitude { get; set; }
        public double Frequenz { get; set; }
        public double PhasenWinkel { get; set; }
        public double[] Intervall { get; set; }
        public override double[] BerechneLastVektor()
        {
            throw new NotImplementedException();
        }
    }
}
