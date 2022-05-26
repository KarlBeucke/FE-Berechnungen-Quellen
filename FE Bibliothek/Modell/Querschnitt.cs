namespace FEBibliothek.Modell
{
    public class Querschnitt
    {
        public string QuerschnittId { get; set; }
        public double[] QuerschnittsWerte { get; set; }


        public Querschnitt(double area, double ixx)
        {
            QuerschnittsWerte = new double[2];
            QuerschnittsWerte[0] = area;
            QuerschnittsWerte[1] = ixx;
        }

        public Querschnitt(double area)
        {
            QuerschnittsWerte = new double[1];
            QuerschnittsWerte[0] = area;
        }
    }
}
