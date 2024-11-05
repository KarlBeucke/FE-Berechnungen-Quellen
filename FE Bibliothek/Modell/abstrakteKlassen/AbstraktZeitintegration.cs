﻿namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktZeitintegration
    {
        public string Id { get; set; }
        public double Tmax { get; set; }
        public double Dt { get; set; }
        public int Methode { get; set; }
        public bool VonStationär { get; set; }
        public double Parameter1 { get; set; }
        public double Parameter2 { get; set; }
        public List<Knotenwerte> Anfangsbedingungen { get; set; }
        //public Dictionary<string, double> Anfangsbedingungen { get; set; } = [];
    }
}
