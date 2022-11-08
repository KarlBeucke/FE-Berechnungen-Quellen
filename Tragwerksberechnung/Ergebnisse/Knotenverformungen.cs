namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public class Knotenverformungen
{
    public double Zeit { get; set; }
    public string Knoten { get; set; }
    public double VerformungX { get; set; }
    public double VerformungY { get; set; }
    public double Verdrehung { get; set; }
    public double BeschleunigungX { get; set; }
    public double BeschleunigungY { get; set; }
    public double BeschleunigungPhi { get; set; }

    public Knotenverformungen(double zeit, double verformungX, double verformungY, double verdrehung,
        double beschleunigungX, double beschleunigungY, double beschleunigungPhi)
    {
        Zeit = zeit;
        VerformungX = verformungX;
        VerformungY = verformungY;
        Verdrehung = verdrehung;
        BeschleunigungX = beschleunigungX;
        BeschleunigungY = beschleunigungY;
        BeschleunigungPhi = beschleunigungPhi;
    }
    public Knotenverformungen(double zeit, double verformungX, double verformungY,
        double beschleunigungX, double beschleunigungY)
    {
        Zeit = zeit;
        VerformungX = verformungX;
        VerformungY = verformungY;
        BeschleunigungX = beschleunigungX;
        BeschleunigungY = beschleunigungY;
    }
    public Knotenverformungen(string knoten, double verformungX, double verformungY, double verdrehung,
        double beschleunigungX, double beschleunigungY, double beschleunigungPhi)
    {
        Knoten = knoten;
        VerformungX = verformungX;
        VerformungY = verformungY;
        Verdrehung = verdrehung;
        BeschleunigungX = beschleunigungX;
        BeschleunigungY = beschleunigungY;
        BeschleunigungPhi = beschleunigungPhi;
    }
    public Knotenverformungen(string knoten, double verformungX, double verformungY,
        double beschleunigungX, double beschleunigungY)
    {
        Knoten = knoten;
        VerformungX = verformungX;
        VerformungY = verformungY;
        BeschleunigungX = beschleunigungX;
        BeschleunigungY = beschleunigungY;
    }
}