namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public class Zeitintervall
{
    public Zeitintervall(string knotenId, double zeit, double last)
    {
        KnotenId = knotenId;
        Zeit = zeit;
        Last = last;
    }

    public string KnotenId { get; set; }
    public double Zeit { get; set; }
    public double Last { get; set; }
}