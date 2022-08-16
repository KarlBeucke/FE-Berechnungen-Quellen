using System.Windows;

namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class Abstrakt2D : AbstraktElement
    {
        protected Querschnitt ElementQuerschnitt { get; set; }

        public void SetzQuerschnittReferenzen(FeModell modell)
        {
            // Elementquerschnitt für 2D Elemente
            if (ElementQuerschnittId == null) return;
            if (modell.Querschnitt.TryGetValue(ElementQuerschnittId, out var querschnitt))
            {
                ElementQuerschnitt = querschnitt;
            }
            else
            {
                var msgQuerschnitt =
                    MessageBox.Show("Querschnitt " + ElementQuerschnittId + " ist nicht im Modell enthalten.", "Abstract2D");
                _ = msgQuerschnitt;
            }
        }
        public abstract double[] BerechneElementZustand(double z0, double z1);
        public abstract Point BerechneSchwerpunkt();
    }
}
